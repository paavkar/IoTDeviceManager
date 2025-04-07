

using Newtonsoft.Json;

namespace IoTDeviceManager.server.Models.Devices
{
    public class CSensorReading
    {
        [JsonProperty(PropertyName = "measurementType")]
        public string? MeasurementType { get; set; }
        [JsonProperty(PropertyName = "unit")]
        public string? Unit { get; set; }
        [JsonProperty(PropertyName = "reading")]
        public double Reading { get; set; }
    }
}
