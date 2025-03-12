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
        [HttpPost("create")]
        public async Task<IActionResult> CreateDevice([FromBody] Device device)
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            dynamic result = await tokenService.ValidateTokensAsync(accessToken!, refreshToken!);

            if (!result.Success)
                return Unauthorized(new { Message = result.Message });

            dynamic userId = result.UserId;

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
    }
}
