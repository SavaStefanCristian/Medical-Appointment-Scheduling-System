using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.Services;
using Medical_Appointment_Scheduling_System_App.Utilities;

namespace Medical_Appointment_Scheduling_System_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Date invalide.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Eroare: Acest email este deja folosit.");

            var newUser = new User
            {
                Email = dto.Email,
                PasswordHash = PasswordHasher.HashPassword(dto.Password),
                Role = "Patient"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var newPatient = new Patient
            {
                Name = dto.Name,
                Phone = dto.Phone,
                UserId = newUser.Id
            };

            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();

            var accessToken = _jwtService.GenerateToken(newUser.Id.ToString(), newUser.Email, newUser.Role);

            return Ok(new AuthResponseDto(accessToken, "Cont creat și autentificare automată cu succes!"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest("Date invalide.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized("Eroare: Email sau parolă incorectă.");

            var accessToken = _jwtService.GenerateToken(user.Id.ToString(), user.Email, user.Role);

            return Ok(new AuthResponseDto(accessToken, "Autentificare cu succes!"));
        }
    }
}