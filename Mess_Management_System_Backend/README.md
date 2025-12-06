# ??? Mess Management System - Backend API

**A comprehensive ASP.NET Core 8 Web API for managing mess (dining hall) operations with attendance tracking, menu management, and automated billing.**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Build](https://img.shields.io/badge/build-passing-brightgreen)](.)
[![License](https://img.shields.io/badge/license-MIT-blue)](.)

---

## ?? Table of Contents
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Quick Start](#-quick-start)
- [API Documentation](#-api-documentation)
- [Database Schema](#-database-schema)
- [Billing Logic](#-billing-logic)
- [Architecture](#-architecture)
- [Security](#-security)
- [Migration Guide](#-migration-guide)
- [Troubleshooting](#-troubleshooting)

---

## ? Features

### **User Management**
- ? JWT-based authentication & authorization
- ? Role-based access control (Student, Teacher, Admin)
- ? Password hashing with PBKDF2
- ? User CRUD operations

### **Attendance Management**
- ? Daily attendance tracking (Present/Absent)
- ? **Bulk attendance marking** - mark entire class at once
- ? **Student attendance summary with menu details** - students can estimate their bill
- ? Attendance history with date filtering
- ? One attendance per user per day (enforced)

### **Menu Management**
- ? Daily menu creation with **flexible meal structure**
- ? **Meals stored as JSON array** (Breakfast, Lunch, Dinner)
- ? Fixed daily charges (tea + water)
- ? One menu per date (enforced)

### **Billing System**
- ? **Monthly bill generation (Admin only)**
- ? **Bills generated ONLY for previous months** (not current month)
- ? **Fixed charges for ALL days**
- ? **Food charges for PRESENT days only**
- ? Students can view their bills
- ? Payment tracking
- ? Duplicate bill prevention

---

## ??? Tech Stack

| Technology | Purpose |
|------------|---------|
| **.NET 8** | Framework |
| **ASP.NET Core Web API** | REST API |
| **Entity Framework Core** | ORM |
| **SQL Server** | Database |
| **JWT** | Authentication |
| **ASP.NET Identity** | Password Hashing |
| **System.Text.Json** | JSON Serialization |

---

## ?? Quick Start

### **Prerequisites**
- .NET 8 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

### **1. Clone Repository**
```bash
git clone https://github.com/faiqisthis/Mess_Management_System_Backend.git
cd Mess_Management_System_Backend/Mess_Management_System_Backend
```

### **2. Configure Database**
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MessManagementDB;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong",
    "Issuer": "MessManagementSystem",
    "Audience": "MessManagementClients",
    "ExpiryInMinutes": 60
  }
}
```

### **3. Run Migrations**
```bash
dotnet ef database update
```

### **4. Run Application**
```bash
dotnet run
```

API will be available at:
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:5001`

### **5. Test the API**

#### Register a New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "User",
  "email": "test@example.com",
  "password": "Test@123",
  "role": 0,
  "rollNumber": "CS2024001",
  "roomNumber": "A-101",
  "contactNumber": "+1234567890"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Test@123"
}
```

#### Use the Token
```http
GET /api/users/1
Authorization: Bearer <your-token-here>
```

---

## ?? API Documentation

### **Authentication Endpoints**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/register` | POST | None | Register new user |
| `/api/auth/login` | POST | None | User login |

#### **Register User**
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "Password123",
  "role": 0,
  "rollNumber": "CS2024001",
  "roomNumber": "A-101",
  "contactNumber": "+1234567890"
}
```

**Roles:** `0 = Student, 1 = Teacher, 2 = Admin`

**Response (Success - 201 Created):**
```json
{
  "message": "User registered successfully",
  "userId": 5,
  "email": "john@example.com",
  "role": "Student"
}
```

**Response (Error - 400 Bad Request):**
```json
{
  "message": "Email already exists."
}
```

**Validation:**
- ? Email must be unique
- ? Roll number must be unique (if provided)
- ? Password is required (minimum 8 characters recommended)
- ? Email format validation

#### **Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "Password123"
}
```

**Response (Success - 200 OK):**
```json
{
  "userId": 5,
  "email": "john@example.com",
  "role": "Student",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (Error - 401 Unauthorized):**
```json
{
  "message": "Invalid email or password"
}
```

---

### **User Management Endpoints**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/users` | POST | Admin | Create new user |
| `/api/users` | GET | Admin | Get all users |
| `/api/users/{id}` | GET | Authenticated | Get user by ID |
| `/api/users/{id}` | PUT | Admin | Update user (supports partial updates) |
| `/api/users/{id}/change-password` | POST | User/Admin | Change password |
| `/api/users/{id}` | DELETE | Admin | Delete user |

#### **Create User (Admin Only)**
```http
POST /api/users
Authorization: Bearer <admin-token>

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "Password123",
  "role": 0,
  "rollNumber": "CS2024001",
  "roomNumber": "A-101",
  "contactNumber": "+1234567890"
}
```

**Required Fields:** `firstName`, `lastName`, `email`, `password`

#### **Update User (Admin Only - Partial Updates Supported)** ?
```http
PUT /api/users/5
Authorization: Bearer <admin-token>

{
  "lastName": "Smith"
}
```

**? Partial Update Support:**
- You can update **any combination** of fields
- Only fields present in the request will be updated
- Missing fields will **not** be changed
- No required fields - update only what you need!

**Examples:**

Update only last name:
```json
{
  "lastName": "Smith"
}
```

Update email and room number:
```json
{
  "email": "newemail@example.com",
  "roomNumber": "B-202"
}
```

Update multiple fields:
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "role": 1,
  "isActive": true
}
```

**Available Fields for Update:**
- `firstName` - User's first name
- `lastName` - User's last name
- `email` - User's email (must be unique)
- `role` - User role (0=Student, 1=Teacher, 2=Admin)
- `isActive` - Account active status
- `rollNumber` - Student roll number (must be unique if provided)
- `roomNumber` - Room number
- `contactNumber` - Contact number

**Note:** Password updates must use the `/change-password` endpoint.

---

### **Attendance Endpoints**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/attendance` | POST | Admin/Teacher | Mark single attendance |
| `/api/attendance/bulk` | POST | Admin/Teacher | **Mark multiple attendances** |
| `/api/attendance/{id}` | PUT | Admin/Teacher | Update attendance |
| `/api/attendance/date/{date}` | GET | Admin/Teacher | Get all attendance for date |
| `/api/attendance/user/{userId}` | GET | Authenticated | **Get user attendance with flexible filters** |
| `/api/attendance/{id}` | DELETE | Admin | Delete attendance |

#### **Mark Single Attendance**
```http
POST /api/attendance
Authorization: Bearer <token>

{
  "userId": 5,
  "date": "2024-12-06",
  "status": 1
}
```

**Status:** `0 = Absent, 1 = Present`

#### **Mark Bulk Attendance** ?? NEW
```http
POST /api/attendance/bulk
Authorization: Bearer <token>

{
  "date": "2024-12-06",
  "attendances": [
    { "userId": 1, "status": 1 },
    { "userId": 2, "status": 1 },
    { "userId": 3, "status": 0 },
    { "userId": 4, "status": 1 }
  ]
}
```

**Benefits:**
- ? One API call instead of multiple
- ? Faster performance
- ? Transactional (all succeed or all fail)
- ? Perfect for marking entire class at once

#### **Get User Attendance (Flexible Filters)** ?? IMPROVED

This **single endpoint** supports multiple query modes through query parameters:

**Query Parameters:**
- `startDate` & `endDate` - Filter by date range
- `year` & `month` - Filter by specific month (alternative to date range)
- `includeSummary` - Include summary statistics (boolean)
- `includeMenuDetails` - Include daily menu details and cost breakdown (boolean)

**Example 1: Simple Attendance List**
```http
GET /api/attendance/user/5?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer <token>
```

**Response:**
```json
[
  {
    "id": 1,
    "userId": 5,
    "date": "2024-01-01T00:00:00Z",
    "status": 1,
    "createdAt": "2024-01-01T08:00:00Z"
  },
  {
    "id": 2,
    "userId": 5,
    "date": "2024-01-02T00:00:00Z",
    "status": 0,
    "createdAt": "2024-01-02T08:00:00Z"
  }
]
```

**Example 2: Monthly Attendance with Summary**
```http
GET /api/attendance/user/5?year=2024&month=12&includeSummary=true
Authorization: Bearer <token>
```

**Response:**
```json
{
  "userId": 5,
  "startDate": "2024-12-01",
  "endDate": "2024-12-31",
  "year": 2024,
  "month": 12,
  "monthName": "December 2024",
  "attendances": [
    {
      "id": 1,
      "userId": 5,
      "date": "2024-12-01T00:00:00Z",
      "status": 1
    }
  ],
  "summary": {
    "totalDays": 31,
    "presentDays": 25,
    "absentDays": 5,
    "daysWithoutAttendance": 1,
    "estimatedFixedCharges": 620,
    "estimatedFoodCharges": 7500,
    "estimatedTotalCost": 8120
  }
}
```

**Example 3: Full Details with Menu & Cost Breakdown** ?? **BEST FOR STUDENTS**
```http
GET /api/attendance/user/5?year=2024&month=12&includeSummary=true&includeMenuDetails=true
Authorization: Bearer <token>
```

**Response:**
```json
{
  "userId": 5,
  "startDate": "2024-12-01",
  "endDate": "2024-12-31",
  "year": 2024,
  "month": 12,
  "monthName": "December 2024",
  "dailySummaries": [
    {
      "date": "2024-12-01",
      "status": "Present",
      "meals": [
        {"name": "Paratha, Eggs", "type": 0, "price": 50},
        {"name": "Biryani", "type": 1, "price": 150},
        {"name": "Chapati, Dal", "type": 2, "price": 100}
      ],
      "dailyFixedCharge": 20,
      "dailyFoodCost": 300,
      "dailyTotalCost": 320,
      "menuAvailable": true
    },
    {
      "date": "2024-12-02",
      "status": "Absent",
      "meals": [...],
      "dailyFixedCharge": 20,
      "dailyFoodCost": 280,
      "dailyTotalCost": 20,
      "menuAvailable": true
    },
    {
      "date": "2024-12-03",
      "status": "NotMarked",
      "meals": [],
      "dailyFixedCharge": 20,
      "dailyFoodCost": 0,
      "dailyTotalCost": 20,
      "menuAvailable": false
    }
  ],
  "summary": {
    "totalDays": 31,
    "presentDays": 25,
    "absentDays": 5,
    "daysWithoutAttendance": 1,
    "estimatedFixedCharges": 620,
    "estimatedFoodCharges": 7500,
    "estimatedTotalCost": 8120
  },
  "note": "This is an estimated cost based on your attendance. Actual bill may vary and will be generated by admin at the start of next month."
}
```

**Benefits:**
- ? **One flexible endpoint** instead of multiple separate endpoints
- ? **RESTful design** - use query parameters for filtering
- ? **Backward compatible** - simple queries work without breaking changes
- ? **Granular control** - clients choose what data they need
- ? Students can estimate their bill before official generation
- ? Clear breakdown: fixed charges vs food charges
- ? No surprises when official bill is generated

---

### **Menu Endpoints**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/dailymenu` | POST | Admin | Create menu |
| `/api/dailymenu/{id}` | PUT | Admin | Update menu |
| `/api/dailymenu/date/{date}` | GET | Public | Get menu by date |
| `/api/dailymenu/range` | GET | Public | Get menus in range |
| `/api/dailymenu/{id}` | DELETE | Admin | Delete menu |

#### **Create Menu** ? NEW STRUCTURE
```http
POST /api/dailymenu
Authorization: Bearer <admin-token>

{
  "date": "2024-12-06",
  "meals": [
    { "name": "Paratha, Eggs", "type": 0, "price": 50 },
    { "name": "Biryani", "type": 1, "price": 150 },
    { "name": "Chapati, Dal", "type": 2, "price": 100 }
  ],
  "dailyFixedCharge": 20
}
```

**Meal Types:** `0 = Breakfast, 1 = Lunch, 2 = Dinner`

**Benefits:**
- ? Array structure - easy to map/destructure in frontend
- ? Flexible - can have multiple items per meal type
- ? JSON storage in database
- ? Standard JavaScript array methods

---

### **Billing Endpoints**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/billing/generate` | POST | **Admin Only** | Generate monthly bill (previous months only) |
| `/api/billing/{id}` | GET | User/Admin | Get bill by ID |
| `/api/billing/user/{userId}` | GET | User/Admin | Get user's bills |
| `/api/billing` | GET | Admin | View all bills |
| `/api/billing/{id}/pay` | PUT | Admin | Mark bill as paid |
| `/api/billing/{id}` | DELETE | Admin | Delete bill |

#### **Generate Monthly Bill** ?? UPDATED
```http
POST /api/billing/generate
Authorization: Bearer <admin-token>

{
  "userId": 5,
  "year": 2024,
  "month": 11
}
```

**?? Important Rules:**
- Bills can **ONLY** be generated for **previous months**
- Cannot generate bill for current month
- Prevents duplicate bills for same month
- Year must be between 2000 and current year

**Example Validation:**
```
? Valid:   December 2024 ? Generate November 2024 bill
? Valid:   January 2025  ? Generate December 2024 bill
? Invalid: December 2024 ? Generate December 2024 bill (current month)
? Invalid: November 2024 ? Generate November 2024 bill (already exists)
```

**Response:**
```json
{
  "id": 1,
  "userId": 5,
  "startDate": "2024-11-01T00:00:00Z",
  "endDate": "2024-11-30T00:00:00Z",
  "totalFixedCharges": 600,
  "totalFoodCharges": 6300,
  "totalAmount": 6900,
  "totalDays": 30,
  "presentDays": 21,
  "absentDays": 9,
  "isPaid": false,
  "generatedDate": "2024-12-01T10:00:00Z"
}
```

---

## ?? Database Schema

### **Tables Overview**

```
Users
??? Attendances (FK: UserId)
??? UserBills (FK: UserId)

DailyMenus (standalone)
```


### **Users Table**
```sql
CREATE TABLE Users (
    Id int PRIMARY KEY IDENTITY,
    FirstName nvarchar(100) NOT NULL,
    LastName nvarchar(100) NOT NULL,
    Email nvarchar(max) NOT NULL,
    PasswordHash nvarchar(max) NOT NULL,
    Role int NOT NULL,
    IsActive bit NOT NULL,
    CreatedAt datetime2 NOT NULL,
    RollNumber nvarchar(450) NULL UNIQUE,
    RoomNumber nvarchar(max) NULL,
    ContactNumber nvarchar(max) NULL
);
```

### **Attendances Table**
```sql
CREATE TABLE Attendances (
    Id int PRIMARY KEY IDENTITY,
    UserId int NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    Date datetime2 NOT NULL,
    Status int NOT NULL,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NULL,
    CONSTRAINT UQ_Attendance UNIQUE (UserId, Date)
);
```

### **DailyMenus Table** ? NEW STRUCTURE
```sql
CREATE TABLE DailyMenus (
    Id int PRIMARY KEY IDENTITY,
    Date datetime2 NOT NULL UNIQUE,
    Meals nvarchar(max) NOT NULL,  -- JSON array
    DailyFixedCharge decimal(18,2) NOT NULL,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NULL
);
```

**Example JSON stored in Meals:**
```json
[
  {"name":"Paratha, Eggs","type":0,"price":50},
  {"name":"Biryani","type":1,"price":150},
  {"name":"Chapati, Dal","type":2,"price":100}
]
```

### **UserBills Table**
```sql
CREATE TABLE UserBills (
    Id int PRIMARY KEY IDENTITY,
    UserId int NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    StartDate datetime2 NOT NULL,
    EndDate datetime2 NOT NULL,
    TotalFixedCharges decimal(18,2) NOT NULL,
    TotalFoodCharges decimal(18,2) NOT NULL,
    TotalAmount decimal(18,2) NOT NULL,
    TotalDays int NOT NULL,
    PresentDays int NOT NULL,
    AbsentDays int NOT NULL,
    IsPaid bit NOT NULL,
    PaidDate datetime2 NULL,
    GeneratedDate datetime2 NOT NULL
);
```

---

## ?? Billing Logic

### **How Bills are Calculated**

```
Total Bill = Fixed Charges + Food Charges

Fixed Charges = Daily Fixed Charge × Total Days in Month
              = (Tea + Water) × ALL days (even absent days)

Food Charges = Sum of meal prices for PRESENT days only
             = ?(Breakfast + Lunch + Dinner) for each present day
```

### **Example Calculation**

**Scenario:**
- Period: November 2024 (30 days)
- Daily Fixed Charge: ?20
- Average Daily Food Cost: ?300 (?50 + ?150 + ?100)
- User was Present: 25 days
- User was Absent: 5 days

**Calculation:**
```
Fixed Charges = ?20 × 30 days = ?600 (ALL days)
Food Charges = ?300 × 25 days = ?7,500 (PRESENT days only)
Total Bill = ?600 + ?7,500 = ?8,100
```

**Key Point:** ?? Fixed charge applies even on absent days (tea & water costs are fixed)

### **Bill Generation Timeline**

```
November 2024:
  - Attendance marked daily
  - Students can view estimated costs anytime
  - Bill CANNOT be generated yet

December 1, 2024:
  - Admin generates November bill
  - Student receives official bill
  - Student compares with their estimated cost
  - Payment process begins
```

---

## ??? Architecture

### **Project Structure**
```
Mess_Management_System_Backend/
??? Controllers/
?   ??? AuthController.cs
?   ??? UsersController.cs
?   ??? AttendanceController.cs
?   ??? DailyMenuController.cs
?   ??? BillingController.cs
??? Data/
?   ??? ApplicationDbContext.cs
??? Models/
?   ??? User.cs
?   ??? Attendance.cs
?   ??? DailyMenu.cs
?   ??? UserBill.cs
??? Services/
?   ??? IUserService.cs / UserService.cs
?   ??? IAttendanceService.cs / AttendanceService.cs
?   ??? IDailyMenuService.cs / DailyMenuService.cs
?   ??? IBillingService.cs / BillingService.cs
?   ??? IJwtService.js / JwtService.cs
??? Migrations/
??? Program.cs
```

### **Middleware Pipeline**
```
Request
  ?
[1] HTTPS Redirection
  ?
[2] CORS
  ?
[3] Authentication (JWT)
  ?
[4] Authorization (Roles)
  ?
[5] User Ownership Validation
  ?
[6] Controllers
  ?
Response
```

### **Security Layers**

1. **Authentication Middleware** - Validates JWT token
2. **Authorization Middleware** - Checks role permissions
3. **Ownership Validation** - Ensures users can only access their own resources
4. **Service Layer** - Business logic validation

---

## ?? Security

### **Authentication**
- JWT tokens with HS256 signing
- Token expiry (configurable, default 60 minutes)
- Claims-based identity (User ID, Email, Role)

### **Authorization**
- Role-based: Student, Teacher, Admin
- Ownership validation for personal resources
- Admin bypass for management operations

### **Password Security**
- PBKDF2 hashing with salt
- Passwords never returned in responses
- Minimum complexity requirements

### **Role Permissions**

| Role | Permissions |
|------|-------------|
| **Student** | View own profile, change own password, **view attendance summary with estimated costs**, view own bills |
| **Teacher** | Student permissions + mark attendance, view menus, view anyone's attendance summary |
| **Admin** | All permissions + user management, **generate monthly bills**, billing management, menu management |

---

## ?? Migration Guide

### **Create Migration**
```bash
cd Mess_Management_System_Backend
dotnet ef migrations add AddMessManagementFeatures
```

### **Apply Migration**
```bash
dotnet ef database update
```

### **Drop Database (Dev Only)**
```bash
dotnet ef database drop --force
dotnet ef database update
```

### **Verify Migration**
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

**Expected Tables:**
- ? Users
- ? Attendances
- ? DailyMenus
- ? UserBills
- ? __EFMigrationsHistory

### **Check Unique Indexes**
```sql
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.is_unique AS IsUnique
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.is_unique = 1 AND i.name IS NOT NULL
ORDER BY t.name;
```

**Expected Unique Indexes:**
- ? IX_Attendances_UserId_Date
- ? IX_DailyMenus_Date
- ? IX_Users_RollNumber

---

## ?? Complete Workflow Example

### **1. Admin Creates Menu**
```http
POST /api/dailymenu
Authorization: Bearer <admin-token>

{
  "date": "2024-12-06",
  "meals": [
    { "name": "Paratha, Eggs", "type": 0, "price": 50 },
    { "name": "Chicken Biryani", "type": 1, "price": 150 },
    { "name": "Chapati, Dal", "type": 2, "price": 100 }
  ],
  "dailyFixedCharge": 20
}
```

### **2. Teacher Marks Attendance (Bulk)**
```http
POST /api/attendance/bulk
Authorization: Bearer <teacher-token>

{
  "date": "2024-12-06",
  "attendances": [
    { "userId": 1, "status": 1 },
    { "userId": 2, "status": 1 },
    { "userId": 3, "status": 0 },
    { "userId": 4, "status": 1 }
  ]
}
```

### **3. Student Checks Estimated Cost (Anytime During Month)** ?? IMPROVED
```http
GET /api/attendance/user/1?year=2024&month=12&includeSummary=true&includeMenuDetails=true
Authorization: Bearer <student-token>
```

**Student sees:**
- Daily attendance status
- Menu for each day
- Cost breakdown
- Estimated monthly total

### **4. Admin Generates Monthly Bill (Start of Next Month)** ?? UPDATED
```http
POST /api/billing/generate
Authorization: Bearer <admin-token>

{
  "userId": 1,
  "year": 2024,
  "month": 11
}
```

### **5. Student Views Their Official Bill**
```http
GET /api/billing/user/1
Authorization: Bearer <student-token>
```

### **6. Admin Marks Bill as Paid**
```http
PUT /api/billing/1/pay
Authorization: Bearer <admin-token>
```

---

## ?? Troubleshooting

### **Database Connection Error**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MessManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Check SQL Server is running:
```bash
sqllocaldb info
sqllocaldb start MSSQLLocalDB
```

### **Migration Command Not Found**
```bash
dotnet tool install --global dotnet-ef
```

### **Port Already in Use**
```bash
dotnet run --urls "http://localhost:5555"
```

Or edit `Properties/launchSettings.json`

### **JWT Token Invalid**
- Check token expiry
- Verify JWT secret key in appsettings.json
- Ensure Authorization header format: `Bearer <token>`

---

## ?? Dependencies

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity" Version="2.2.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
```

---

## ?? Recent Updates

### **v2.2 (December 2024)** ?? LATEST
- ? **Consolidated attendance API** - Single flexible endpoint with query parameters
- ? **RESTful design improvement** - `/api/attendance/user/{userId}` supports all filtering needs
- ? **Reduced API surface** - One endpoint replaces two separate endpoints
- ? **Backward compatible** - Simple queries still work without breaking changes
- ? **Query parameter filters** - `startDate`, `endDate`, `year`, `month`, `includeSummary`, `includeMenuDetails`

### **v2.1 (December 2024)** ?? NEW
- ?? **Monthly bill generation restriction** - bills only for previous months
- ?? **Student attendance summary** - view daily attendance with menu & estimated costs
- ?? **Improved billing workflow** - clear separation between estimation and official billing
- ?? **Duplicate bill prevention** - automatic validation
