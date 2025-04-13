using System.Data;

namespace IoTDeviceManager.server.Models
{
    public class DeviceCommandRequest
    {
        public CommandType CommandType { get; set; }
        public int? Value { get; set; }
    }
}
