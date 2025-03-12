namespace IoTDeviceManager.server.Models.Devices
{
    public class Sensor
    {
        public string? Id { get; set; }
        public bool IsOnline { get; set; } = false;
        public DateTimeOffset? LastConnectionTime { get; set; }
        public string MeasurementType { get; set; }
        public string Unit { get; set; }
        public string Name { get; set; }
        public double LatestReading { get; set; }
        public string DeviceSerialNumber { get; set; }
    }
}
