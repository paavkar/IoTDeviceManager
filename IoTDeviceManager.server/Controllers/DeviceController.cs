using Asp.Versioning;
using IoTDeviceManager.server.CosmosDB;
using IoTDeviceManager.server.Models.Auth;
using IoTDeviceManager.server.Models.Devices;
using IoTDeviceManager.server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IoTDeviceManager.server.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    public class DeviceController(
        TokenService tokenService,
        UserManager<ApplicationUser>userManager,
        IDeviceService deviceService,
        ICosmosDbService cosmosService) : ControllerBase
    {
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetDevicesV1()
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];
            dynamic result = await tokenService.ValidateTokensAsync(accessToken!, refreshToken!);

            if (!result.Success)
                return Unauthorized(new { Message = result.Message });

            string userId = result.UserId;
            IEnumerable<Device> devices = await deviceService.GetDevicesAsync(userId);

            return Ok(new { Devices = devices });
        }

        [HttpGet, MapToApiVersion("2.0")]
        public async Task<IActionResult> GetDevicesV2()
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];
            dynamic result = await tokenService.ValidateTokensAsync(accessToken!, refreshToken!);

            if (!result.Success)
                return Unauthorized(new { Message = result.Message });

            string userId = result.UserId;
            List<CDevice> devicesList = await cosmosService.GetDevicesAsync(userId);

            return Ok(new { Devices = devicesList });
        }

        [HttpPost("create"), MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateDeviceV1([FromBody] Device device)
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

        [HttpPost("create"), MapToApiVersion("2.0")]
        public async Task<IActionResult> CreateDeviceV2([FromBody] CDevice device)
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

            dynamic created = await cosmosService.CreateDeviceAsync(device);

            if (created.Created)
                return Ok(new { Message = "Device created successfully.", Device = created.Device });

            return BadRequest(new { Message = "Device creation failed. Make sure you are giving the required parameters." });
        }

        [HttpGet("{serialNumber}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetDeviceV1(string serialNumber)
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

            return device.UserId != userId
                ? Unauthorized(new { Message = "You are not authorized to view this device." })
                : Ok(device);
        }

        [HttpGet("{serialNumber}"), MapToApiVersion("2.0")]
        public async Task<IActionResult> GetDeviceV2(string serialNumber)
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            dynamic result = await tokenService.ValidateTokensAsync(accessToken!, refreshToken!);

            if (!result.Success)
                return Unauthorized(new { Message = result.Message });

            string userId = result.UserId;
            CDevice? device = await cosmosService.GetDeviceAsync(serialNumber);

            if (device == null)
                return NotFound(new { Message = "Device not found with given Serial number." });

            return device.UserId != userId
                ? Unauthorized(new { Message = "You are not authorized to view this device." })
                : Ok(device);
        }

        [HttpPut("{serialNumber}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateDeviceV1(string serialNumber, [FromBody]DeviceSensorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.MeasurementType) || string.IsNullOrWhiteSpace(request.Unit))
                return BadRequest(new { Message = "Name, MeasurementType and Unit are required fields. Make sure you include them in your request." });

            Device existingDevice = await deviceService.GetDeviceAsync(serialNumber);

            if (existingDevice == null)
                return NotFound(new { Message = "Device not found with given Serial number." });

            existingDevice.IsOnline = true;
            existingDevice.LastConnectionTime = DateTimeOffset.Now;

            Sensor? existingSensor = await deviceService.GetExistingSensorAsync(request.Name, serialNumber);

            if (existingSensor == null)
            {
                existingSensor = new() {
                    Id = Guid.CreateVersion7().ToString(),
                    IsOnline = true,
                    LastConnectionTime = DateTimeOffset.Now,
                    MeasurementType = request.MeasurementType,
                    Unit = request.Unit,
                    Name = request.Name,
                    LatestReading = request.LatestReading,
                    DeviceSerialNumber = serialNumber,
                };
            }
            else
            {
                existingSensor.IsOnline = true;
                existingSensor.LastConnectionTime = DateTimeOffset.Now;
                existingSensor.LatestReading = request.LatestReading;
            }

            var deviceUpdated = await deviceService.UpdateDeviceAsync(existingDevice);
            var sensorUpdated = await deviceService.UpdateDeviceSensorAsync(existingDevice, existingSensor!);

            return deviceUpdated && sensorUpdated
                ? Ok(new { Message = "Device and its sensors updated successfully." })
                : BadRequest(new { Message = "Something went wrong updating the device and its sensor." +
                    "Make sure the device serial number is correct and sensor name is unique for the device." });
        }

        [HttpPut("{serialNumber}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateDeviceV2(string serialNumber, [FromBody] DeviceSensorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.MeasurementType) || string.IsNullOrWhiteSpace(request.Unit))
                return BadRequest(new { Message = "Name, MeasurementType and Unit are required fields. Make sure you include them in your request." });

            CDevice? existingDevice = await cosmosService.GetDeviceAsync(serialNumber);

            if (existingDevice == null)
                return NotFound(new { Message = "Device not found with given Serial number." });

            existingDevice.IsOnline = true;
            existingDevice.LastConnectionTime = DateTimeOffset.Now;

            CSensor? existingSensor = existingDevice.Sensors
                .FirstOrDefault(s => s.Name == request.Name);

            if (existingSensor == null)
            {
                existingSensor = new()
                {
                    IsOnline = true,
                    LastConnectionTime = DateTimeOffset.Now,
                    Name = request.Name,
                    LatestReadings = [new CSensorReading { MeasurementType = request.MeasurementType, Unit = request.Unit, Reading = request.LatestReading }],
                };
                existingDevice.Sensors.Add(existingSensor);
            }
            else
            {
                existingSensor.IsOnline = true;
                existingSensor.LastConnectionTime = DateTimeOffset.Now;
                CSensorReading? existingReading = existingSensor.LatestReadings
                    .FirstOrDefault(r => r.MeasurementType == request.MeasurementType);

                if (existingReading is null)
                {
                    existingReading = new()
                    {
                        MeasurementType = request.MeasurementType,
                        Unit = request.Unit,
                        Reading = request.LatestReading
                    };
                    existingSensor.LatestReadings.Add(existingReading);
                }
                else
                {
                    existingReading.Reading = request.LatestReading;
                    existingReading.Unit = request.Unit;
                }
            }

            var deviceUpdated = await cosmosService.UpdateDeviceAsync(existingDevice);

            return deviceUpdated
                ? Ok(new { Message = "Device and its sensors updated successfully." })
                : BadRequest(new
                {
                    Message = "Something went wrong updating the device and its sensor." +
                    "Make sure the device serial number is correct and sensor name is unique for the device."
                });
        }
    }
}
