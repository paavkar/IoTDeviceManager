using IoTDeviceManager.server.Models.Devices;

namespace IoTDeviceManager.server.Services
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetDevicesAsync(string userId);
        Task<Device> GetDeviceAsync(string serialNumber);
        Task<dynamic> CreateDeviceAsync(Device device);
        Task<bool> UpdateDeviceAsync(Device device);
        Task<bool> UpdateDeviceSensorAsync(Device device, Sensor sensor);
        Task<Sensor?> GetExistingSensorAsync(string sensorName, string deviceSerialNumber);
    }
}