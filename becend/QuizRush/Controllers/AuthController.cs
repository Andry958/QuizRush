using BusinessLogic.DTOs;
using BusinessLogic.Services;
using DataAccess.Data;
using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QuizRush.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TokenService _tokenService;
        private readonly QuizRushContext _context;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            TokenService tokenService,
            QuizRushContext context,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _context = context;
            _config = config;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.Nickname,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User created");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var result = await _signInManager
                .CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Invalid credentials");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(
                    double.Parse(_config["JwtSettings:RefreshTokenDays"]))
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken,
                refreshToken,
                nickname = user.UserName
            });
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenRequest request)
        {
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u =>
                    u.RefreshTokens.Any(t => t.Token == request.RefreshToken));

            if (user == null)
                return Unauthorized();

            var oldToken = user.RefreshTokens
                .First(t => t.Token == request.RefreshToken);

            if (oldToken.IsRevoked || oldToken.Expires <= DateTime.UtcNow)
                return Unauthorized();

            // ROTATION
            oldToken.IsRevoked = true;

            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                Expires = DateTime.UtcNow.AddDays(
                    double.Parse(_config["JwtSettings:RefreshTokenDays"]))
            });

            var newAccessToken = _tokenService.GenerateAccessToken(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken,
                nickname = user.UserName
            });
        }
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(TokenRequest request)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

            if (token == null)
                return NotFound();

            token.IsRevoked = true;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}


