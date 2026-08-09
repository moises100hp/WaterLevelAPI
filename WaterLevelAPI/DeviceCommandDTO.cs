using System.ComponentModel.DataAnnotations;

namespace WaterLevelAPI
{
    public class DeviceCommandDTO
    {
        [Required(ErrorMessage = "DeviceId é obrigatório.")]
        public string DeviceId { get; set; } = string.Empty;

        public bool PumpOn { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
