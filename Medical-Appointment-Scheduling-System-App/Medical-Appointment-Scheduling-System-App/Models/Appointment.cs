namespace Medical_Appointment_Scheduling_System_App.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public required string Status { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
    }
}
