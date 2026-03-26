using Common.DTOs;
using Identity.API.Data;
using Identity.API.Entities;
using Identity.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Threading.Tasks;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            var exists = await _context.AppUsers.AnyAsync(u => u.UserName == registerDto.UserName.ToLower());
            if (exists) return BadRequest("Bu kullanıcı adı zaten alınmış!");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var newUser = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                Email = registerDto.Email,
                Password = hashedPassword
            };

            _context.AppUsers.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kayıt başarılı! Şifreniz güvenli bir şekilde saklandı." });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            var user = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == loginDto.UserName.ToLower());
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı!" });
            }

            var tokenService = new TokenService(_configuration);
            var token = tokenService.CreateToken(user);

            return Ok(new { Token = token, Role = user.Role, message = "Giriş başarılı" });
        }

        [HttpGet("profile/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetProfile(string username)
        {
            var user = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == username.ToLower());
            
            if (user == null) return NotFound();

            return Ok(new UserProfileDto
            {
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role
            });
        }

        [HttpPut("profile/{username}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(string username, [FromBody] UpdateProfileDto updateDto)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.UserName == username.ToLower());
            if (user == null) return NotFound();

            user.Email = updateDto.Email;

            if (!string.IsNullOrWhiteSpace(updateDto.NewPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(updateDto.NewPassword);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
