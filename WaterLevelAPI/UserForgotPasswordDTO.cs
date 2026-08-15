using System.ComponentModel.DataAnnotations;

namespace WaterLevelAPI
{
    public class UserForgotPasswordDTO
    {
        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;
    }
}
