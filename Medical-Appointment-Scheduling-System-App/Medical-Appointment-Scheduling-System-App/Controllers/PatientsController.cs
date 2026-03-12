using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.DTOs;

namespace Medical_Appointment_Scheduling_System_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Patients
                .Select(p => new PatientResponseDto(p.Id, p.Name, p.Phone, p.UserId))
                .ToListAsync();

            return Ok(patients);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto patientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(patientDto.UserId);
            if (user == null)
            {
                return BadRequest("Eroare: Nu există niciun User cu acest Id în baza de date.");
            }

            if (user.Role != "Patient")
            {
                return BadRequest($"Eroare: User-ul specificat are rolul de '{user.Role}', nu de 'Patient'.");
            }

            var newPatient = new Patient
            {
                Name = patientDto.Name,
                Phone = patientDto.Phone,
                UserId = patientDto.UserId
            };

            _context.Patients.Add(newPatient);
            await _context.SaveChangesAsync();

            var responsePayload = new PatientResponseDto(newPatient.Id, newPatient.Name, newPatient.Phone, newPatient.UserId);

            return CreatedAtAction(nameof(GetPatients), new { id = newPatient.Id }, responsePayload);
        }
    }

    public record PatientResponseDto(int Id, string Name, string Phone, int UserId);
}