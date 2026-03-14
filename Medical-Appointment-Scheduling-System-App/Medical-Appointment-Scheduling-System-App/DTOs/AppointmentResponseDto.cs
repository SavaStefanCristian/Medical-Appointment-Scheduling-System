using System;

namespace Medical_Appointment_Scheduling_System_App.DTOs
{
    public record AppointmentResponseDto(int Id, int DoctorId, int PatientId, DateTime AppointmentDate, string Status);
}
