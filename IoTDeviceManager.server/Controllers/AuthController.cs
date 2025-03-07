using IoTDeviceManager.server.Models;
using IoTDeviceManager.server.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IoTDeviceManager.server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                bool roleExists = await roleManager.RoleExistsAsync("User");
                if (!roleExists) await roleManager.CreateAsync(new IdentityRole("User"));
                await userManager.AddToRoleAsync(user, "User");

                return Ok(new { Username = user.UserName, Email = user.Email });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            if (result.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return BadRequest("Invalid login attempt");

                var userRoles = (await userManager.GetRolesAsync(user)).ToList();
                var token = GenerateJwtToken(user, userRoles!);
                return Ok(new { Token = token });
            }
            return BadRequest("Invalid login attempt");
        }

        private string GenerateJwtToken(ApplicationUser user, List<string> userRoles)
        {
            var claims = new List<Claim>
            {
                new("id", user.Id),
                new("username", user.UserName!),
                new("email", user.Email!),
            };

            int i = 0;
            foreach (var role in userRoles)
            {
                claims.Add(new($"role{i}", role));
                i++;
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"]));

            var token = new JwtSecurityToken(
                configuration["Jwt:Issuer"],
                configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
