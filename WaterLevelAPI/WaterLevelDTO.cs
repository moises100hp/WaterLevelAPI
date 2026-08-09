using System.ComponentModel.DataAnnotations;

namespace WaterLevelAPI
{
    public class WaterLevelDTO
    {
        [Required(ErrorMessage = "DeviceId é obrigatório.")]
        public string DeviceId { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "MinLevel não pode ser negativo.")]
        public double MinLevel { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MaxLevel não pode ser negativo.")]
        public double MaxLevel { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "CurrentLevel não pode ser negativo.")]
        public double CurrentLevel { get; set; }
    }
}
