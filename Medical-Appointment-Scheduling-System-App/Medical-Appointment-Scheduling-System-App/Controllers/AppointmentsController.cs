using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.DTOs;

namespace Medical_Appointment_Scheduling_System_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var response = new AppointmentResponseDto(
                appointment.Id, appointment.DoctorId, appointment.PatientId, appointment.AppointmentDate, appointment.Status);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == dto.DoctorId);
            if (!doctorExists)
            {
                return BadRequest("Eroare: Doctorul cu ID-ul specificat nu a fost găsit.");
            }

            var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
            if (!patientExists)
            {
                return BadRequest("Eroare: Pacientul cu ID-ul specificat nu a fost găsit.");
            }

            var newAppointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                AppointmentDate = dto.AppointmentDate,
                Status = "Pending"
            };

            _context.Appointments.Add(newAppointment);
            await _context.SaveChangesAsync();

            var responsePayload = new AppointmentResponseDto(
                newAppointment.Id,
                newAppointment.DoctorId,
                newAppointment.PatientId,
                newAppointment.AppointmentDate,
                newAppointment.Status
            );

            return CreatedAtAction(nameof(GetAppointment), new { id = newAppointment.Id }, responsePayload);
        }
    }

    public record AppointmentResponseDto(int Id, int DoctorId, int PatientId, DateTime AppointmentDate, string Status);
}