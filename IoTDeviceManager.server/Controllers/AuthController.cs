using IoTDeviceManager.server.Models.Auth;
using IoTDeviceManager.server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IoTDeviceManager.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        TokenService tokenService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            ApplicationUser user = new()
            {
                UserName = model.Username,
                Email = model.Email
            };

            IdentityResult result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleExists = await roleManager.RoleExistsAsync("User");
                if (!roleExists) await roleManager.CreateAsync(new IdentityRole("User"));
                await userManager.AddToRoleAsync(user, "User");

                return Ok(new { Username = user.UserName, user.Email });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            ApplicationUser? user;

            if (!string.IsNullOrEmpty(model.EmailOrUsername))
            {
                user = model.EmailOrUsername.Contains('@')
                    ? await userManager.FindByEmailAsync(model.EmailOrUsername)
                    : await userManager.FindByNameAsync(model.EmailOrUsername);
            }
            else
                return BadRequest(new { Message = "Username or email is required." });

            if (user is null)
                return BadRequest(new { Message = "There was an error with sign-in." });

            Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
            {
                if (user == null)
                    return BadRequest(new { Message = "Invalid login attempt" });

                TokenResponse token = await tokenService.GenerateTokensAsync(user);

                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddHours(3),
                    Path = "/"
                };

                Response.Cookies.Append("auth_token", token.AccessToken, cookieOptions);

                cookieOptions.Expires = DateTimeOffset.Now.AddDays(7);
                Response.Cookies.Append("refresh_token", token.RefreshToken, cookieOptions);

                return Ok(new { Message = "Login was successful", User = user });
            }

            return BadRequest(new { Message = "Invalid login attempt" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { Message = "Missing tokens." });

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { Message = "Missing refresh token." });

            RefreshToken? rf = await tokenService.GetRefreshTokenAsync(refreshToken);

            if (rf is null)
                return Unauthorized(new { Message = "Invalid refresh token." });

            if (rf.Revoked)
                return Unauthorized(new { Message = "Refresh token has been revoked." });

            if (rf.Expires < DateTimeOffset.Now)
                return Unauthorized(new { Message = "Refresh token has expired." });

            var userId = rf.UserId;

            if (!string.IsNullOrEmpty(accessToken))
            {
                ClaimsPrincipal principal = tokenService.GetPrincipalFromExpiredToken(accessToken);
                if (principal is null)
                    return Unauthorized(new { Message = "Invalid access token." });

                userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            }

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Invalid access token." });

            var isRefreshValid = await tokenService.ValidateRefreshTokenAsync(userId, refreshToken);
            if (!isRefreshValid)
                return Unauthorized(new { Message = "Invalid refresh token." });

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

            return Ok(newTokens);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var accessToken = Request.Cookies["auth_token"];
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(refreshToken))
            {
                Response.Cookies.Delete("auth_token");
                Response.Cookies.Delete("refresh_token");
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                ClaimsPrincipal principal = tokenService.GetPrincipalFromExpiredToken(accessToken);
                if (principal is null)
                    return Unauthorized("Invalid access token.");

                var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Invalid access token.");
                //await tokenService.RevokeRefreshTokenAsync(userId, refreshToken);
            }
            Response.Cookies.Delete("auth_token");
            Response.Cookies.Delete("refresh_token");

            return Ok("Logged out successfully.");
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var accessToken = Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized(new { Message = "Missing access token." });

            ClaimsPrincipal principal = tokenService.GetPrincipalFromExpiredToken(accessToken);

            if (principal is null)
                return Unauthorized(new { Message = "Invalid access token." });

            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Invalid access token." });

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            return user is null ? NotFound(new { Message = "No user found." }) : Ok(user);
        }
    }
}
