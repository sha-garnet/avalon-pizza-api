-----------------------------------------------------------------------------------------
-- 0004_Create_StoredProcedures.sql
-- Full CRUD Stored Procedures with Business Logic and Error Handling
-----------------------------------------------------------------------------------------

-- =============================================
-- Toppings_GetAll
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_Toppings_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [ToppingId],
        [ToppingName],
        [ToppingPrice],
        [IsActive]
    FROM [dbo].[Toppings]
    WHERE [IsActive] = 1
    ORDER BY [ToppingName] ASC;
END
GO

-- =============================================
-- PizzaSizes_GetAll
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_PizzaSizes_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        [SizeId],
        [SizeName],
        [BasePrice],
        [IsActive]
    FROM [dbo].[PizzaSizes]
    WHERE [IsActive] = 1
    ORDER BY [BasePrice] ASC;
END
GO

-- =============================================
-- Orders_GetById
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_Orders_GetById]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        o.[Id],
        o.[CustomerName],
        o.[PhoneNumber],
        o.[DeliveryAddress],
        o.[SizeId],
        o.[TotalPrice],
        o.[StatusId],
        o.[CreatedAt],
        o.[UpdatedAt],
        o.[IsActive],
        s.[SizeName]
    FROM [dbo].[Orders] o
    INNER JOIN [dbo].[PizzaSizes] s ON o.[SizeId] = s.[SizeId]
    INNER JOIN [dbo].[OrderStatus] os ON o.[StatusId] = os.[StatusId]
    WHERE o.[Id] = @Id AND o.[IsActive] = 1;

    SELECT
        t.[ToppingId],
        t.[ToppingName],
        t.[ToppingPrice]
    FROM [dbo].[OrderToppings] ot
    INNER JOIN [dbo].[Toppings] t ON ot.[ToppingId] = t.[ToppingId]
    WHERE ot.[OrderId] = @Id;
END
GO

-- =============================================
-- Orders_Insert
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_Orders_Insert]
    @CustomerName    NVARCHAR(100),
    @PhoneNumber     NVARCHAR(20),
    @DeliveryAddress NVARCHAR(500),
    @SizeId          INT,
    @TotalPrice      DECIMAL(18,2),
    @Toppings        [dbo].[ToppingListType] READONLY -- Our list of IDs
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY

        -- Ensure the list of toppings is not empty
        IF NOT EXISTS (SELECT 1 FROM @Toppings)
        BEGIN
            ;THROW 50002, 'An order must contain at least one topping.', 4;
        END

        -- Validate that the new toppings actually exists in the lookup table
        IF EXISTS (SELECT 1 FROM @Toppings t WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Toppings] d WHERE d.ToppingId = t.ToppingId))
        BEGIN
            ;THROW 50003, 'One or more selected toppings do not exist.', 5;
        END

        -- In-memory container to securely capture the identity generation
        DECLARE @InsertedRows TABLE (OrderId INT);

        BEGIN TRANSACTION;

        INSERT INTO [dbo].[Orders] (
            [CustomerName],
            [PhoneNumber],
            [DeliveryAddress],
            [SizeId],
            [TotalPrice],
            [StatusId],
            [IsActive]
        )
        OUTPUT inserted.Id INTO @InsertedRows (OrderId) -- key capture
        VALUES (
            @CustomerName,
            @PhoneNumber,
            @DeliveryAddress,
            @SizeId,
            @TotalPrice,
            1,-- Pending
            1 -- Active
        );

        DECLARE @NewOrderId INT;
        SELECT TOP 1 @NewOrderId = OrderId FROM @InsertedRows;

        -- Bulk insert child topping rows using the new verified parent identifier
        INSERT INTO [dbo].[OrderToppings] ([OrderId], [ToppingId])
        SELECT @NewOrderId, [ToppingId]
        FROM @Toppings;

        COMMIT TRANSACTION;

        -- Return the ID so the API knows the Order Number
        SELECT @NewOrderId AS NewOrderId;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- Orders_Update
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_Orders_Update]
    @OrderId         INT,
    @SizeId          INT,
    @TotalPrice      DECIMAL(18,2),
    @Toppings        [dbo].[ToppingListType] READONLY -- The new list of Topping IDs
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY

        -- Ensure the list of toppings is not empty
        IF NOT EXISTS (SELECT 1 FROM @Toppings)
        BEGIN
            ;THROW 50004, 'An order must contain at least one topping.', 4;
        END

        -- Validate that the new toppings actually exists in the lookup table
        IF EXISTS (SELECT 1 FROM @Toppings t WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Toppings] d WHERE d.ToppingId = t.ToppingId))
        BEGIN
            ;THROW 50005, 'One or more selected toppings do not exist.', 5;
        END

        BEGIN TRANSACTION;

        -- Update the main Order details
        UPDATE [dbo].[Orders]
        SET [SizeId] = @SizeId,
            [TotalPrice] = @TotalPrice,
            [UpdatedAt] = SYSUTCDATETIME() AT TIME ZONE 'UTC'
        WHERE [Id] = @OrderId 
            AND [IsActive] = 1
            AND [StatusId] = 1;

        -- Check if the order actually exists
        IF @@ROWCOUNT = 0
        BEGIN
            -- We check if it exists at all to give a better error message
            -- Use UPDLOCK to prevent rare concurrent state modification gaps during inspection (prevent unlikely rece condition)
            IF EXISTS(SELECT 1 FROM [dbo].[Orders] WITH (UPDLOCK) WHERE [Id] = @OrderId AND [StatusId] <> 1)
            BEGIN
                ;THROW 50006, 'Order can only be modified while in Pending status.', 2;
            END
            ELSE
            BEGIN
                ;THROW 50007, 'Order not found or inactive.', 1;
            END
        END

        -- Sync Toppings (The "Delete and Re-insert" strategy)
        -- This is the cleanest way to handle a Junction Table update
        -- Remove existing toppings for this order
        DELETE FROM [dbo].[OrderToppings] 
        WHERE [OrderId] = @OrderId;

        -- Insert the new set of toppings from the User-Defined Table Type
        INSERT INTO [dbo].[OrderToppings] ([OrderId], [ToppingId])
        SELECT @OrderId, [ToppingId]
        FROM @Toppings;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- Orders_UpdateStatus
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_Orders_UpdateStatus]
    @OrderId  INT,
    @StatusId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate that the new StatusId actually exists in the lookup table
    IF NOT EXISTS (SELECT 1 FROM [dbo].[OrderStatus] WHERE [StatusId] = @StatusId)
    BEGIN
        ;THROW 50008, 'The provided Status ID is invalid.', 3;
    END

    UPDATE [dbo].[Orders]
    SET [StatusId] = @StatusId,
        [UpdatedAt] = SYSUTCDATETIME() AT TIME ZONE 'UTC'
    WHERE [Id] = @OrderId AND [IsActive] = 1;

    IF @@ROWCOUNT = 0
    BEGIN
        ;THROW 50009, 'Order not found or inactive.', 1;
    END
END
GO

-- =============================================
-- Orders_Delete
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_Orders_Delete]
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- We only update IsActive to 0 if the status is Pending (1)
        UPDATE [dbo].[Orders]
        SET [IsActive] = 0,
            [UpdatedAt] = SYSUTCDATETIME() AT TIME ZONE 'UTC'
        WHERE [Id] = @OrderId 
          AND [IsActive] = 1 
          AND [StatusId] = 1;

        -- If the update failed, find out if it was the ID or the Status
        IF @@ROWCOUNT = 0
        BEGIN
            IF EXISTS(SELECT 1 FROM [dbo].[Orders] WITH (UPDLOCK) WHERE [Id] = @OrderId AND [StatusId] <> 1)
            BEGIN
                ;THROW 50010, 'Order can only be modified while in Pending status.', 2;
            END
            ELSE
            BEGIN
                ;THROW 50011, 'Order not found or inactive.', 1;
            END
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO