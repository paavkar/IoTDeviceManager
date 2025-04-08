using Newtonsoft.Json;

namespace IoTDeviceManager.server.Models.Devices
{
    public class CDevice
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }
        [JsonProperty(PropertyName = "isOnline")]
        public bool IsOnline { get; set; } = false;
        [JsonProperty(PropertyName = "lastConnectionTime")]
        public DateTimeOffset? LastConnectionTime { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string? Name { get; set; }
        [JsonProperty(PropertyName = "serialNumber")]
        public string? SerialNumber { get; set; }
        [JsonProperty(PropertyName = "userId")]
        public string? UserId { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string? PartitionKey { get; set; } = "Devices";
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = "Device";
        [JsonProperty(PropertyName = "sensors")]
        public List<CSensor> Sensors { get; set; } = [];
    }
}
