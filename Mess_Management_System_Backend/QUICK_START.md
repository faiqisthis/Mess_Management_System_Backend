# Quick Start Guide - Mess Management System

## ?? Running the Application

```bash
cd Mess_Management_System_Backend
dotnet run
```

The API will start on:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001

## ?? Test the API

### 1. Login as Admin
```bash
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "admin@mess.com",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "userId": 1,
  "email": "admin@mess.com",
  "role": "Admin",
  "token": "eyJhbGc..."
}
```

### 2. Get All Users (Admin Only)
```bash
GET http://localhost:5000/api/users
Authorization: Bearer {token}
```

### 3. Register New User
```bash
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "firstName": "Test",
  "lastName": "User",
  "email": "test@mess.com",
  "password": "Test@123",
  "role": "Student"
}
```

## ?? Default Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@mess.com | Admin@123 |
| Student | student@mess.com | Student@123 |
| Teacher | teacher@mess.com | Teacher@123 |

## ?? API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register

### Users (Requires Auth)
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

## ??? Development Commands

### Database
```bash
# Drop database
dotnet ef database drop --force

# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Build & Run
```bash
# Build project
dotnet build

# Run project
dotnet run

# Run with watch (auto-reload)
dotnet watch run

# Clean build
dotnet clean
dotnet build
```

## ?? Project Structure
```
Mess_Management_System_Backend/
??? Controllers/         # API endpoints
??? Data/               # Database context & entities
??? Dtos/               # Data transfer objects
??? Models/             # Domain models
??? Services/           # Business logic
??? Migrations/         # EF Core migrations
??? Program.cs          # Application entry point
```

## ?? Troubleshooting

### Database Connection Error
Check `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MessManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Port Already in Use
Change ports in `Properties/launchSettings.json` or:
```bash
dotnet run --urls "http://localhost:5555"
```

### Migration Issues
```bash
# Reset database
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## ?? Next Steps
1. Test all endpoints using Postman/Thunder Client
2. Implement additional features
3. Deploy to production server
