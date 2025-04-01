using IoTDeviceManager.server.Models.Devices;

namespace IoTDeviceManager.server.Services
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetDevicesAsync(string userId);
        Task<Device> GetDeviceAsync(string serialNumber);
        Task<dynamic> CreateDeviceAsync(Device device);
    }
}