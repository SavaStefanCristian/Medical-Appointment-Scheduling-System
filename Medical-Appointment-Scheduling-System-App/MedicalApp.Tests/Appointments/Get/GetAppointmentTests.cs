using Medical_Appointment_Scheduling_System_App.Controllers;
using Medical_Appointment_Scheduling_System_App.DTOs;
using Medical_Appointment_Scheduling_System_App.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MedicalApp.Tests.Appointments.Get
{
    public class GetAppointmentTests : AppointmentTestBase
    {
        [Fact]
        // PASS: 200 OK with appointment data when doctor requests their own appointment
        // FAIL: Doctor cannot view their own appointment details
        public async Task GetAppointment_ExistingId_AsDoctor_Returns200()
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

            var result = await controller.GetAppointment(appointment.Id);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        // PASS: 200 OK with correct appointment fields in response
        // FAIL: Response shape is wrong — frontend cannot parse appointment data
        public async Task GetAppointment_ExistingId_ResponseHasCorrectFields()
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

            var result = await controller.GetAppointment(appointment.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AppointmentResponseDto>(ok.Value);
            Assert.Equal(appointment.Id, response.Id);
            Assert.Equal("Pending", response.Status);
        }

        [Fact]
        // PASS: 404 Not Found returned for non-existent appointment ID
        // FAIL: Server crashes or returns wrong data for missing records
        public async Task GetAppointment_NonExistentId_Returns404()
        {
            var (docUser, _) = await SeedDoctorAsync();
            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.GetAppointment(99999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        // PASS: 200 OK with list containing the doctor's appointments
        // FAIL: Doctor cannot see their schedule — list is empty or endpoint crashes
        public async Task GetDoctorAppointments_DoctorHasAppointments_Returns200WithList()
        {
            var (docUser, doctor) = await SeedDoctorAsync();
            var (_, patient) = await SeedPatientAsync();

            _context.Appointments.Add(new Appointment
            {
                DoctorId = doctor.Id, PatientId = patient.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(1), Status = "Pending"
            });
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.GetDoctorAppointments(doctor.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<AppointmentResponseDto>>(ok.Value);
            Assert.NotEmpty(list);
        }

        [Fact]
        // PASS: 200 OK with empty list when doctor has no appointments yet
        // FAIL: Endpoint crashes or returns 404 instead of empty list
        public async Task GetDoctorAppointments_NoAppointments_Returns200WithEmptyList()
        {
            var (docUser, doctor) = await SeedDoctorAsync();
            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.GetDoctorAppointments(doctor.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<AppointmentResponseDto>>(ok.Value);
            Assert.Empty(list);
        }

        [Fact]
        // PASS: 404 Not Found returned when doctorId does not exist
        // FAIL: Endpoint returns empty list instead of 404 — silent wrong result
        public async Task GetDoctorAppointments_NonExistentDoctor_Returns404()
        {
            var (docUser, _) = await SeedDoctorAsync();
            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser.Id.ToString(), "Doctor");

            var result = await controller.GetDoctorAppointments(99999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // PASS: Only appointments belonging to the requested doctor are returned
        // FAIL: Appointments from other doctors leak into the response
        public async Task GetDoctorAppointments_OnlyReturnsThatDoctorsAppointments()
        {
            var (docUser1, doctor1) = await SeedDoctorAsync("doc1@test.com");
            var (_, doctor2) = await SeedDoctorAsync("doc2@test.com");
            var (_, patient) = await SeedPatientAsync();

            _context.Appointments.Add(new Appointment { DoctorId = doctor1.Id, PatientId = patient.Id, AppointmentDate = DateTime.UtcNow.AddDays(1), Status = "Pending" });
            _context.Appointments.Add(new Appointment { DoctorId = doctor2.Id, PatientId = patient.Id, AppointmentDate = DateTime.UtcNow.AddDays(2), Status = "Pending" });
            await _context.SaveChangesAsync();

            var controller = new AppointmentsController(_context);
            SetUserOnController(controller, docUser1.Id.ToString(), "Doctor");

            var result = await controller.GetDoctorAppointments(doctor1.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<AppointmentResponseDto>>(ok.Value).ToList();
            Assert.Single(list);
            Assert.All(list, a => Assert.Equal(doctor1.Id, a.DoctorId));
        }
    }
}
