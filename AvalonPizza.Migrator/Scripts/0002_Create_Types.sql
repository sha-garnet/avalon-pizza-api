-----------------------------------------------------------------------------------------
-- 0002_Create_Types.sql
-- Defines User-Defined Table Types (UDTTs) used for efficient passing of data 
-- to stored procedures.
-----------------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.types WHERE name = 'ToppingListType' AND is_table_type = 1)
BEGIN
    CREATE TYPE [dbo].[ToppingListType] AS TABLE (
        [ToppingId] INT
    );
END
GO