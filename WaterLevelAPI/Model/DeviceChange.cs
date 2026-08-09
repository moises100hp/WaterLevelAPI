using System.ComponentModel.DataAnnotations;

namespace WaterLevelAPI.Model
{
    public class DeviceChange
    {
        [Key]
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public bool StatusChanged { get; set; }
    }
}
