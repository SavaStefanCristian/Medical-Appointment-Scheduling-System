namespace Medical_Appointment_Scheduling_System_App.DTOs
{
    public record CreateAppointmentDto(int DoctorId, int PatientId, DateTime AppointmentDate);
}