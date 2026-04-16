using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Medical_Appointment_Scheduling_System_App.Data;
using Medical_Appointment_Scheduling_System_App.Models;
using Medical_Appointment_Scheduling_System_App.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_Appointment_Scheduling_System_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var loggedInUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole == "Patient")
            {
                if (appointment.Patient == null || appointment.Patient.UserId.ToString() != loggedInUserId)
                {
                    return Forbid();
                }
            }
            else if (userRole == "Doctor")
            {
                if (appointment.Doctor == null || appointment.Doctor.UserId.ToString() != loggedInUserId)
                {
                    return Forbid();
                }
            }

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
                AppointmentDate = dto.AppointmentDate.ToUniversalTime(),
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
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentResponseDto(
                    a.Id, a.DoctorId, a.PatientId, a.AppointmentDate, a.Status))
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Doctor, Admin")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound($"Eroare: Programarea cu ID-ul {id} nu a fost găsită.");
            }

            var terminalStatuses = new[] { "Cancelled", "Completed" };
            if (terminalStatuses.Contains(appointment.Status, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest($"Eroare: Programarea este deja în stadiul '{appointment.Status}' și nu mai poate fi modificată.");
            }

            var requestedStatus = dto.Status.Trim();

            if (requestedStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Eroare: Pentru a anula o programare, vă rugăm să folosiți endpoint-ul dedicat: PATCH /api/appointments/{id}/cancel");
            }

            var allowedStatuses = new[] { "Confirmed", "Completed" };

            if (!allowedStatuses.Contains(requestedStatus, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest($"Status invalid. Statusurile permise sunt: {string.Join(", ", allowedStatuses)}");
            }

            appointment.Status = allowedStatuses.First(s => s.Equals(requestedStatus, StringComparison.OrdinalIgnoreCase));

            await _context.SaveChangesAsync();

            var responsePayload = new AppointmentResponseDto(
                appointment.Id, appointment.DoctorId, appointment.PatientId, appointment.AppointmentDate, appointment.Status);

            return Ok(responsePayload);
        }

        [HttpPatch("{id}/cancel")]
        [Authorize(Roles = "Patient, Doctor, Admin")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound($"Eroare: Programarea cu ID-ul {id} nu a fost găsită.");
            }

            var loggedInUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole == "Patient" && (appointment.Patient == null || appointment.Patient.UserId.ToString() != loggedInUserId))
            {
                return Forbid();
            }

            if (userRole == "Doctor" && (appointment.Doctor == null || appointment.Doctor.UserId.ToString() != loggedInUserId))
            {
                return Forbid();
            }

            if (appointment.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Eroare: Programarea este deja anulată.");
            }

            if (appointment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Eroare: Programarea este deja finalizată.");
            }

            if (appointment.AppointmentDate < DateTime.Now)
            {
                return BadRequest("Eroare: Nu puteți anula o programare a cărei dată a trecut deja.");
            }

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            var responsePayload = new AppointmentResponseDto(
                appointment.Id, appointment.DoctorId, appointment.PatientId, appointment.AppointmentDate, appointment.Status);

            return Ok(responsePayload);
        }
    }
}