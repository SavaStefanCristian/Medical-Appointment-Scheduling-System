using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Utilities;
using System.Threading.Tasks;

namespace Medical_Appointment_Scheduling_System_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("doctor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDoctorAccount([FromBody] CreateDoctorAccountAndProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
            {
                return BadRequest("Eroare: Există deja un utilizator înregistrat cu acest email.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var hashedPassword = PasswordHasher.HashPassword(dto.Password);

                var newUser = new User
                {
                    Email = dto.Email,
                    PasswordHash = hashedPassword,
                    Role = "Doctor"
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                var newDoctor = new Doctor
                {
                    Name = dto.Name,
                    Specialty = dto.Specialty,
                    UserId = newUser.Id
                };

                _context.Doctors.Add(newDoctor);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Created(string.Empty, new
                {
                    UserId = newUser.Id,
                    DoctorId = newDoctor.Id,
                    Email = newUser.Email,
                    Role = newUser.Role,
                    Name = newDoctor.Name,
                    Specialty = newDoctor.Specialty
                });
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Eroare internă la crearea contului: {ex.Message}");
            }
        }
    }
}