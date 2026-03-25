using Microsoft.AspNetCore.Mvc;
using ProductInventoryAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
 
namespace ProductInventoryAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
           private List<User> users = new List<User>()
        {
            new User { Username = "admin", Password = "admin123",Role="Admin"},
            new User { Username = "manager", Password = "manager123",Role="Manager"},
            new User { Username = "viewer", Password = "viewer123",Role="Viewer"} 
        };
 
        [HttpPost("login")]
        public IActionResult Login([FromBody] User login)
        {
            var user = users.FirstOrDefault(u =>u.Username == login.Username &&u.Password == login.Password);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("ThisIsMyVeryStrongSecretKeyForJwtAuthentication12345");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
 
                Expires = DateTime.UtcNow.AddHours(1),
 
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
 
            var token = tokenHandler.CreateToken(tokenDescriptor);
 
            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}
 