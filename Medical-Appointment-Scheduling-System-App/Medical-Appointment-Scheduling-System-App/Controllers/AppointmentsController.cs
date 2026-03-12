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

        // TICHET #4: Crearea programării (POST /api/appointments)
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

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorAppointments(int doctorId)
        {
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!doctorExists)
            {
                return NotFound($"Eroare: Doctorul cu ID-ul {doctorId} nu a fost găsit.");
            }

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => new AppointmentResponseDto(
                    a.Id, a.DoctorId, a.PatientId, a.AppointmentDate, a.Status))
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound($"Eroare: Programarea cu ID-ul {id} nu a fost găsită.");
            }

            var allowedStatuses = new[] { "Confirmed", "Cancelled", "Completed" };
            if (!allowedStatuses.Contains(dto.Status))
            {
                return BadRequest($"Status invalid. Statusurile permise sunt: {string.Join(", ", allowedStatuses)}");
            }

            appointment.Status = dto.Status;
            await _context.SaveChangesAsync();

            var responsePayload = new AppointmentResponseDto(
                appointment.Id, appointment.DoctorId, appointment.PatientId, appointment.AppointmentDate, appointment.Status);

            return Ok(responsePayload);
        }
    }

    public record AppointmentResponseDto(int Id, int DoctorId, int PatientId, DateTime AppointmentDate, string Status);
}