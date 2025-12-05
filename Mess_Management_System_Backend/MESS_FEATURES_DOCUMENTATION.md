# ??? Mess Management Features - Complete API Documentation

## ?? Table of Contents
1. [Feature Overview](#feature-overview)
2. [Attendance Management](#attendance-management)
3. [Daily Menu Management](#daily-menu-management)
4. [Billing System](#billing-system)
5. [API Reference](#api-reference)
6. [Business Logic](#business-logic)
7. [Examples & Use Cases](#examples--use-cases)

---

## ? Feature Overview

### **1. Attendance Management**
- Mark daily attendance (Present/Absent)
- One attendance record per user per day
- Track attendance history
- Admin/Teacher can manage attendance

### **2. Daily Menu Management**
- Create menu for each day
- Support for Breakfast, Lunch, Dinner
- Fixed daily charges (tea + water)
- One menu per date

### **3. Billing System**
- **Fixed Charges**: Applied to ALL users (tea + water) - regardless of attendance
- **Food Charges**: Only applied to users marked **Present**
- Generate bills for specific periods
- Track paid/unpaid status

---

## ?? Attendance Management

### **Models**

```csharp
public class Attendance
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }  // Present = 1, Absent = 0
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum AttendanceStatus
{
    Absent = 0,
    Present = 1
}
```

### **Business Rules**
- ? One attendance record per user per day (enforced by unique index)
- ? Only Admin/Teacher can mark/update attendance
- ? Users can view their own attendance
- ? Date is normalized (time component removed)

### **API Endpoints**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/attendance` | POST | Admin/Teacher | Mark attendance |
| `/api/attendance/{id}` | PUT | Admin/Teacher | Update attendance |
| `/api/attendance/date/{date}` | GET | Admin/Teacher | Get all attendance for a date |
| `/api/attendance/user/{userId}` | GET | Authenticated | Get user attendance history |
| `/api/attendance/{id}` | DELETE | Admin | Delete attendance |

### **Example Requests**

#### Mark Attendance
```http
POST /api/attendance
Authorization: Bearer <admin-or-teacher-token>
Content-Type: application/json

{
  "userId": 5,
  "date": "2024-01-15",
  "status": 1  // 1 = Present, 0 = Absent
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "userId": 5,
  "date": "2024-01-15T00:00:00Z",
  "status": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

#### Update Attendance
```http
PUT /api/attendance/1
Authorization: Bearer <admin-or-teacher-token>
Content-Type: application/json

{
  "status": 0  // Change to Absent
}
```

#### Get Attendance by Date
```http
GET /api/attendance/date/2024-01-15
Authorization: Bearer <admin-or-teacher-token>
```

**Response:**
```json
[
  {
    "id": 1,
    "userId": 5,
    "date": "2024-01-15T00:00:00Z",
    "status": 1,
    "user": {
      "id": 5,
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com"
    }
  },
  {
    "id": 2,
    "userId": 6,
    "date": "2024-01-15T00:00:00Z",
    "status": 0,
    "user": {
      "id": 6,
      "firstName": "Jane",
      "lastName": "Smith",
      "email": "jane@example.com"
    }
  }
]
```

#### Get User Attendance History
```http
GET /api/attendance/user/5?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer <user-token>
```

---

## ?? Daily Menu Management

### **Models**

```csharp
public class DailyMenu
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    
    // Breakfast
    public string? BreakfastItems { get; set; }
    public decimal BreakfastPrice { get; set; }
    
    // Lunch
    public string? LunchItems { get; set; }
    public decimal LunchPrice { get; set; }
    
    // Dinner
    public string? DinnerItems { get; set; }
    public decimal DinnerPrice { get; set; }
    
    // Fixed daily charges (tea + water)
    public decimal DailyFixedCharge { get; set; } = 20;  // Default
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### **Business Rules**
- ? One menu per date (enforced by unique index)
- ? Only Admin can create/update/delete menus
- ? Anyone can view menus
- ? Default fixed charge: 20 (tea + water)

### **API Endpoints**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/dailymenu` | POST | Admin | Create menu |
| `/api/dailymenu/{id}` | PUT | Admin | Update menu |
| `/api/dailymenu/{id}` | GET | Public | Get menu by ID |
| `/api/dailymenu/date/{date}` | GET | Public | Get menu for a date |
| `/api/dailymenu/range?startDate=&endDate=` | GET | Public | Get menus in range |
| `/api/dailymenu/{id}` | DELETE | Admin | Delete menu |

### **Example Requests**

#### Create Daily Menu
```http
POST /api/dailymenu
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "date": "2024-01-15",
  "breakfastItems": "Paratha, Eggs, Jam",
  "breakfastPrice": 50,
  "lunchItems": "Chicken Biryani, Raita, Salad",
  "lunchPrice": 150,
  "dinnerItems": "Chapati, Dal, Sabzi",
  "dinnerPrice": 100,
  "dailyFixedCharge": 20
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "date": "2024-01-15T00:00:00Z",
  "breakfastItems": "Paratha, Eggs, Jam",
  "breakfastPrice": 50,
  "lunchItems": "Chicken Biryani, Raita, Salad",
  "lunchPrice": 150,
  "dinnerItems": "Chapati, Dal, Sabzi",
  "dinnerPrice": 100,
  "dailyFixedCharge": 20,
  "createdAt": "2024-01-15T08:00:00Z",
  "updatedAt": null
}
```

#### Get Menu for a Date
```http
GET /api/dailymenu/date/2024-01-15
```

#### Get Menus in Range
```http
GET /api/dailymenu/range?startDate=2024-01-01&endDate=2024-01-31
```

---

## ?? Billing System

### **Models**

```csharp
public class UserBill
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public decimal TotalFixedCharges { get; set; }    // Tea + Water for ALL days
    public decimal TotalFoodCharges { get; set; }     // Food cost for PRESENT days only
    public decimal TotalAmount { get; set; }          // Fixed + Food
    
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime GeneratedDate { get; set; }
}
```

### **Billing Logic** ??

```
Total Bill = Fixed Charges + Food Charges

Fixed Charges = Daily Fixed Charge × Total Days in Period
              = (Tea + Water charge) × All days

Food Charges = Sum of (Breakfast + Lunch + Dinner) for PRESENT days only
             = ? (Menu costs) for each day user was Present
```

### **Example Calculation**

**Scenario:**
- Period: Jan 1 - Jan 10 (10 days)
- Daily Fixed Charge: ?20
- User was Present: 7 days
- User was Absent: 3 days

**Calculation:**
```
Fixed Charges = ?20 × 10 days = ?200
Food Charges = (Day1: ?300) + (Day2: ?300) + ... (Day7: ?300) = ?2,100
Total Bill = ?200 + ?2,100 = ?2,300
```

**Key Point:** User pays ?200 for tea+water even on the 3 absent days!

### **Business Rules**
- ? Fixed charges apply to ALL days (even absent days)
- ? Food charges ONLY apply to days marked Present
- ? Only Admin can generate bills
- ? Users can view their own bills
- ? Bills can be marked as paid

### **API Endpoints**

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/billing/generate` | POST | Admin | Generate bill |
| `/api/billing/{id}` | GET | User/Admin | Get bill by ID |
| `/api/billing/user/{userId}` | GET | User/Admin | Get user's bills |
| `/api/billing` | GET | Admin | Get all bills |
| `/api/billing/{id}/pay` | PUT | Admin | Mark bill as paid |
| `/api/billing/{id}` | DELETE | Admin | Delete bill |

### **Example Requests**

#### Generate Bill
```http
POST /api/billing/generate
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "userId": 5,
  "startDate": "2024-01-01",
  "endDate": "2024-01-31"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "userId": 5,
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-31T00:00:00Z",
  "totalFixedCharges": 620,
  "totalFoodCharges": 6300,
  "totalAmount": 6920,
  "totalDays": 31,
  "presentDays": 21,
  "absentDays": 10,
  "isPaid": false,
  "paidDate": null,
  "generatedDate": "2024-02-01T10:00:00Z",
  "user": {
    "id": 5,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com"
  }
}
```

#### Get User Bills
```http
GET /api/billing/user/5
Authorization: Bearer <user-5-token>
```

#### Mark Bill as Paid
```http
PUT /api/billing/1/pay
Authorization: Bearer <admin-token>
```

**Response:**
```json
{
  "id": 1,
  "userId": 5,
  "totalAmount": 6920,
  "isPaid": true,
  "paidDate": "2024-02-01T11:30:00Z",
  ...
}
```

---

## ?? Complete Workflow Example

### **Monthly Mess Management Flow**

#### **1. Admin Creates Monthly Menus** (Beginning of Month)
```http
POST /api/dailymenu
{
  "date": "2024-01-01",
  "breakfastItems": "...",
  "breakfastPrice": 50,
  "lunchItems": "...",
  "lunchPrice": 150,
  "dinnerItems": "...",
  "dinnerPrice": 100,
  "dailyFixedCharge": 20
}

// Repeat for each day of the month
```

#### **2. Teacher Marks Daily Attendance**
```http
POST /api/attendance
{
  "userId": 5,
  "date": "2024-01-01",
  "status": 1  // Present
}

POST /api/attendance
{
  "userId": 6,
  "date": "2024-01-01",
  "status": 0  // Absent
}
```

#### **3. Admin Generates Monthly Bills** (End of Month)
```http
POST /api/billing/generate
{
  "userId": 5,
  "startDate": "2024-01-01",
  "endDate": "2024-01-31"
}
```

**System Calculation:**
```
User 5 Attendance:
- Present: 25 days
- Absent: 6 days
- Total: 31 days

Fixed Charges:
- ?20 × 31 days = ?620

Food Charges (25 present days):
- Day 1: ?50 + ?150 + ?100 = ?300
- Day 2: ?300
- ...
- Day 25: ?300
- Total: ?300 × 25 = ?7,500

Total Bill = ?620 + ?7,500 = ?8,120
```

#### **4. Student Views Bill**
```http
GET /api/billing/user/5
```

#### **5. Student Pays, Admin Marks as Paid**
```http
PUT /api/billing/1/pay
```

---

## ?? Database Schema

### **Attendance Table**
```sql
CREATE TABLE Attendances (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Date DATE NOT NULL,
    Status INT NOT NULL,  -- 0=Absent, 1=Present
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT UQ_Attendance_User_Date UNIQUE (UserId, Date)
);
```

### **DailyMenu Table**
```sql
CREATE TABLE DailyMenus (
    Id INT PRIMARY KEY IDENTITY,
    Date DATE NOT NULL UNIQUE,
    BreakfastItems NVARCHAR(MAX),
    BreakfastPrice DECIMAL(18,2),
    LunchItems NVARCHAR(MAX),
    LunchPrice DECIMAL(18,2),
    DinnerItems NVARCHAR(MAX),
    DinnerPrice DECIMAL(18,2),
    DailyFixedCharge DECIMAL(18,2) NOT NULL DEFAULT 20,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);
```

### **UserBills Table**
```sql
CREATE TABLE UserBills (
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    TotalFixedCharges DECIMAL(18,2) NOT NULL,
    TotalFoodCharges DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    TotalDays INT NOT NULL,
    PresentDays INT NOT NULL,
    AbsentDays INT NOT NULL,
    IsPaid BIT NOT NULL DEFAULT 0,
    PaidDate DATETIME2 NULL,
    GeneratedDate DATETIME2 NOT NULL
);
```

---

## ?? Authorization Summary

| Feature | Create | Read | Update | Delete |
|---------|--------|------|--------|--------|
| **Attendance** | Admin/Teacher | Admin/Teacher/Own | Admin/Teacher | Admin |
| **Menu** | Admin | Anyone | Admin | Admin |
| **Billing** | Admin | Admin/Own | Admin (pay) | Admin |

---

## ?? Important Notes

### **Billing Logic**
1. **Fixed Charges are ALWAYS charged** - even if user is absent all days
2. **Food Charges are ONLY charged for Present days**
3. Bills are generated manually by Admin (not automatic)

### **Attendance**
- Cannot mark duplicate attendance for same user+date
- Use UPDATE to correct mistakes
- Date is normalized (time ignored)

### **Menus**
- One menu per date maximum
- Missing menu days use default fixed charge (?20)
- Anyone can view menus (no auth required for GET)

### **Default Values**
- Daily Fixed Charge: ?20
- Attendance Status: Absent = 0, Present = 1

---

## ?? Testing Checklist

### Attendance
- [ ] Mark attendance for a user
- [ ] Try marking duplicate attendance (should fail)
- [ ] Update attendance status
- [ ] Get attendance by date
- [ ] Get user attendance history with date range
- [ ] Delete attendance

### Menu
- [ ] Create daily menu
- [ ] Try creating duplicate menu for same date (should fail)
- [ ] Update menu
- [ ] Get menu by date
- [ ] Get menus in range
- [ ] Delete menu

### Billing
- [ ] Generate bill for user with attendance
- [ ] Verify fixed charges (all days)
- [ ] Verify food charges (present days only)
- [ ] Get user bills
- [ ] Mark bill as paid
- [ ] Verify bill totals match manual calculation

---

## ?? Related Files

- **Models**: `Models/Attendance.cs`, `Models/DailyMenu.cs`, `Models/UserBill.cs`
- **Services**: `Services/AttendanceService.cs`, `Services/DailyMenuService.cs`, `Services/BillingService.cs`
- **Controllers**: `Controllers/AttendanceController.cs`, `Controllers/DailyMenuController.cs`, `Controllers/BillingController.cs`
- **DbContext**: `Data/ApplicationDbContext.cs`

---

**Last Updated:** 2024  
**Version:** 1.0  
**Status:** Production Ready ??
