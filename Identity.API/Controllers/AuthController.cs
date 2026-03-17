using Identity.API.Data;
using Identity.API.Entities;
using Identity.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IdentityContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(IdentityContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] AppUser user)
        {
            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);
            if(user== null) return Unauthorized(new { message = "Invalid username or password" });

            var tokenService = new TokenService(_configuration);
            var token = tokenService.CreateToken(user);

            return Ok(new { message = "Login successful" });
        }
    }
}
