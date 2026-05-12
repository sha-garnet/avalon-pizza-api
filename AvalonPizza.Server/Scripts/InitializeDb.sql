-- In the professional world, we call this SQL Scripting or Database Schema Scripting
-- Create the Database
USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PizzaStoreDb')
BEGIN
    CREATE DATABASE [PizzaStoreDb];
END
GO

USE [PizzaStoreDb];
GO

-----------------------------------------------------------------------------------------
-- Tables
-----------------------------------------------------------------------------------------
-- Tables for the Avalon Pizza application created in Order.
-----------------------------------------------------------------------------------------
GO

-- Toppings
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Toppings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Toppings] (
        [ToppingId]   INT IDENTITY(1,1) NOT NULL,
        [ToppingName] NVARCHAR(50)      NOT NULL,
        [ToppingPrice] DECIMAL(18, 2)   NOT NULL,
        [IsActive]    BIT DEFAULT 1,
        CONSTRAINT [PK_Toppings] PRIMARY KEY CLUSTERED ([ToppingId] ASC)
    );
    INSERT INTO [dbo].[Toppings] ([ToppingName], [ToppingPrice])
    VALUES ('Pepperoni', 1.50), ('Mushrooms', 1.00), ('Cheese', 0.00), ('Extra Cheese', 1.25), ('Onions', 0.75), ('Sausage', 1.50), ('Bacon', 2.00), ('Chicken', 2.00);
END
GO

-- OrderStatus
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderStatus]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderStatus] (
        [StatusId]   INT NOT NULL,
        [StatusName] NVARCHAR(50) NOT NULL,
        CONSTRAINT [PK_OrderStatus] PRIMARY KEY CLUSTERED ([StatusId] ASC)
    );
    INSERT INTO [dbo].[OrderStatus] ([StatusId], [StatusName])
    VALUES (1, 'Pending'), (2, 'In Preparation'), (3, 'Baking'), (4, 'Out for Delivery'), (5, 'Delivered'), (6, 'Cancelled');
END
GO

-- PizzaSizes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PizzaSizes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PizzaSizes] (
        [SizeId]    INT IDENTITY(1,1) NOT NULL,
        [SizeName]  NVARCHAR(50)      NOT NULL,
        [BasePrice] DECIMAL(18, 2)    NOT NULL,
        [IsActive]  BIT DEFAULT 1,
        CONSTRAINT [PK_PizzaSizes] PRIMARY KEY CLUSTERED ([SizeId] ASC)
    );
    INSERT INTO [dbo].[PizzaSizes] ([SizeName], [BasePrice])
    VALUES ('Small', 8.00), ('Medium', 10.00), ('Large', 12.00);
END
GO

-- Orders Transactional Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Orders] (
        [Id]              INT IDENTITY(1,1) NOT NULL,
        [CustomerName]    NVARCHAR (100) NOT NULL,
        [PhoneNumber]     NVARCHAR (20)  NOT NULL,
        [DeliveryAddress] NVARCHAR (500) NOT NULL,
        [SizeId]          INT NOT NULL,
        [TotalPrice]      DECIMAL(18, 2) NOT NULL,
        [StatusId]        INT NOT NULL CONSTRAINT [DF_Orders_StatusId] DEFAULT (1),
        [CreatedAt]       DATETIMEOFFSET(3) NOT NULL CONSTRAINT [DF_Orders_CreatedAt] DEFAULT ((SYSUTCDATETIME() AT TIME ZONE N'UTC')),
        [UpdatedAt]       DATETIMEOFFSET(3) NULL,
        [IsActive]        BIT NOT NULL CONSTRAINT [DF_Orders_IsActive] DEFAULT (1),
        CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Orders_Sizes] FOREIGN KEY ([SizeId]) REFERENCES [dbo].[PizzaSizes]([SizeId]),
        CONSTRAINT [FK_Orders_OrderStatus] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[OrderStatus] ([StatusId])
    );
END
GO

-- Topping Type - User-Defined Table Type (UDTT)
IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'ToppingListType' AND is_table_type = 1)
BEGIN
    CREATE TYPE [dbo].[ToppingListType] AS TABLE ([ToppingId] INT);
END

-- The Junction Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderToppings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderToppings] (
        [OrderId]   INT NOT NULL,
        [ToppingId] INT NOT NULL,
        CONSTRAINT [PK_OrderToppings] PRIMARY KEY (OrderId, ToppingId),
        CONSTRAINT [FK_OrderToppings_Orders] FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderToppings_Toppings] FOREIGN KEY (ToppingId) REFERENCES Toppings(ToppingId)
    );
END
GO

-----------------------------------------------------------------------------------------
-- STORED PROCEDURES
-----------------------------------------------------------------------------------------
-- The procedures below handle the CRUD logic for the Avalon Pizza application.
-- Ensure all related tables are created before running this section.
-----------------------------------------------------------------------------------------
GO

-- =============================================
-- Section: Toppings Stored Procedures
-- =============================================
GO

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
-- Section: PIzzaSizes Stored Procedures
-- =============================================
GO

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
-- Section: Orders Stored Procedures
-- =============================================
GO

-- Get order by Id
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

-- Create Order
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
        VALUES (
            @CustomerName,
            @PhoneNumber,
            @DeliveryAddress,
            @SizeId,
            @TotalPrice,
            1,
            1
        );

        -- Capture the New Order ID
        DECLARE @NewOrderId INT = SCOPE_IDENTITY();

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