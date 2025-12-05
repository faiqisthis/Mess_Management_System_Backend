-- Migration: Change Role column from INT to NVARCHAR
-- This migration changes the Role column in the Users table from int to nvarchar(50)
-- Since there's no data, we can do a simple drop and add

-- Step 1: Drop the old Role column
ALTER TABLE Users DROP COLUMN Role;

-- Step 2: Add the new Role column as NVARCHAR
ALTER TABLE Users ADD Role NVARCHAR(50) NOT NULL DEFAULT 'Student';