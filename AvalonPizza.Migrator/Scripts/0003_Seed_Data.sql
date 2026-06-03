-----------------------------------------------------------------------------------------
-- 0003_Seed_Data.sql
-- Populates lookup tables with initial required data.
-----------------------------------------------------------------------------------------

-- Seed Toppings
IF NOT EXISTS (SELECT 1 FROM [dbo].[Toppings])
BEGIN
    INSERT INTO [dbo].[Toppings] ([ToppingName], [ToppingPrice])
    VALUES 
    ('Pepperoni', 1.50), 
    ('Mushrooms', 1.00), 
    ('Cheese', 0.00), 
    ('Extra Cheese', 1.25), 
    ('Onions', 0.75), 
    ('Sausage', 1.50), 
    ('Bacon', 2.00), 
    ('Chicken', 2.00);
END
GO

-- Seed OrderStatus
IF NOT EXISTS (SELECT 1 FROM [dbo].[OrderStatus])
BEGIN
    INSERT INTO [dbo].[OrderStatus] ([StatusId], [StatusName])
    VALUES 
    (1, 'Pending'), 
    (2, 'In Preparation'), 
    (3, 'Baking'), 
    (4, 'Out for Delivery'), 
    (5, 'Delivered'), 
    (6, 'Cancelled');
END
GO

-- Seed PizzaSizes
IF NOT EXISTS (SELECT 1 FROM [dbo].[PizzaSizes])
BEGIN
    INSERT INTO [dbo].[PizzaSizes] ([SizeName], [BasePrice])
    VALUES 
    ('Small', 8.00), 
    ('Medium', 10.00), 
    ('Large', 12.00);
END
GO