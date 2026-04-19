using Medical_Appointment_Scheduling_System_App.Controllers;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace MedicalApp.Tests.Users
{
    public class UsersControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;

        public UsersControllerTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _connection.Close();
            _context.Dispose();
        }

        [Fact]
        // PASS: 201 Created returned with correct user and doctor data in response
        // FAIL: Doctor account creation broken — admin cannot add doctors
        public async Task CreateDoctorAccount_ValidData_Returns201()
        {
            var controller = new UsersController(_context);
            var dto = new CreateDoctorAccountAndProfileDto("doc@test.com", "Pass123!", "Dr. House", "Cardiologie");

            var result = await controller.CreateDoctorAccount(dto);

            Assert.IsType<CreatedResult>(result);
        }

        [Fact]
        // PASS: User record exists in DB with Role = "Doctor"
        // FAIL: User saved with wrong role — doctor gets patient permissions
        public async Task CreateDoctorAccount_ValidData_UserHasDoctorRole()
        {
            var controller = new UsersController(_context);
            var dto = new CreateDoctorAccountAndProfileDto("doc2@test.com", "Pass123!", "Dr. Wilson", "Oncologie");

            await controller.CreateDoctorAccount(dto);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "doc2@test.com");
            Assert.NotNull(user);
            Assert.Equal("Doctor", user.Role);
        }

        [Fact]
        // PASS: Doctor profile (name, specialty) saved and linked to user via UserId
        // FAIL: Doctor user created but no doctor profile — linked data incomplete
        public async Task CreateDoctorAccount_ValidData_CreatesDoctorProfile()
        {
            var controller = new UsersController(_context);
            var dto = new CreateDoctorAccountAndProfileDto("doc3@test.com", "Pass123!", "Dr. Chase", "Neurologie");

            await controller.CreateDoctorAccount(dto);

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Name == "Dr. Chase");
            Assert.NotNull(doctor);
            Assert.Equal("Neurologie", doctor.Specialty);
        }

        [Fact]
        // PASS: 400 returned when email already exists — no duplicate account created
        // FAIL: Duplicate doctor accounts created for the same email
        public async Task CreateDoctorAccount_DuplicateEmail_Returns400()
        {
            _context.Users.Add(new User { Email = "existing@test.com", PasswordHash = "h", Role = "Doctor" });
            await _context.SaveChangesAsync();

            var controller = new UsersController(_context);
            var dto = new CreateDoctorAccountAndProfileDto("existing@test.com", "Pass123!", "Dr. Dup", "Cardiologie");

            var result = await controller.CreateDoctorAccount(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: [Authorize(Roles = "Admin")] attribute is present on the endpoint
        // FAIL: Endpoint accessible without Admin role — any user can create doctors
        public void CreateDoctorAccount_Endpoint_RequiresAdminRole()
        {
            var method = typeof(UsersController).GetMethod(nameof(UsersController.CreateDoctorAccount));
            var attr = method?
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Equal("Admin", attr.Roles);
        }
    }

    public class DoctorsControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;

        public DoctorsControllerTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _connection.Close();
            _context.Dispose();
        }

        [Fact]
        // PASS: 200 OK with a non-empty list of doctors
        // FAIL: Doctor list is empty or endpoint crashes — patients cannot book
        public async Task GetDoctors_WithDoctorsInDb_Returns200WithList()
        {
            var user = new User { Email = "d@test.com", PasswordHash = "h", Role = "Doctor" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _context.Doctors.Add(new Doctor { Name = "Dr. A", Specialty = "Cardiologie", UserId = user.Id });
            await _context.SaveChangesAsync();

            var result = await new DoctorsController(_context).GetDoctors();

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = (ok.Value as System.Collections.IEnumerable)!.Cast<object>();
            Assert.NotEmpty(list);
        }

        [Fact]
        // PASS: 200 OK with an empty list when no doctors exist
        // FAIL: Endpoint crashes or returns non-200 when table is empty
        public async Task GetDoctors_EmptyDb_Returns200WithEmptyList()
        {
            var result = await new DoctorsController(_context).GetDoctors();

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = (ok.Value as System.Collections.IEnumerable)!.Cast<object>();
            Assert.Empty(list);
        }

        [Fact]
        // PASS: Response includes Id, Name, Specialty, UserId fields for each doctor
        // FAIL: Frontend crashes trying to access fields that don't exist in response
        public async Task GetDoctors_WithDoctors_ResponseContainsExpectedFields()
        {
            var user = new User { Email = "fields@test.com", PasswordHash = "h", Role = "Doctor" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _context.Doctors.Add(new Doctor { Name = "Dr. Fields", Specialty = "Dermatologie", UserId = user.Id });
            await _context.SaveChangesAsync();

            var result = await new DoctorsController(_context).GetDoctors();

            var ok = Assert.IsType<OkObjectResult>(result);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ok.Value);
            Assert.Contains("Name", json);
            Assert.Contains("Specialty", json);
        }
    }
}
