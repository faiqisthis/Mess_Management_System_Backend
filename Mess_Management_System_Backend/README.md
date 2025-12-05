# Mess Management System Backend - Simplified

## Overview
A clean and simple ASP.NET Core 8 Web API for managing mess (dining hall) operations with user authentication.

## Features
- ? User Authentication (JWT)
- ? User Management (CRUD)
- ? Role-based Authorization
- ? SQL Server Database
- ? CORS Support for Frontend

## Project Structure

```
Mess_Management_System_Backend/
??? Controllers/
?   ??? AuthController.cs      # Login & Register
?   ??? UsersController.cs     # User CRUD operations
??? Data/
?   ??? ApplicationDbContext.cs # Database context & entities
??? Dtos/
?   ??? AuthDto.cs             # Login/Register DTOs
?   ??? CreateUserDto.cs       # Create user DTO
?   ??? UpdateUserDto.cs       # Update user DTO
?   ??? UserDto.cs             # User response DTO
??? Models/
?   ??? User.cs                # User model
??? Services/
?   ??? IJwtService.cs         # JWT interface
?   ??? JwtService.cs          # JWT implementation
?   ??? IUserService.cs        # User service interface
?   ??? UserService.cs         # User service implementation
??? Program.cs                 # Application entry point

```

## Simplifications Made

### ? Removed Complexity:
1. **AutoMapper** ? Simple manual mapping
2. **FluentValidation** ? Built-in DataAnnotations
3. **Serilog** ? Built-in .NET logging
4. **ApiResponse Wrapper** ? Direct controller responses
5. **Custom Exception Middleware** ? Built-in error handling
6. **Razor Pages/Views** ? Pure API project
7. **HomeController** ? Not needed for API
8. **Complex Password Validation** ? Simplified authentication

### ? What Remains:
- Clean service layer
- JWT authentication
- Entity Framework Core
- Data annotations for validation
- Simple and maintainable code

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register new user

### Users
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

## Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=MessManagement;..."
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "your-issuer",
    "Audience": "your-audience"
  }
}
```

## Running the Project

```bash
cd Mess_Management_System_Backend
dotnet restore
dotnet ef database update
dotnet run
```

## Dependencies

- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.AspNetCore.Identity (for password hashing)

## Notes

- All responses are in JSON format
- Authentication uses JWT tokens
- Passwords are hashed using ASP.NET Identity password hasher
- Built with .NET 8 and C# 12
