using Medical_Appointment_Scheduling_System_App.Controllers;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace MedicalApp.Tests.Appointments.Status
{
    public class UpdateAppointmentStatusTests : AppointmentTestBase
    {
        [Fact]
        // PASS: 200 OK, status updated to "Confirmed" in response
        // FAIL: Doctor cannot confirm appointments — schedule management broken
        public async Task UpdateStatus_PendingToConfirmed_Returns200()
        {
            var (docUser, doctor) = await SeedDoctorAsync();
            var (_, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(3), Status = "Pending"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.UpdateAppointmentStatus(appointment.Id, new UpdateAppointmentStatusDto("Confirmed"));

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AppointmentResponseDto>(ok.Value);
            Assert.Equal("Confirmed", response.Status);
        }

        [Fact]
        // PASS: 200 OK, status updated to "Completed"
        // FAIL: Doctor cannot mark appointments as done — workflow stuck at Confirmed
        public async Task UpdateStatus_ToCompleted_Returns200()
        {
            var (docUser, doctor) = await SeedDoctorAsync();
            var (_, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(3), Status = "Confirmed"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.UpdateAppointmentStatus(appointment.Id, new UpdateAppointmentStatusDto("Completed"));

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AppointmentResponseDto>(ok.Value);
            Assert.Equal("Completed", response.Status);
        }

        [Fact]
        // PASS: 400 Bad Request for an unrecognized status value
        // FAIL: Arbitrary status strings accepted — DB contains garbage data
        public async Task UpdateStatus_InvalidStatus_Returns400()
        {
            var (docUser, doctor) = await SeedDoctorAsync();
            var (_, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(3), Status = "Pending"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.UpdateAppointmentStatus(appointment.Id, new UpdateAppointmentStatusDto("Garbage"));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: 400 Bad Request — Cancelled appointments are terminal, no edits allowed
        // FAIL: Cancelled appointments can be reactivated — invalid state transition
        public async Task UpdateStatus_CancelledAppointment_Returns400()
        {
            var (docUser, doctor) = await SeedDoctorAsync();
            var (_, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(3), Status = "Cancelled"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.UpdateAppointmentStatus(appointment.Id, new UpdateAppointmentStatusDto("Confirmed"));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: 400 Bad Request — Completed appointments are terminal, no edits allowed
        // FAIL: Completed appointments can be changed — historical data corrupted
        public async Task UpdateStatus_CompletedAppointment_Returns400()
        {
            var (docUser, doctor) = await SeedDoctorAsync();
            var (_, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(3), Status = "Completed"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.UpdateAppointmentStatus(appointment.Id, new UpdateAppointmentStatusDto("Confirmed"));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: 400 — Cancel via status endpoint blocked, must use /cancel endpoint
        // FAIL: Cancel can be triggered from two different endpoints — logic inconsistency
        public async Task UpdateStatus_AttemptCancelViaWrongEndpoint_Returns400()
        {
            var (docUser, doctor) = await SeedDoctorAsync();
            var (_, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(3), Status = "Pending"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.UpdateAppointmentStatus(appointment.Id, new UpdateAppointmentStatusDto("Cancelled"));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: 404 Not Found for a non-existent appointment ID
        // FAIL: Server crashes or returns 200 for missing records
        public async Task UpdateStatus_NonExistentAppointment_Returns404()
        {
            var (docUser, _) = await SeedDoctorAsync();
            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.UpdateAppointmentStatus(99999, new UpdateAppointmentStatusDto("Confirmed"));

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // PASS: [Authorize(Roles = "Doctor, Admin")] attribute present on the method
        // FAIL: Patients can change appointment status — role restriction missing
        public void UpdateStatus_Endpoint_RequiresDoctorOrAdminRole()
        {
            var method = typeof(AppointmentsController)
                .GetMethod(nameof(AppointmentsController.UpdateAppointmentStatus));
            var attr = method?
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .FirstOrDefault();

            Assert.NotNull(attr);
            Assert.Contains("Doctor", attr.Roles);
        }
    }
}
