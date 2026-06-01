-----------------------------------------------------------------------------------------
-- 0001_Create_Tables.sql
-- Creates core schema: Toppings, OrderStatus, PizzaSizes, Orders, and OrderToppings.
-----------------------------------------------------------------------------------------

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

-- The Junction Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderToppings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderToppings] (
        [OrderId]   INT NOT NULL,
        [ToppingId] INT NOT NULL,
        CONSTRAINT [PK_OrderToppings] PRIMARY KEY CLUSTERED (OrderId ASC, ToppingId ASC),
        CONSTRAINT [FK_OrderToppings_Orders] FOREIGN KEY (OrderId) REFERENCES [dbo].[Orders]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderToppings_Toppings] FOREIGN KEY (ToppingId) REFERENCES [dbo].[Toppings]([ToppingId])
    );
END
GO