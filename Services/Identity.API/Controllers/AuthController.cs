using Common.DTOs;
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
        public async Task<ActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            // Kullanıcı zaten var mı kontrol 
            var exists = await _context.AppUsers.AnyAsync(u => u.UserName == registerDto.UserName.ToLower());
            if (exists) return BadRequest("Bu kullanıcı adı zaten alınmış!");

            //Yeni kullanıcıyı 
            var newUser = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                Email = registerDto.Email,
                Password = registerDto.Password 
            };

            //Veritabanına kaydet
            _context.AppUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kayıt başarılı!" });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName.ToLower() && u.Password == loginDto.Password);
            if(user == null) return Unauthorized(new { message = "Invalid username or password" });

            var tokenService = new TokenService(_configuration);
            var token = tokenService.CreateToken(user);

            return Ok(new { Token = token, message = "Login successful" });
        }
    }
}
