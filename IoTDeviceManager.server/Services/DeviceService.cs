using Dapper;
using IoTDeviceManager.server.Models.Devices;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace IoTDeviceManager.server.Services
{
    public class DeviceService(IConfiguration configuration) : IDeviceService
    {
        private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private SqlConnection GetConnection() => new(configuration.GetConnectionString("DefaultConnection"));

        public async Task<dynamic> CreateDeviceAsync(Device device)
        {
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

            using SqlConnection connection = GetConnection();
            connection.Open();
            using SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                var insertionCommand = """
                    INSERT INTO Devices (Id, IsOnline, Name, SerialNumber, UserId)
                    VALUES (@Id, @IsOnline, @Name, @SerialNumber, @UserId)
                    """;

                var rowsAffected = await connection.ExecuteAsync(insertionCommand, device, transaction);
                transaction.Commit();

                return new { Created = rowsAffected > 0, Device = device };
            }
            catch (Exception)
            {
                transaction.Rollback();
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


        public Task<Device> GetDeviceAsync(string id) => throw new NotImplementedException();
        public Task<IEnumerable<Device>> GetDevicesAsync() => throw new NotImplementedException();
    }
}
