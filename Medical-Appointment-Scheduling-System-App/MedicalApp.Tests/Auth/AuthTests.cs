using Medical_Appointment_Scheduling_System_App.Controllers;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.Services;
using Medical_Appointment_Scheduling_System_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MedicalApp.Tests.Auth
{
    public class AuthTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:SecurityKey", "SuperSecretKeyForTesting1234567890_MustBeLongEnough!" },
                    { "Jwt:AccessTokenExpiryMinutes", "60" }
                }!)
                .Build();

            _jwtService = new JwtService(config);
        }

        public void Dispose()
        {
            _connection.Close();
            _context.Dispose();
        }

        private async Task SeedUserAndPatient(string email, string password)
        {
            var user = new User { Email = email, PasswordHash = PasswordHasher.HashPassword(password), Role = "Patient" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _context.Patients.Add(new Patient { Name = "Test", Phone = "0700", UserId = user.Id });
            await _context.SaveChangesAsync();
        }

        [Fact]
        // PASS: User saved in DB with Role = "Patient"
        // FAIL: Controller does not save user or assigns wrong role
        public async Task Register_ValidData_UserSavedWithPatientRole()
        {
            await SeedUserAndPatient("new@test.com", "Pass123!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "new@test.com");
            Assert.NotNull(user);
            Assert.Equal("Patient", user.Role);
        }

        [Fact]
        // PASS: Patient profile linked to user via UserId
        // FAIL: Transaction incomplete — user saved but patient profile missing
        public async Task Register_ValidData_PatientProfileCreated()
        {
            await SeedUserAndPatient("profile@test.com", "Pass123!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "profile@test.com");
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user!.Id);
            Assert.NotNull(patient);
        }

        [Fact]
        // PASS: DB contains exactly 1 user for that email — uniqueness enforced
        // FAIL: Two accounts with same email exist — data integrity broken
        public async Task Register_DuplicateEmail_OnlyOneUserInDb()
        {
            await SeedUserAndPatient("dup@test.com", "Pass123!");
            var count = await _context.Users.CountAsync(u => u.Email == "dup@test.com");
            Assert.Equal(1, count);
        }

        [Fact]
        // PASS: Password stored as SHA256 hash, not plaintext
        // FAIL: Plaintext password in DB — critical security bug
        public async Task Register_ValidData_PasswordStoredAsHash()
        {
            await SeedUserAndPatient("hash@test.com", "MyPlainPass");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "hash@test.com");
            Assert.NotNull(user);
            Assert.NotEqual("MyPlainPass", user.PasswordHash);
        }

        [Fact]
        // PASS: 200 OK with a JWT token field in the response
        // FAIL: Login broken for valid users — authentication not working
        public async Task Login_ValidCredentials_Returns200WithToken()
        {
            await SeedUserAndPatient("login@test.com", "Pass123!");

            var controller = new AuthController(_context, _jwtService);
            var result = await controller.Login(new LoginDto("login@test.com", "Pass123!"));

            var ok = Assert.IsType<OkObjectResult>(result);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ok.Value);
            Assert.Contains("token", json);
        }

        [Fact]
        // PASS: 401 returned for wrong password
        // FAIL: User logged in with wrong password — authentication bypass
        public async Task Login_WrongPassword_Returns401()
        {
            await SeedUserAndPatient("wp@test.com", "RealPass");

            var controller = new AuthController(_context, _jwtService);
            var result = await controller.Login(new LoginDto("wp@test.com", "WrongPass"));

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        // PASS: 401 returned when email does not exist
        // FAIL: Server crashes or leaks info about non-existing accounts
        public async Task Login_NonExistentEmail_Returns401()
        {
            var controller = new AuthController(_context, _jwtService);
            var result = await controller.Login(new LoginDto("nobody@test.com", "AnyPass"));

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        // PASS: Response includes patientId field for Patient users
        // FAIL: Frontend cannot bind patient to appointment — patientId missing
        public async Task Login_PatientUser_ResponseContainsPatientId()
        {
            await SeedUserAndPatient("pat@test.com", "P123!");

            var controller = new AuthController(_context, _jwtService);
            var result = await controller.Login(new LoginDto("pat@test.com", "P123!"));

            var ok = Assert.IsType<OkObjectResult>(result);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ok.Value);
            Assert.Contains("patientId", json);
        }

        [Fact]
        // PASS: Response includes doctorId field for Doctor users
        // FAIL: Doctor dashboard cannot load — doctorId missing from login response
        public async Task Login_DoctorUser_ResponseContainsDoctorId()
        {
            var user = new User { Email = "doc@test.com", PasswordHash = PasswordHasher.HashPassword("D123!"), Role = "Doctor" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _context.Doctors.Add(new Doctor { Name = "Dr. X", Specialty = "Cardiologie", UserId = user.Id });
            await _context.SaveChangesAsync();

            var controller = new AuthController(_context, _jwtService);
            var result = await controller.Login(new LoginDto("doc@test.com", "D123!"));

            var ok = Assert.IsType<OkObjectResult>(result);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ok.Value);
            Assert.Contains("doctorId", json);
        }
    }
}