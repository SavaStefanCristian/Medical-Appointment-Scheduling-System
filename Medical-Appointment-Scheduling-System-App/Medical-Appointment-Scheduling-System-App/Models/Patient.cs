namespace Medical_Appointment_Scheduling_System_App.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Phone { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}