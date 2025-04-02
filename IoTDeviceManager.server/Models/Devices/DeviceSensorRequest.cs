namespace IoTDeviceManager.server.Models.Devices
{
    public class DeviceSensorRequest
    {
        public string Name { get; set; }
        public string MeasurementType { get; set; }
        public string Unit { get; set; }
        public double LatestReading { get; set; }
    }
}
