using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedicalApp.Tests.Appointments
{
    public abstract class AppointmentTestBase : IDisposable
    {
        protected readonly SqliteConnection _connection;
        protected readonly ApplicationDbContext _context;
        protected readonly JwtService _jwtService;

        protected AppointmentTestBase()
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

        protected async Task<(User user, Doctor doctor)> SeedDoctorAsync(string email = "doc@test.com")
        {
            var user = new User { Email = email, PasswordHash = "hash", Role = "Doctor" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var doctor = new Doctor { Name = "Dr. Test", Specialty = "Cardiologie", UserId = user.Id };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return (user, doctor);
        }

        protected async Task<(User user, Patient patient)> SeedPatientAsync(string email = "patient@test.com")
        {
            var user = new User { Email = email, PasswordHash = "hash", Role = "Patient" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var patient = new Patient { Name = "Test Patient", Phone = "0700000000", UserId = user.Id };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return (user, patient);
        }

        protected static void SetUserOnController(ControllerBase controller, string userId, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
                }
            };
        }
    }
}
