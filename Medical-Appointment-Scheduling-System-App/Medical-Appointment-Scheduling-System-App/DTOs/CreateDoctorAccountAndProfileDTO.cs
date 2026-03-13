using System.ComponentModel.DataAnnotations;

namespace Medical_Appointment_Scheduling_System_App.DTOs
{
    public record CreateDoctorAccountAndProfileDto(
        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Formatul email-ului este invalid.")]
        string Email,

        [Required(ErrorMessage = "Parola este obligatorie.")]
        [MinLength(6, ErrorMessage = "Parola trebuie să aibă minim 6 caractere.")]
        string Password,

        [Required(ErrorMessage = "Numele medicului este obligatoriu.")]
        string Name,

        [Required(ErrorMessage = "Specializarea este obligatorie.")]
        string Specialty
    );
}