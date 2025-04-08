namespace IoTHub.Models
{
    public class SensorReading
    {
        public string? measurementType { get; set; }
        public string? unit { get; set; }
        public double reading { get; set; }
    }
}
