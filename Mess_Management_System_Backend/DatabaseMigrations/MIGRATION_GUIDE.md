# ?? Database Migration Guide

## ?? Overview

This guide covers how to create and apply database migrations for the new Mess Management features.

---

## ?? Quick Start

### **Step 1: Create Migration**

```bash
cd Mess_Management_System_Backend

dotnet ef migrations add AddMessManagementFeatures --project Mess_Management_System_Backend.csproj
```

### **Step 2: Review Migration**

Check the generated migration file in `Migrations/` folder to ensure it contains:
- Attendances table
- DailyMenus table
- UserBills table
- Unique indexes

### **Step 3: Update Database**

```bash
dotnet ef database update
```

---

## ?? Expected Database Changes

### **New Tables**

#### **1. Attendances**
```sql
CREATE TABLE [Attendances] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attendances_Users_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_Attendances_UserId_Date] ON [Attendances] ([UserId], [Date]);
```

#### **2. DailyMenus**
```sql
CREATE TABLE [DailyMenus] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [BreakfastItems] nvarchar(max) NULL,
    [BreakfastPrice] decimal(18,2) NOT NULL,
    [LunchItems] nvarchar(max) NULL,
    [LunchPrice] decimal(18,2) NOT NULL,
    [DinnerItems] nvarchar(max) NULL,
    [DinnerPrice] decimal(18,2) NOT NULL,
    [DailyFixedCharge] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_DailyMenus] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_DailyMenus_Date] ON [DailyMenus] ([Date]);
```

#### **3. UserBills**
```sql
CREATE TABLE [UserBills] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [TotalFixedCharges] decimal(18,2) NOT NULL,
    [TotalFoodCharges] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [TotalDays] int NOT NULL,
    [PresentDays] int NOT NULL,
    [AbsentDays] int NOT NULL,
    [IsPaid] bit NOT NULL,
    [PaidDate] datetime2 NULL,
    [GeneratedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_UserBills] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserBills_Users_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
```

---

## ??? Troubleshooting

### **Issue: Migration Command Not Found**

**Solution:** Install EF Core Tools
```bash
dotnet tool install --global dotnet-ef
```

### **Issue: Connection String Error**

**Check:** Verify `appsettings.json` connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MessManagementDB;Trusted_Connection=True;"
  }
}
```

### **Issue: Old Tables Still Exist**

If you have old `DailyAttendance`, `MenuItem`, or `Bill` tables from before:

**Option 1: Fresh Start (Development Only)**
```bash
# Drop database and recreate
dotnet ef database drop
dotnet ef database update
```

**Option 2: Manual Cleanup**
```sql
-- Drop old tables if they exist
DROP TABLE IF EXISTS [DailyAttendance];
DROP TABLE IF EXISTS [MenuItem];
DROP TABLE IF EXISTS [Bill];
```

---

## ? Verify Migration

### **Check Tables Exist**
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

**Expected Output:**
- Attendances
- DailyMenus
- UserBills
- Users
- __EFMigrationsHistory

### **Check Indexes**
```sql
SELECT 
    i.name AS IndexName,
    t.name AS TableName,
    c.name AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.is_unique = 1
ORDER BY t.name, i.name;
```

**Expected Unique Indexes:**
- IX_Attendances_UserId_Date
- IX_DailyMenus_Date
- IX_Users_RollNumber (if not null)

---

## ?? Migration Commands Reference

| Command | Description |
|---------|-------------|
| `dotnet ef migrations add <Name>` | Create new migration |
| `dotnet ef migrations list` | List all migrations |
| `dotnet ef migrations remove` | Remove last migration (if not applied) |
| `dotnet ef database update` | Apply pending migrations |
| `dotnet ef database update <MigrationName>` | Rollback to specific migration |
| `dotnet ef database drop` | Drop database (dev only!) |
| `dotnet ef migrations script` | Generate SQL script |

---

## ?? Post-Migration Testing

### **1. Test with SQL**
```sql
-- Insert test menu
INSERT INTO DailyMenus (Date, BreakfastItems, BreakfastPrice, LunchItems, LunchPrice, 
                        DinnerItems, DinnerPrice, DailyFixedCharge, CreatedAt)
VALUES ('2024-01-15', 'Paratha, Eggs', 50, 'Biryani', 150, 'Chapati, Dal', 100, 20, GETUTCDATE());

-- Insert test attendance
INSERT INTO Attendances (UserId, Date, Status, CreatedAt)
VALUES (1, '2024-01-15', 1, GETUTCDATE());

-- Verify
SELECT * FROM DailyMenus;
SELECT * FROM Attendances;
```

### **2. Test with API**

```bash
# Start the application
dotnet run

# Test endpoints (use Postman/curl)
# POST /api/dailymenu
# POST /api/attendance
# POST /api/billing/generate
```

---

## ?? Rollback Guide

### **If Something Goes Wrong**

**Step 1: List Migrations**
```bash
dotnet ef migrations list
```

**Step 2: Rollback to Previous Migration**
```bash
dotnet ef database update <PreviousMigrationName>
```

**Step 3: Remove Bad Migration**
```bash
dotnet ef migrations remove
```

---

## ?? Additional Resources

- [Entity Framework Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core CLI Reference](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)
- [Connection Strings](https://www.connectionstrings.com/sql-server/)

---

**Last Updated:** 2024  
**Version:** 1.0
