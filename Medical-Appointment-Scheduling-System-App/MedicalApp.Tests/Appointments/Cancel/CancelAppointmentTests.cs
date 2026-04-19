using Medical_Appointment_Scheduling_System_App.Controllers;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MedicalApp.Tests.Appointments.Cancel
{
    public class CancelAppointmentTests : AppointmentTestBase
    {
        [Fact]
        // PASS: 200 OK, response has Status = "Cancelled"
        // FAIL: Future appointments cannot be cancelled — core patient feature broken
        public async Task CancelAppointment_FutureAppointment_Returns200WithCancelledStatus()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var (patUser, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(5), Status = "Pending"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            var result = await controller.CancelAppointment(appointment.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AppointmentResponseDto>(ok.Value);
            Assert.Equal("Cancelled", response.Status);
        }

        [Fact]
        // PASS: Status updated to "Cancelled" in the database
        // FAIL: Response says Cancelled but DB still shows Pending — state mismatch
        public async Task CancelAppointment_FutureAppointment_StatusUpdatedInDb()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var (patUser, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(5), Status = "Pending"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            await controller.CancelAppointment(appointment.Id);

            var updated = await _context.Appointments.FindAsync(appointment.Id);
            Assert.Equal("Cancelled", updated!.Status);
        }

        [Fact]
        // PASS: 400 Bad Request returned — past appointments cannot be cancelled
        // FAIL: Past appointments cancelled — business rule not enforced
        public async Task CancelAppointment_PastAppointment_Returns400()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var (patUser, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(-3), Status = "Pending"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            var result = await controller.CancelAppointment(appointment.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: 404 Not Found for a non-existent appointment ID
        // FAIL: Server crashes or returns wrong status for missing records
        public async Task CancelAppointment_NonExistentAppointment_Returns404()
        {
            var (patUser, _) = await SeedPatientAsync();
            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            var result = await controller.CancelAppointment(99999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // PASS: 400 Bad Request — idempotent cancel blocked to avoid confusion
        // FAIL: Cancelling twice allowed — status reset silently, data inconsistent
        public async Task CancelAppointment_AlreadyCancelled_Returns400()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var (patUser, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(5), Status = "Cancelled"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            var result = await controller.CancelAppointment(appointment.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: 400 Bad Request — completed appointments cannot be cancelled
        // FAIL: Completed appointments reverted to Cancelled — invalid state transition
        public async Task CancelAppointment_AlreadyCompleted_Returns400()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var (patUser, patient) = await SeedPatientAsync();

            var appointment = new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(5), Status = "Completed"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            var result = await controller.CancelAppointment(appointment.Id);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
