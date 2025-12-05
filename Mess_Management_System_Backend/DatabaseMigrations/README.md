# Database Schema Update - Role Column Migration

## Summary
Changed the `Role` column in the `Users` table from `INT` to `NVARCHAR(50)` to store role as a string.

## Code Changes Made
1. ? Updated `User.cs` - Changed `Role` property from `int` to `string`
2. ? Updated `UserService.cs` - Removed enum casting, now uses string directly
3. ? Updated `Program.cs` - Fixed IPasswordHasher registration to use correct User type

## Database Migration Required
To apply the schema change to your database, you have two options:

### Option 1: Run the SQL Script (Recommended if you have existing data)
1. Open SQL Server Management Studio or your preferred SQL client
2. Connect to your database: `(localdb)\MSSQLLocalDB` - Database: `MessManagementDB`
3. Run the script: `DatabaseMigrations/ChangeRoleToString.sql`

This script will:
- Convert existing int values to strings (0?Student, 1?Teacher, 2?Admin)
- Preserve all existing data
- Update the column type to NVARCHAR(50)

### Option 2: Drop and Recreate (Only if you don't need existing data)
If you don't have important data in the database:
1. Delete the database
2. Update your DbContext or scaffolding
3. Recreate the database with the new schema

## Testing After Migration
After running the migration:
1. Build the solution: `dotnet build`
2. Run the application: `dotnet run`
3. Test user creation with role as string (e.g., "Student", "Teacher", "Admin")
4. Test authentication and verify role is returned correctly

## Valid Role Values
The application now expects role as string with these values:
- "Student"
- "Teacher"
- "Admin"

Make sure your API calls use these exact strings (case-sensitive based on your validation logic).
