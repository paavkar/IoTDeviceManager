namespace IoTHub.Models
{
    public class Device
    {
        public string? id { get; set; }
        public bool isOnline { get; set; } = false;
        public DateTimeOffset? lastConnectionTime { get; set; }
        public string? name { get; set; }
        public string? serialNumber { get; set; }
        public string? userId { get; set; }
        public string? partitionKey { get; set; } = "Devices";
        public string type { get; set; } = "Device";
        public List<Sensor> sensors { get; set; } = [];
    }
}
