using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.DTOs;

namespace Medical_Appointment_Scheduling_System_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Specialty,
                    d.UserId
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDto doctorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(doctorDto.UserId);

            if (user == null)
            {
                return BadRequest("Eroare: Nu există niciun User cu acest Id în baza de date.");
            }

            if (user.Role != "Doctor")
            {
                return BadRequest($"Eroare: User-ul specificat are rolul de '{user.Role}', nu de 'Doctor'. Nu poate fi adăugat ca medic.");
            }

            var newDoctor = new Doctor
            {
                Name = doctorDto.Name,
                Specialty = doctorDto.Specialty,
                UserId = doctorDto.UserId
            };

            _context.Doctors.Add(newDoctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDoctors), new { id = newDoctor.Id }, newDoctor);
        }
    }
}