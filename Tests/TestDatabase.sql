-- Test Database Schema for MyBatis.NET Dynamic SQL Tests
-- Run this script to create test database

USE master;
GO

-- Create test database if not exists
IF NOT EXISTS (SELECT *
FROM sys.databases
WHERE name = 'MyBatisTestDB')
BEGIN
    CREATE DATABASE MyBatisTestDB;
END
GO

USE MyBatisTestDB;
GO

-- Drop tables if exist
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

-- Create Users table
CREATE TABLE Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200),
    Age INT,
    Role NVARCHAR(50),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    DeletedDate DATETIME2 NULL
);
GO

-- Insert test data
INSERT INTO Users
    (UserName, Email, Age, Role, IsActive, CreatedDate)
VALUES
    ('john_doe', 'john@example.com', 25, 'User', 1, GETDATE()),
    ('jane_smith', 'jane@example.com', 30, 'Admin', 1, GETDATE()),
    ('bob_wilson', 'bob@example.com', 18, 'User', 1, GETDATE()),
    ('alice_jones', 'alice@example.com', 35, 'Manager', 1, GETDATE()),
    ('charlie_brown', 'charlie@example.com', 22, 'User', 0, GETDATE()),
    ('david_miller', 'david@example.com', 45, 'Admin', 1, GETDATE()),
    ('emma_davis', 'emma@example.com', 28, 'User', 1, GETDATE()),
    ('frank_garcia', 'frank@example.com', 40, 'Manager', 1, GETDATE()),
    ('grace_martinez', 'grace@example.com', 19, 'User', 1, GETDATE()),
    ('henry_rodriguez', NULL, 50, 'Admin', 1, GETDATE());
GO

-- Add one deleted user
UPDATE Users SET DeletedDate = GETDATE(), IsActive = 0 WHERE UserName = 'charlie_brown';
GO

SELECT *
FROM Users;
GO
