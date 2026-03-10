namespace Medical_Appointment_Scheduling_System_App.DTOs
{
    public class CreateDoctorDto
    {
        public required string Name { get; set; }
        public required string Specialty { get; set; }
        public int UserId { get; set; }
    }
}