# ?? Attendance API Guide

## ?? Authentication & Authorization Rules

### **Teacher Role:**
- ? Can view all students
- ? Can mark attendance for **all students**
- ? **CANNOT** mark attendance for themselves
- ? **CANNOT** mark attendance for other teachers or admins
- ? Can view attendance history

### **Admin Role:**
- ? Can mark attendance for **everyone** (students, teachers, admins)
- ? Full access to all attendance operations
- ? Can delete attendance records

### **Student Role:**
- ? Can view their own attendance only
- ? Cannot mark or modify attendance

---

## ?? API Endpoints

### Base URL
```
https://localhost:7259/api
```

---

## 1?? Get All Students (For Marking Attendance)

**Endpoint:** `GET /api/attendance/students`

**Authorization:** Teacher or Admin

**Purpose:** Get list of all active students to display for attendance marking

**Request:**
```http
GET https://localhost:7259/api/attendance/students
Authorization: Bearer <teacher-or-admin-token>
```

**Response (200 OK):**
```json
[
  {
    "id": 3,
    "firstName": "Alice",
    "lastName": "Student",
    "fullName": "Alice Student",
    "email": "alice@mess.com",
    "rollNumber": "CS2024001",
    "roomNumber": "A-101"
  },
  {
    "id": 4,
    "firstName": "Bob",
    "lastName": "Student",
    "fullName": "Bob Student",
    "email": "bob@mess.com",
    "rollNumber": "CS2024002",
    "roomNumber": "A-102"
  },
  {
    "id": 5,
    "firstName": "Charlie",
    "lastName": "Student",
    "fullName": "Charlie Student",
    "email": "charlie@mess.com",
    "rollNumber": "CS2024003",
    "roomNumber": "B-201"
  }
]
```

**Frontend Usage:**
```javascript
// React example
const fetchStudents = async () => {
  const response = await axios.get(
    `${API_BASE_URL}/attendance/students`,
    {
      headers: { Authorization: `Bearer ${token}` }
    }
  );
  return response.data; // Array of students
};
```

---

## 2?? Mark Single Student Attendance

**Endpoint:** `POST /api/attendance`

**Authorization:** Teacher or Admin

**Validation:**
- Teachers can only mark students (not themselves or other teachers)
- Admin can mark anyone

**Request:**
```http
POST https://localhost:7259/api/attendance
Authorization: Bearer <teacher-or-admin-token>
Content-Type: application/json

{
  "userId": 3,
  "date": "2024-12-06",
  "status": 1
}
```

**Status Values:**
- `0` = Absent
- `1` = Present

**Response (201 Created):**
```json
{
  "id": 15,
  "userId": 3,
  "date": "2024-12-06T00:00:00Z",
  "status": 1,
  "createdAt": "2024-12-06T10:30:00Z",
  "updatedAt": null
}
```

**Error Response (403 Forbidden) - Teacher trying to mark themselves:**
```json
{
  "message": "Teachers cannot mark their own attendance"
}
```

**Error Response (400 Bad Request) - Already marked:**
```json
{
  "message": "Attendance already marked for user 3 on 2024-12-06. Use update instead."
}
```

---

## 3?? Mark Bulk Attendance (Recommended for Classes)

**Endpoint:** `POST /api/attendance/bulk`

**Authorization:** Teacher or Admin

**Purpose:** Mark attendance for multiple students at once

**Request:**
```http
POST https://localhost:7259/api/attendance/bulk
Authorization: Bearer <teacher-or-admin-token>
Content-Type: application/json

{
  "date": "2024-12-06",
  "attendances": [
    { "userId": 3, "status": 1 },
    { "userId": 4, "status": 1 },
    { "userId": 5, "status": 0 }
  ]
}
```

**Response (200 OK):**
```json
{
  "message": "Successfully marked attendance for 3 users",
  "attendances": [
    {
      "id": 15,
      "userId": 3,
      "date": "2024-12-06T00:00:00Z",
      "status": 1,
      "createdAt": "2024-12-06T10:30:00Z"
    },
    {
      "id": 16,
      "userId": 4,
      "date": "2024-12-06T00:00:00Z",
      "status": 1,
      "createdAt": "2024-12-06T10:30:00Z"
    },
    {
      "id": 17,
      "userId": 5,
      "date": "2024-12-06T00:00:00Z",
      "status": 0,
      "createdAt": "2024-12-06T10:30:00Z"
    }
  ]
}
```

**Frontend Example:**
```javascript
const markBulkAttendance = async (date, studentAttendances) => {
  // studentAttendances = [
  //   { userId: 3, status: 1 },
  //   { userId: 4, status: 1 },
  //   ...
  // ]
  
  const response = await axios.post(
    `${API_BASE_URL}/attendance/bulk`,
    {
      date: date, // "2024-12-06"
      attendances: studentAttendances
    },
    {
      headers: { Authorization: `Bearer ${token}` }
    }
  );
  return response.data;
};
```

---

## 4?? Get Attendance for Specific Date

**Endpoint:** `GET /api/attendance/date/{date}`

**Authorization:** Teacher or Admin

**Purpose:** View all attendance records for a specific date

**Request:**
```http
GET https://localhost:7259/api/attendance/date/2024-12-06
Authorization: Bearer <teacher-or-admin-token>
```

**Response (200 OK):**
```json
[
  {
    "id": 15,
    "userId": 3,
    "date": "2024-12-06T00:00:00Z",
    "status": 1,
    "createdAt": "2024-12-06T10:30:00Z",
    "user": {
      "id": 3,
      "firstName": "Alice",
      "lastName": "Student",
      "email": "alice@mess.com",
      "rollNumber": "CS2024001"
    }
  },
  {
    "id": 16,
    "userId": 4,
    "date": "2024-12-06T00:00:00Z",
    "status": 1,
    "createdAt": "2024-12-06T10:30:00Z",
    "user": {
      "id": 4,
      "firstName": "Bob",
      "lastName": "Student",
      "email": "bob@mess.com",
      "rollNumber": "CS2024002"
    }
  }
]
```

---

## 5?? Get Student's Attendance History

**Endpoint:** `GET /api/attendance/user/{userId}`

**Authorization:** User (for own data), Teacher, or Admin

**Purpose:** View attendance history with flexible filters

### Query Parameters:
- `startDate` - Start date (YYYY-MM-DD)
- `endDate` - End date (YYYY-MM-DD)
- `year` - Year (e.g., 2024)
- `month` - Month (1-12)
- `includeSummary` - Include statistics (true/false)
- `includeMenuDetails` - Include meal costs (true/false)

### Example 1: Simple Date Range
```http
GET https://localhost:7259/api/attendance/user/3?startDate=2024-12-01&endDate=2024-12-31
Authorization: Bearer <token>
```

**Response:**
```json
[
  {
    "id": 15,
    "userId": 3,
    "date": "2024-12-01T00:00:00Z",
    "status": 1,
    "createdAt": "2024-12-01T10:00:00Z"
  },
  {
    "id": 16,
    "userId": 3,
    "date": "2024-12-02T00:00:00Z",
    "status": 1,
    "createdAt": "2024-12-02T10:00:00Z"
  }
]
```

### Example 2: Monthly with Summary
```http
GET https://localhost:7259/api/attendance/user/3?year=2024&month=12&includeSummary=true
Authorization: Bearer <token>
```

**Response:**
```json
{
  "userId": 3,
  "startDate": "2024-12-01",
  "endDate": "2024-12-31",
  "year": 2024,
  "month": 12,
  "monthName": "December 2024",
  "attendances": [
    {
      "id": 15,
      "userId": 3,
      "date": "2024-12-01T00:00:00Z",
      "status": 1
    }
  ],
  "summary": {
    "totalDays": 31,
    "presentDays": 20,
    "absentDays": 8,
    "daysWithoutAttendance": 3,
    "estimatedFixedCharges": 620,
    "estimatedFoodCharges": 6000,
    "estimatedTotalCost": 6620
  }
}
```

---

## 6?? Update Attendance

**Endpoint:** `PUT /api/attendance/{id}`

**Authorization:** Teacher or Admin

**Purpose:** Change attendance status (Present ? Absent)

**Request:**
```http
PUT https://localhost:7259/api/attendance/15
Authorization: Bearer <teacher-or-admin-token>
Content-Type: application/json

{
  "status": 0
}
```

**Response (200 OK):**
```json
{
  "id": 15,
  "userId": 3,
  "date": "2024-12-06T00:00:00Z",
  "status": 0,
  "createdAt": "2024-12-06T10:30:00Z",
  "updatedAt": "2024-12-06T11:45:00Z"
}
```

---

## 7?? Delete Attendance

**Endpoint:** `DELETE /api/attendance/{id}`

**Authorization:** Admin only

**Purpose:** Remove attendance record

**Request:**
```http
DELETE https://localhost:7259/api/attendance/15
Authorization: Bearer <admin-token>
```

**Response (204 No Content):**
```
(Empty response body)
```

---

## ?? Complete Teacher Workflow

### Step 1: Login as Teacher
```javascript
const login = async () => {
  const response = await axios.post(`${API_BASE_URL}/auth/login`, {
    email: "teacher@mess.com",
    password: "Teacher@123"
  });
  const { token, userId } = response.data;
  localStorage.setItem('token', token);
  localStorage.setItem('userId', userId);
};
```

### Step 2: Get All Students
```javascript
const students = await axios.get(
  `${API_BASE_URL}/attendance/students`,
  {
    headers: { Authorization: `Bearer ${token}` }
  }
);
// Display students in UI with checkboxes
```

### Step 3: Mark Attendance (Bulk)
```javascript
const markAttendance = async (date, attendanceData) => {
  // attendanceData = students.map(s => ({
  //   userId: s.id,
  //   status: s.isPresent ? 1 : 0
  // }))
  
  await axios.post(
    `${API_BASE_URL}/attendance/bulk`,
    {
      date: date,
      attendances: attendanceData
    },
    {
      headers: { Authorization: `Bearer ${token}` }
    }
  );
};
```

### Step 4: View Attendance History
```javascript
const viewAttendance = async (date) => {
  const response = await axios.get(
    `${API_BASE_URL}/attendance/date/${date}`,
    {
      headers: { Authorization: `Bearer ${token}` }
    }
  );
  // Display attendance records
};
```

---

## ?? Complete Admin Workflow

Admins have the same endpoints as teachers, but with additional permissions:

1. **Can mark attendance for teachers** (not just students)
2. **Can delete attendance records**
3. **Full access to all operations**

---

## ?? Common Errors

### 401 Unauthorized
```json
{
  "message": "Invalid token claims"
}
```
**Solution:** Token expired or invalid. Re-login.

### 403 Forbidden
```json
{
  "message": "Teachers cannot mark their own attendance"
}
```
**Solution:** Teacher tried to mark themselves. This is blocked.

### 400 Bad Request
```json
{
  "message": "Attendance already marked for user 3 on 2024-12-06. Use update instead."
}
```
**Solution:** Use PUT endpoint to update existing attendance.

---

## ?? Quick Reference

| Action | Endpoint | Method | Role | URL |
|--------|----------|--------|------|-----|
| Get Students | `/attendance/students` | GET | Teacher/Admin | `https://localhost:7259/api/attendance/students` |
| Mark Single | `/attendance` | POST | Teacher/Admin | `https://localhost:7259/api/attendance` |
| Mark Bulk | `/attendance/bulk` | POST | Teacher/Admin | `https://localhost:7259/api/attendance/bulk` |
| View by Date | `/attendance/date/{date}` | GET | Teacher/Admin | `https://localhost:7259/api/attendance/date/2024-12-06` |
| View User History | `/attendance/user/{userId}` | GET | Any (own data) | `https://localhost:7259/api/attendance/user/3` |
| Update | `/attendance/{id}` | PUT | Teacher/Admin | `https://localhost:7259/api/attendance/15` |
| Delete | `/attendance/{id}` | DELETE | Admin | `https://localhost:7259/api/attendance/15` |

---

## ?? Test Credentials

```
Teacher:
  Email: teacher@mess.com
  Password: Teacher@123

Admin:
  Email: admin@mess.com
  Password: Admin@123

Student (Alice):
  Email: alice@mess.com
  Password: Student@123
```

---

## ? Validation Summary

| Role | Can Mark Students | Can Mark Teachers | Can Mark Self | Can Delete |
|------|------------------|-------------------|---------------|------------|
| **Teacher** | ? Yes | ? No | ? No | ? No |
| **Admin** | ? Yes | ? Yes | ? Yes | ? Yes |
| **Student** | ? No | ? No | ? No | ? No |
