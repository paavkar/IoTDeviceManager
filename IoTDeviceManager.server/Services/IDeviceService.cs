using IoTDeviceManager.server.Models.Devices;

namespace IoTDeviceManager.server.Services
{
    public interface IDeviceService
    {
        Task<IEnumerable<Device>> GetDevicesAsync();
        Task<Device> GetDeviceAsync(string id);
        Task<dynamic> CreateDeviceAsync(Device device);
    }
}