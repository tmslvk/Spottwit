using AspNet.Security.OAuth.Spotify;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuslinkReact.Server.Models.DTOs;
using MuslinkReact.Server.Models.Entities;
using MuslinkReact.Server.Services;
using Spottwit.Server.Data;
using System.Security.Claims;

namespace MuslinkReact.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, IJwtService jwtService, IPasswordHasher<User> passwordHasher, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
        {
            var emailExists = await _context.Set<User>()
                .AnyAsync(u => u.Email == dto.Email, cancellationToken);

            if (emailExists)
                return BadRequest(new { message = "User with this email is already exists." });

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _context.Set<User>().AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New user has registered: {Email}", user.Email);

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

            if (user == null)
            {
                return Unauthorized(new { message = "Incorrect email or password." });
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Failed attempt of login for email: {Email}", dto.Email);
                return Unauthorized(new { message = "Incorrect email or password." });
            }

            var token = _jwtService.GenerateToken(user);
            _logger.LogInformation("User entered the system: {Email}", user.Email);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email
            });
        }

        [HttpGet("signin-spotify")]
        [AllowAnonymous]
        public async Task<IActionResult> SpotifyCallback(CancellationToken cancellationToken)
        {
            var result = await HttpContext.AuthenticateAsync(SpotifyAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal == null)
                return BadRequest(new { message = "Authorization through Spotify failed." });

            var email = result.Principal.FindFirstValue(ClaimTypes.Email)
                        ?? $"{result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)}@spotify.local";
            var username = result.Principal.FindFirstValue(ClaimTypes.Name) ?? "SpotifyUser";

            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
            {
                user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = "SPOTIFY_EXTERNAL_AUTH"
                };

                await _context.Set<User>().AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("New user created through Spotify: {Email}", email);
            }

            var token = _jwtService.GenerateToken(user);
    
            return Ok(new AuthResponseDto
             {
                 Token = token,
                 Username = user.Username,
                 Email = user.Email
             });
            }
    }
}