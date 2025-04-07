using Newtonsoft.Json;

namespace IoTDeviceManager.server.Models.Devices
{
    public class CSensor
    {
        [JsonProperty(PropertyName = "isOnline")]
        public bool IsOnline { get; set; } = false;
        [JsonProperty(PropertyName = "lastConnectionTime")]
        public DateTimeOffset? LastConnectionTime { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string? Name { get; set; }
        [JsonProperty(PropertyName = "latestReadings")]
        public List<CSensorReading> LatestReadings { get; set; } = [];
    }
}
