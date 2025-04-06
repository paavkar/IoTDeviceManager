using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Rest;
using Newtonsoft.Json;
using System.Text;

namespace IoTDeviceManager.server.Services
{
    public class AzureIoTService
    {
        private readonly string _registryConnectionString = Environment.GetEnvironmentVariable("AZURE_IOT_HUB_CONNECTION_STRING")!;
        private readonly string _serviceConnectionString = Environment.GetEnvironmentVariable("AZURE_IOT_SERVICE_CONNECTION_STRING")!;
        private RegistryManager _registryManager;
        private readonly ServiceClient _serviceClient;

        public AzureIoTService()
        {
            _registryManager = RegistryManager.CreateFromConnectionString(_registryConnectionString);
            _serviceClient = ServiceClient.CreateFromConnectionString(_serviceConnectionString);
        }

        public async Task SendCommandAsync(string deviceId, string commandPayload)
        {
            var commandMessage = new Message(Encoding.UTF8.GetBytes(commandPayload));
            await _serviceClient.SendAsync(deviceId, commandMessage);
        }

        public async Task<dynamic> CreateDeviceAsync(string serialNumber)
        {
            try
            {
                var device = new Device(serialNumber);

                device = await _registryManager.AddDeviceAsync(device);

                return new { Succeeded = true, Message = "Device created successfully.", Device = device };
            }
            catch (DeviceAlreadyExistsException)
            {
                return new { Succeeded = false, Message = "Device already exists." };
            }
            catch (Exception ex)
            {
                return new { Succeeded = false, Message = "Error creating device.", Error = ex.Message };
            }
        }

        public async Task<List<Twin>> QueryAllDevicesAsync()
        {
            var twins = new List<Twin>();

            var queryString = $"SELECT * FROM devices";

            var query = _registryManager.CreateQuery(queryString);

            while (query.HasMoreResults)
            {
                var twinPage = await query.GetNextAsTwinAsync();
                twins.AddRange(twinPage);
            }

            return twins;
        }

        public async Task<List<Twin>> QueryDevicesByUserIdAsync(string userId)
        {
            var twins = new List<Twin>();

            var queryString = $"SELECT * FROM devices WHERE tags.userId = '{userId}'";

            var query = _registryManager.CreateQuery(queryString);

            while (query.HasMoreResults)
            {
                var twinPage = await query.GetNextAsTwinAsync();
                twins.AddRange(twinPage);
            }

            return twins;
        }

        public async Task<dynamic> UpdateDeviceTwinUserTagAsync(string deviceId, string userId)
        {
            Twin twin = await _registryManager.GetTwinAsync(deviceId);

            var patchDocument = new
            {
                tags = new
                {
                    userId = userId
                }
            };

            var patch = JsonConvert.SerializeObject(patchDocument);

            Twin updatedTwin = await _registryManager.UpdateTwinAsync(deviceId, patch, twin.ETag);

            if (updatedTwin == null)
            {
                return new { Succeeded = false, Message = "Failed to update device twin." };
            }

            return new { Succeeded = true, Message = "Device twin updated successfully.", Tags = updatedTwin.Tags.ToJson() };
        }
    }
}
