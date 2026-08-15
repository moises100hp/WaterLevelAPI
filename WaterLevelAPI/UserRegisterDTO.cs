using System.ComponentModel.DataAnnotations;

namespace WaterLevelAPI
{
    public class UserRegisterDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MinLength(2, ErrorMessage = "Nome deve ter pelo menos 2 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }
}
