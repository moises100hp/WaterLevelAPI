namespace WaterLevelAPI
{
    public class WaterLevelDTO
    {
        public string DeviceId { get; set; }
        public double MinLevel { get; set; }
        public double MaxLevel { get; set; }
        public double CurrentLevel { get; set; }
    }
}
