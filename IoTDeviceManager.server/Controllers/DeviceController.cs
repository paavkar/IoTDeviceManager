using IoTDeviceManager.server.Models.Auth;
using IoTDeviceManager.server.Models.Devices;
using IoTDeviceManager.server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IoTDeviceManager.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController(TokenService tokenService, UserManager<ApplicationUser> userManager, IDeviceService deviceService) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetDevices()
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];
            dynamic result = await tokenService.ValidateTokensAsync(accessToken!, refreshToken!);

            if (!result.Success)
                return Unauthorized(new { Message = result.Message });

            string userId = result.UserId;
            IEnumerable<Device> devices = await deviceService.GetDevicesAsync(userId);

            return Ok(devices);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDevice([FromBody] Device device)
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            dynamic result = await tokenService.ValidateTokensAsync(accessToken!, refreshToken!);

            if (!result.Success)
                return Unauthorized(new { Message = result.Message });

            string userId = result.UserId;

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            TokenResponse newTokens = await tokenService.GenerateTokensAsync(user!);

            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.Now.AddHours(3),
                Path = "/"
            };

            Response.Cookies.Append("auth_token", newTokens.AccessToken, cookieOptions);

            cookieOptions.Expires = DateTimeOffset.Now.AddDays(7);
            Response.Cookies.Append("refresh_token", newTokens.RefreshToken, cookieOptions);

            dynamic created = await deviceService.CreateDeviceAsync(device);

            if (created.Created)
                return Ok(new { Message = "Device created successfully.", Device = created.Device });

            return BadRequest(new { Message = "Device creation failed. Make sure you are giving the required parameters." });
        }

        [HttpGet("{serialNumber}")]
        public async Task<IActionResult> GetDevice(string serialNumber)
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            dynamic result = await tokenService.ValidateTokensAsync(accessToken!, refreshToken!);

            if (!result.Success)
                return Unauthorized(new { Message = result.Message });

            string userId = result.UserId;
            Device device = await deviceService.GetDeviceAsync(serialNumber);

            if (device == null)
                return NotFound(new { Message = "Device not found with given Serial number." });

            if (device.UserId != userId)
                return Unauthorized(new { Message = "You are not authorized to view this device." });

            return Ok(device);
        }

        [HttpPut("{serialNumber}")]
        public async Task<IActionResult> UpdateDevice(string serialNumber, string name, string measurementType, string unit, double latestReading)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(measurementType) || string.IsNullOrWhiteSpace(unit))
                return BadRequest(new { Message = "Name, MeasurementType and Unit are required fields. Make sure you include them in your request." });

            Device existingDevice = await deviceService.GetDeviceAsync(serialNumber);

            if (existingDevice == null)
                return NotFound(new { Message = "Device not found with given Serial number." });

            existingDevice.IsOnline = true;
            existingDevice.LastConnectionTime = DateTimeOffset.Now;

            Sensor? existingSensor = await deviceService.GetExistingSensorAsync(name, serialNumber);

            if (existingSensor == null)
            {
                existingSensor = new() {
                    Id = Guid.CreateVersion7().ToString(),
                    IsOnline = true,
                    LastConnectionTime = DateTimeOffset.Now,
                    MeasurementType = measurementType,
                    Unit = unit,
                    Name = name,
                    LatestReading = latestReading,
                    DeviceSerialNumber = serialNumber,
                };
            }
            else
            {
                existingSensor.IsOnline = true;
                existingSensor.LastConnectionTime = DateTimeOffset.Now;
                existingSensor.LatestReading = latestReading;
            }

            var deviceUpdated = await deviceService.UpdateDeviceAsync(existingDevice);
            var sensorUpdated = await deviceService.UpdateDeviceSensorAsync(existingDevice, existingSensor!);

            return deviceUpdated && sensorUpdated
                ? Ok(new { Message = "Device and its sensors updated successfully." })
                : BadRequest(new { Message = "Something went wrong updating the device and its sensor." +
                    "Make sure the device serial number is correct and sensor name is unique for the device." });
        }
    }
}
