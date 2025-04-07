using IoTDeviceManager.server.Models.Devices;
using IoTDeviceManager.server.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace IoTDeviceManager.server.CosmosDB
{
    public class CosmosDbService(
        CosmosDbFactory cosmosDbFactory,
        IConfiguration configuration,
        IAzureIoTService azureIotService) : ICosmosDbService
    {
        private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly Container DevicesContainer = cosmosDbFactory.CosmosClient.GetContainer(cosmosDbFactory.DatabaseName, "devices");

        public async Task<dynamic> CreateDeviceAsync(CDevice device)
        {
            if (!string.IsNullOrEmpty(device.UserId))
                device.PartitionKey = $"Devices-{device.UserId}";
            Guid guid = Guid.CreateVersion7();
            device.Id = guid.ToString();

            var guidString = guid.ToString("N");
            var hashBytes = Encoding.UTF8.GetBytes(guidString);

            using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            var hash = hmac.ComputeHash(hashBytes);
            var encoded = Base64UrlEncoder.Encode(hash);

            var serialNumber = CleanSerialNumber(encoded);
            DateTime now = DateTime.Now;
            serialNumber = $"IDM{now.Year}{now.Month}{now.Day}{serialNumber[..16]}";

            device.SerialNumber = serialNumber;
            var partitionKey = new PartitionKey(device.PartitionKey);
            var itemResponse = await DevicesContainer.CreateItemAsync(device, partitionKey);

            if (itemResponse.StatusCode == HttpStatusCode.Created)
            {
                var result = await azureIotService.CreateDeviceAsync(serialNumber);
                if (result.Succeeded && !string.IsNullOrEmpty(device.UserId))
                {
                    await azureIotService.UpdateDeviceTwinUserTagAsync(serialNumber, device.UserId!);
                }
                return new { Created = true, Device = itemResponse.Resource };
            }
            else
            {
                return new { Created = false };
            }
        }

        public static string CleanSerialNumber(string input)
        {
            var seedBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
            var seed = BitConverter.ToInt32(seedBytes, 0);
            Random rng = new(seed);

            StringBuilder sb = new(input.Length);
            foreach (var ch in input)
            {
                if (ch is '-' or '_')
                {
                    var index = rng.Next(AllowedCharacters.Length);
                    sb.Append(AllowedCharacters[index]);
                }
                else
                {
                    if (char.IsLetterOrDigit(ch) && char.IsUpper(ch))
                        sb.Append(ch);
                    else
                        sb.Append(char.ToUpperInvariant(ch));
                }
            }
            return sb.ToString();
        }

        public async Task<CDevice?> GetDeviceAsync(string serialNumber)
        {
            var getDevicesQuery = new QueryDefinition("SELECT * FROM devices d WHERE d.serialNumber = @SerialNumber")
                .WithParameter("@SerialNumber", serialNumber);

            var devices = await QueryDevicesAsync(getDevicesQuery);

            return devices.Count > 0 ? devices.First() : null;
        }

        public async Task<List<CDevice>> GetDevicesAsync(string userId)
        {
            var getDevicesQuery = new QueryDefinition("SELECT * FROM devices d WHERE d.userId = @UserId")
                .WithParameter("@UserId", userId);

            var devices = await QueryDevicesAsync(getDevicesQuery);

            return devices;
        }

        private async Task<List<CDevice>> QueryDevicesAsync(QueryDefinition query)
        {
            List<CDevice> devices = [];

            using FeedIterator<CDevice> feedIterator = DevicesContainer.GetItemQueryIterator<CDevice>(query);

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<CDevice> response = await feedIterator.ReadNextAsync();

                foreach (var device in response)
                {
                    devices.Add(device);
                }
            }

            return devices;
        }

        public async Task<bool> UpdateDeviceAsync(CDevice device)
        {
            var deviceResponse = await DevicesContainer.PatchItemAsync<CDevice>(
                device.Id!,
                new PartitionKey(device.PartitionKey),
                new[]
                {
                    PatchOperation.Replace("/isOnline", device.IsOnline),
                    PatchOperation.Replace("/lastConnectionTime", device.LastConnectionTime),
                    PatchOperation.Replace("/sensors", device.Sensors)
                });
            
            return deviceResponse.StatusCode.Equals(HttpStatusCode.OK);
        }
    }
}
