namespace Medical_Appointment_Scheduling_System_App.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; }

        public Doctor? DoctorProfile { get; set; }
        public Patient? PatientProfile { get; set; }
    }
}
