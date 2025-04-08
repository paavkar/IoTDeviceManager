using Microsoft.Azure.Devices.Shared;

namespace IoTDeviceManager.server.Services
{
    public interface IAzureIoTService
    {
        Task<dynamic> CreateDeviceAsync(string serialNumber);
        Task<List<Twin>> QueryAllDevicesAsync();
        Task<List<Twin>> QueryDevicesByUserIdAsync(string userId);
        Task SendCommandAsync(string deviceId, string commandPayload);
        Task<dynamic> UpdateDeviceTwinUserTagAsync(string deviceId, string userId);
    }
}