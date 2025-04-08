namespace IoTHub.Models
{
    public class Sensor
    {
        public bool isOnline { get; set; } = false;
        public DateTimeOffset? lastConnectionTime { get; set; }
        public string? name { get; set; }
        public List<SensorReading> latestReadings { get; set; } = [];
    }
}
