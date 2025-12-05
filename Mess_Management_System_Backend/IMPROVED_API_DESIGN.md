# ?? Improved API Design - Arrays & Bulk Operations

## ? What Changed

### **1. DailyMenu - Now Uses Array Structure**
Instead of separate fields for each meal, menus now use an array of `MealItem` objects:

**Before:**
```json
{
  "breakfastItems": "Paratha, Eggs",
  "breakfastPrice": 50,
  "lunchItems": "Biryani",
  "lunchPrice": 150,
  "dinnerItems": "Chapati, Dal",
  "dinnerPrice": 100
}
```

**After (Improved):**
```json
{
  "date": "2024-01-15",
  "meals": [
    {
      "name": "Paratha, Eggs, Jam",
      "type": 0,  // 0 = Breakfast
      "price": 50
    },
    {
      "name": "Chicken Biryani, Raita, Salad",
      "type": 1,  // 1 = Lunch
      "price": 150
    },
    {
      "name": "Chapati, Dal, Sabzi",
      "type": 2,  // 2 = Dinner
      "price": 100
    }
  ],
  "dailyFixedCharge": 20
}
```

**Benefits:**
- ? Easy to map/destructure in frontend
- ? Can have multiple items per meal type
- ? Flexible structure
- ? JSON storage in database

### **2. Attendance - Now Supports Bulk Marking**
Mark attendance for multiple students in a single request!

**Before (One at a time):**
```http
POST /api/attendance
{ "userId": 1, "date": "2024-01-15", "status": 1 }

POST /api/attendance
{ "userId": 2, "date": "2024-01-15", "status": 1 }

POST /api/attendance
{ "userId": 3, "date": "2024-01-15", "status": 0 }
```

**After (Bulk operation):**
```http
POST /api/attendance/bulk
{
  "date": "2024-01-15",
  "attendances": [
    { "userId": 1, "status": 1 },
    { "userId": 2, "status": 1 },
    { "userId": 3, "status": 0 },
    { "userId": 4, "status": 1 },
    { "userId": 5, "status": 1 }
  ]
}
```

**Benefits:**
- ? One API call instead of multiple
- ? Faster performance
- ? Transactional (all succeed or all fail)
- ? Better for UI (mark entire class at once)

---

## ?? Complete Examples

### **Daily Menu - Create**

```http
POST /api/dailymenu
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "date": "2024-01-15",
  "meals": [
    {
      "name": "Paratha with Eggs and Butter",
      "type": 0,
      "price": 50
    },
    {
      "name": "Chicken Biryani with Raita and Salad",
      "type": 1,
      "price": 150
    },
    {
      "name": "Roti, Dal Tadka, Mixed Vegetables",
      "type": 2,
      "price": 100
    }
  ],
  "dailyFixedCharge": 20
}
```

**Response:**
```json
{
  "id": 1,
  "date": "2024-01-15T00:00:00Z",
  "meals": [
    {
      "name": "Paratha with Eggs and Butter",
      "type": 0,
      "price": 50
    },
    {
      "name": "Chicken Biryani with Raita and Salad",
      "type": 1,
      "price": 150
    },
    {
      "name": "Roti, Dal Tadka, Mixed Vegetables",
      "type": 2,
      "price": 100
    }
  ],
  "dailyFixedCharge": 20,
  "createdAt": "2024-01-15T08:00:00Z",
  "updatedAt": null
}
```

### **Frontend - Easy Destructuring**

```javascript
// React/JavaScript example
const menu = await fetchMenu('2024-01-15');

// Easy to destructure
const breakfast = menu.meals.find(m => m.type === 0);
const lunch = menu.meals.find(m => m.type === 1);
const dinner = menu.meals.find(m => m.type === 2);

// Or map through all meals
menu.meals.map(meal => {
  console.log(`${meal.name}: ?${meal.price}`);
});

// Calculate total food cost
const totalFoodCost = menu.meals.reduce((sum, meal) => sum + meal.price, 0);
// totalFoodCost = 300
```

---

### **Bulk Attendance - Mark Entire Class**

```http
POST /api/attendance/bulk
Authorization: Bearer <teacher-token>
Content-Type: application/json

{
  "date": "2024-01-15",
  "attendances": [
    { "userId": 1, "status": 1 },   // John - Present
    { "userId": 2, "status": 1 },   // Jane - Present
    { "userId": 3, "status": 0 },   // Bob - Absent
    { "userId": 4, "status": 1 },   // Alice - Present
    { "userId": 5, "status": 0 },   // Charlie - Absent
    { "userId": 6, "status": 1 },   // Diana - Present
    { "userId": 7, "status": 1 },   // Eric - Present
    { "userId": 8, "status": 1 },   // Fiona - Present
    { "userId": 9, "status": 1 },   // George - Present
    { "userId": 10, "status": 0 }   // Helen - Absent
  ]
}
```

**Response (Success):**
```json
{
  "message": "Successfully marked attendance for 10 users",
  "attendances": [
    {
      "id": 1,
      "userId": 1,
      "date": "2024-01-15T00:00:00Z",
      "status": 1,
      "createdAt": "2024-01-15T09:00:00Z"
    },
    // ... 9 more records
  ]
}
```

**Response (Partial Failure):**
```json
{
  "message": "Bulk attendance marking failed with errors: User with ID 99 not found.; Attendance already exists for user 5 on 2024-01-15."
}
```

### **Frontend - Bulk Attendance UI**

```javascript
// React example - Mark attendance for entire class
const markClassAttendance = async () => {
  const students = [
    { id: 1, name: "John", status: "present" },
    { id: 2, name: "Jane", status: "present" },
    { id: 3, name: "Bob", status: "absent" },
    // ... more students
  ];

  const payload = {
    date: "2024-01-15",
    attendances: students.map(s => ({
      userId: s.id,
      status: s.status === "present" ? 1 : 0
    }))
  };

  const response = await fetch('/api/attendance/bulk', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });

  if (response.ok) {
    alert('Attendance marked for all students!');
  }
};
```

---

## ?? Migration Impact

### **Database Schema Changes**

**DailyMenus Table:**
```sql
-- Before (separate columns)
CREATE TABLE DailyMenus (
    BreakfastItems NVARCHAR(MAX),
    BreakfastPrice DECIMAL(18,2),
    LunchItems NVARCHAR(MAX),
    LunchPrice DECIMAL(18,2),
    DinnerItems NVARCHAR(MAX),
    DinnerPrice DECIMAL(18,2),
    ...
);

-- After (JSON column)
CREATE TABLE DailyMenus (
    Id INT PRIMARY KEY,
    Date DATETIME2 NOT NULL,
    Meals NVARCHAR(MAX) NOT NULL,  -- JSON array
    DailyFixedCharge DECIMAL(18,2) NOT NULL,
    ...
);
```

**Example JSON stored:**
```json
[
  {"name":"Paratha, Eggs","type":0,"price":50},
  {"name":"Biryani","type":1,"price":150},
  {"name":"Chapati, Dal","type":2,"price":100}
]
```

---

## ?? MealType Enum

```csharp
public enum MealType
{
    Breakfast = 0,
    Lunch = 1,
    Dinner = 2
}
```

**Usage in JSON:**
```json
{
  "name": "Paratha",
  "type": 0,  // Breakfast
  "price": 50
}
```

---

## ?? Complete Workflow Examples

### **Example 1: Teacher Marks Daily Attendance**

**Step 1: Get all students**
```http
GET /api/users
Authorization: Bearer <admin-token>
```

**Step 2: Mark attendance for all**
```http
POST /api/attendance/bulk
{
  "date": "2024-01-15",
  "attendances": [
    { "userId": 1, "status": 1 },
    { "userId": 2, "status": 1 },
    { "userId": 3, "status": 0 },
    // ... all students
  ]
}
```

### **Example 2: Admin Creates Weekly Menus**

```javascript
// Create menus for an entire week
const weekMenus = [
  {
    date: "2024-01-15",
    meals: [
      { name: "Paratha, Eggs", type: 0, price: 50 },
      { name: "Dal Rice", type: 1, price: 120 },
      { name: "Roti, Sabzi", type: 2, price: 100 }
    ],
    dailyFixedCharge: 20
  },
  {
    date: "2024-01-16",
    meals: [
      { name: "Bread, Jam", type: 0, price: 40 },
      { name: "Biryani", type: 1, price: 150 },
      { name: "Chapati, Dal", type: 2, price: 100 }
    ],
    dailyFixedCharge: 20
  },
  // ... more days
];

for (const menu of weekMenus) {
  await fetch('/api/dailymenu', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${adminToken}`
    },
    body: JSON.stringify(menu)
  });
}
```

---

## ?? Comparison: Old vs New

### **Menu Creation**

| Aspect | Old Design | New Design |
|--------|-----------|------------|
| **Structure** | Flat fields | Array of objects |
| **Fields** | 6 fields (items + prices) | 1 array field |
| **Flexibility** | Fixed 3 meals | Any number of meals |
| **Frontend** | Manual mapping | Easy destructuring |
| **Storage** | Multiple columns | Single JSON column |

### **Attendance Marking**

| Aspect | Old Design | New Design |
|--------|-----------|------------|
| **Requests** | N requests for N students | 1 request for N students |
| **Performance** | O(n) API calls | O(1) API call |
| **UI** | Mark one by one | Mark entire class |
| **Errors** | Scattered | Consolidated |
| **Transaction** | Individual | Atomic |

---

## ?? Updated API Endpoints

### **Daily Menu**
| Endpoint | Method | Changes |
|----------|--------|---------|
| `POST /api/dailymenu` | POST | Now accepts `meals` array |
| `PUT /api/dailymenu/{id}` | PUT | Updates `meals` array |
| `GET /api/dailymenu/{id}` | GET | Returns `meals` array |

### **Attendance**
| Endpoint | Method | Changes |
|----------|--------|---------|
| `POST /api/attendance` | POST | Single user (unchanged) |
| `POST /api/attendance/bulk` | POST | **NEW** - Bulk marking |

---

## ? Benefits Summary

### **For Frontend Developers:**
- ? Easier to work with arrays than separate fields
- ? Standard JavaScript array methods (map, filter, reduce)
- ? Less API calls for bulk operations
- ? Better performance

### **For Backend:**
- ? Simpler database schema
- ? Easier to extend (add new meal types)
- ? Better transaction handling
- ? More maintainable code

### **For Users (Teachers/Admin):**
- ? Mark attendance for entire class at once
- ? Faster daily operations
- ? Better UX

---

## ?? Testing

### **Test Menu with Multiple Items**
```http
POST /api/dailymenu
{
  "date": "2024-01-15",
  "meals": [
    { "name": "Breakfast Option 1", "type": 0, "price": 50 },
    { "name": "Breakfast Option 2", "type": 0, "price": 60 },
    { "name": "Lunch Special", "type": 1, "price": 150 },
    { "name": "Dinner Combo", "type": 2, "price": 100 }
  ],
  "dailyFixedCharge": 20
}
```

### **Test Bulk Attendance**
```http
POST /api/attendance/bulk
{
  "date": "2024-01-15",
  "attendances": [
    { "userId": 1, "status": 1 },
    { "userId": 2, "status": 1 },
    { "userId": 3, "status": 0 }
  ]
}
```

---

**Last Updated:** 2024  
**Version:** 2.0  
**Breaking Changes:** Yes - Menu structure changed, migration required
