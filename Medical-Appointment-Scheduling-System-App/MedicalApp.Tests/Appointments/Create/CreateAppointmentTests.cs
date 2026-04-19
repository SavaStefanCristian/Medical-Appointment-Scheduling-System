using Medical_Appointment_Scheduling_System_App.Controllers;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MedicalApp.Tests.Appointments.Create
{
    public class CreateAppointmentTests : AppointmentTestBase
    {
        [Fact]
        // PASS: 201 Created returned, appointment has Status = "Pending"
        // FAIL: Appointment not saved or wrong status assigned on creation
        public async Task CreateAppointment_ValidData_Returns201WithPendingStatus()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var (patUser, patient) = await SeedPatientAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            var result = await controller.CreateAppointment(
                new CreateAppointmentDto(doctor.Id, patient.Id, DateTime.UtcNow.AddDays(5)));

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<AppointmentResponseDto>(created.Value);
            Assert.Equal("Pending", response.Status);
        }

        [Fact]
        // PASS: 400 Bad Request returned when doctorId does not exist in DB
        // FAIL: Appointment created with invalid doctor — data integrity broken
        public async Task CreateAppointment_NonExistentDoctor_Returns400()
        {
            var (patUser, patient) = await SeedPatientAsync();
            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            var result = await controller.CreateAppointment(
                new CreateAppointmentDto(9999, patient.Id, DateTime.UtcNow.AddDays(5)));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: 400 Bad Request returned when patientId does not exist in DB
        // FAIL: Appointment created with invalid patient — data integrity broken
        public async Task CreateAppointment_NonExistentPatient_Returns400()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, "1", "Patient");

            var result = await controller.CreateAppointment(
                new CreateAppointmentDto(doctor.Id, 9999, DateTime.UtcNow.AddDays(5)));

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        // PASS: Appointment record exists in DB after successful creation
        // FAIL: Controller returns 201 but nothing was saved — DB out of sync
        public async Task CreateAppointment_ValidData_AppointmentSavedInDb()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var (patUser, patient) = await SeedPatientAsync();
            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, patUser.Id.ToString(), "Patient");

            await controller.CreateAppointment(
                new CreateAppointmentDto(doctor.Id, patient.Id, DateTime.UtcNow.AddDays(7)));

            var saved = await _context.Appointments
                .FirstOrDefaultAsync(a => a.DoctorId == doctor.Id && a.PatientId == patient.Id);
            Assert.NotNull(saved);
        }

        [Fact]
        // PASS: Multiple appointments can be created for the same doctor
        // FAIL: Second appointment rejected — doctor limited to one appointment
        public async Task CreateAppointment_MultipleForSameDoctor_AllSaved()
        {
            var (_, doctor) = await SeedDoctorAsync();
            var (patUser1, patient1) = await SeedPatientAsync("p1@test.com");
            var (patUser2, patient2) = await SeedPatientAsync("p2@test.com");
            var controller = new AppointmentsController(_context);

            SetUserOnController(controller, patUser1.Id.ToString(), "Patient");
            await controller.CreateAppointment(new CreateAppointmentDto(doctor.Id, patient1.Id, DateTime.UtcNow.AddDays(1)));

            SetUserOnController(controller, patUser2.Id.ToString(), "Patient");
            await controller.CreateAppointment(new CreateAppointmentDto(doctor.Id, patient2.Id, DateTime.UtcNow.AddDays(2)));

            var count = await _context.Appointments.CountAsync(a => a.DoctorId == doctor.Id);
            Assert.Equal(2, count);
        }
    }
}
