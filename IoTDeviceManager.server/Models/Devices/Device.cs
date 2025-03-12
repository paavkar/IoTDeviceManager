namespace IoTDeviceManager.server.Models.Devices
{
    public class Device
    {
        public string? Id { get; set; }
        public bool IsOnline { get; set; } = false;
        public DateTimeOffset? LastConnectionTime { get; set; }
        public string Name { get; set; }
        public string? SerialNumber { get; set; }
        public string? UserId { get; set; }

        // Not saved on database
        public List<Sensor>? Sensors { get; set; }
    }
}
