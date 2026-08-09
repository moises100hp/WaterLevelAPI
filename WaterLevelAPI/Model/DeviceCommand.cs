using System.ComponentModel.DataAnnotations;

namespace WaterLevelAPI.Model
{
    public class DeviceCommand
    {
        [Key]
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public bool PumpOn { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
