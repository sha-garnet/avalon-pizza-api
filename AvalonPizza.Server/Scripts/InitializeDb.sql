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

-- Create the Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Pizzas]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Pizzas] (
        [Id]       INT             IDENTITY (1, 1) NOT NULL, -- auto-incrementing column. It starts at 1 and increases by 1 for every new row
        [Size]     NVARCHAR (50)   NOT NULL,
        [Toppings] NVARCHAR (MAX)  NOT NULL,
        [Price]    DECIMAL (18, 2) NOT NULL, -- This is standard for currency ($19.99)
        PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- Create the 'AddPizza' Stored Procedure
CREATE OR ALTER PROCEDURE [dbo].[AddPizza]
    @Size NVARCHAR(50),
    @Toppings NVARCHAR(MAX),
    @Price DECIMAL(18,2)
AS
BEGIN
    INSERT INTO Pizzas (Size, Toppings, Price)
    VALUES (@Size, @Toppings, @Price);
END
GO

-- Create the 'GetPizzas' Stored Procedure
CREATE OR ALTER PROCEDURE [dbo].[GetPizzas]
AS
BEGIN
    SELECT Id, Size, Toppings, Price FROM Pizzas;
END
GO

-- Create the 'UpdatePizza' Stored Procedure
CREATE OR ALTER PROCEDURE [dbo].[UpdatePizza]
    @Id INT,
    @Size NVARCHAR(50),
    @Toppings NVARCHAR(MAX),
    @Price DECIMAL(18,2)
AS
BEGIN
    UPDATE Pizzas 
    SET Size = @Size, Toppings = @Toppings, Price = @Price
    WHERE Id = @Id;
END
GO

-- Create the 'DeletePizza' Stored Procedure
CREATE OR ALTER PROCEDURE [dbo].[DeletePizza]
    @Id INT
AS
BEGIN
    DELETE FROM Pizzas WHERE Id = @Id;
END
GO