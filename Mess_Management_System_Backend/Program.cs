using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Models;
using Mess_Management_System_Backend.Mappings;
using Mess_Management_System_Backend.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------
// Logging (Serilog)
// -------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// -------------------------------
// Database Context (SQL Server)
// -------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------------------
// Dependency Injection
// -------------------------------
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// -------------------------------
// Controllers + FluentValidation
// -------------------------------
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

// -------------------------------
// AutoMapper
// -------------------------------
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// -------------------------------
// CORS (for Next.js frontend)
// -------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs",
        policy => policy
            .WithOrigins("http://localhost:3000") // adjust when deployed
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// -------------------------------
// Build App
// -------------------------------
var app = builder.Build();

// -------------------------------
// Middleware Pipeline
// -------------------------------
app.UseGlobalExceptionHandler(); // custom middleware
app.UseSerilogRequestLogging();  // logs all HTTP requests

app.UseHttpsRedirection();
app.UseCors("AllowNextJs");
app.UseAuthorization();

app.MapControllers();

app.Run();
