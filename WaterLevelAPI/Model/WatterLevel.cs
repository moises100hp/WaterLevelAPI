using System.ComponentModel.DataAnnotations;

namespace WaterLevelAPI.Model
{
    public class WatterLevel
    {
        [Key]
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public double CurrentLevel { get; set; }
        public double MinLevel { get; set; }
        public double MaxLevel { get; set; }
        public DateTime TimesTamp { get; set; }
    }
}
