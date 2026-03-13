using System.ComponentModel.DataAnnotations;

namespace Medical_Appointment_Scheduling_System_App.DTOs
{
    public record RegisterDto(
        [Required(ErrorMessage = "Numele este obligatoriu.")]
        [MinLength(3, ErrorMessage = "Numele trebuie să aibă cel puțin 3 litere.")]
        string Name,

        [Required(ErrorMessage = "Telefonul este obligatoriu.")]
        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Telefonul trebuie să aibă exact 10 cifre și să înceapă cu 0.")]
        string Phone,

        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Te rugăm să introduci un email valid (ex: nume@domeniu.com).")]
        string Email,

        [Required(ErrorMessage = "Parola este obligatorie.")]
        [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere pentru siguranță.")]
        string Password
    );

    public record LoginDto(
        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Formatul email-ului este invalid.")]
        string Email,

        [Required(ErrorMessage = "Parola este obligatorie.")]
        string Password
    );

    public record AuthResponseDto(string Token, string Message);
}