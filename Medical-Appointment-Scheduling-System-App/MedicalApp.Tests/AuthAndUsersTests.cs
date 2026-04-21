using Medical_Appointment_Scheduling_System_App.Controllers;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.Services;
using Medical_Appointment_Scheduling_System_App.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace MedicalApp.Tests
{
    public class AuthAndUsersTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthAndUsersTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:SecurityKey", "SuperSecretKeyForTesting1234567890_MustBeLongEnough!"},
                {"Jwt:AccessTokenExpiryMinutes", "60"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _jwtService = new JwtService(configuration);
        }

        public void Dispose()
        {
            _connection.Close();
            _context.Dispose();
        }

        [Fact]
        public async Task Register_ValidData_ReturnsCreatedWithPatientRole()
        {
            var controller = new AuthController(_context, _jwtService);

            var dto = new CreatePatientAccountAndProfileDto(Email: "new@test.com", Password: "Pass123", Name: "Test Pacient", Phone: "0700000000");

            var result = await controller.Register(dto);

            var createdResult = Assert.IsType<CreatedResult>(result);
            Assert.NotNull(createdResult.Value);

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == "new@test.com");
            Assert.NotNull(userInDb);
            Assert.Equal("Patient", userInDb.Role);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            _context.Users.Add(new User { Email = "duplicate@test.com", PasswordHash = "hash", Role = "Patient" });
            await _context.SaveChangesAsync();

            var controller = new AuthController(_context, _jwtService);

            var dto = new CreatePatientAccountAndProfileDto(Email: "duplicate@test.com", Password: "Pass123", Name: "Test", Phone: "123");

            var result = await controller.Register(dto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Acest email este deja folosit", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithJwtToken()
        {
            var hash = PasswordHasher.HashPassword("CorrectPass123");
            _context.Users.Add(new User { Email = "login@test.com", PasswordHash = hash, Role = "Patient" });
            await _context.SaveChangesAsync();

            var controller = new AuthController(_context, _jwtService);

            var dto = new LoginDto(Email: "login@test.com", Password: "CorrectPass123");

            var result = await controller.Login(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(okResult.Value);
            Assert.Contains("token", json);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var hash = PasswordHasher.HashPassword("CorrectPass123");
            _context.Users.Add(new User { Email = "login@test.com", PasswordHash = hash, Role = "Patient" });
            await _context.SaveChangesAsync();

            var controller = new AuthController(_context, _jwtService);

            var dto = new LoginDto(Email: "login@test.com", Password: "WrongPassword");

            var result = await controller.Login(dto);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Contains("Email sau parol", unauthorizedResult.Value?.ToString());
        }

        [Fact]
        public async Task CreateDoctorAccount_ValidData_ReturnsCreated()
        {
            var controller = new UsersController(_context);

            var dto = new CreateDoctorAccountAndProfileDto(Email: "doc@test.com", Password: "Pass123", Name: "Dr. House", Specialty: "Cardiologie");

            var result = await controller.CreateDoctorAccount(dto);

            var createdResult = Assert.IsType<CreatedResult>(result);

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == "doc@test.com");
            var doctorInDb = await _context.Doctors.FirstOrDefaultAsync(d => d.Name == "Dr. House");

            Assert.NotNull(userInDb);
            Assert.Equal("Doctor", userInDb.Role);
            Assert.NotNull(doctorInDb);
        }

        [Fact]
        public void CreateDoctorAccount_RequiresAdminRole()
        {
            var methodInfo = typeof(UsersController).GetMethod(nameof(UsersController.CreateDoctorAccount));

            var authorizeAttr = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false)
                                          .Cast<AuthorizeAttribute>()
                                          .FirstOrDefault();

            Assert.NotNull(authorizeAttr);
            Assert.Equal("Admin", authorizeAttr.Roles);
        }
    }
}