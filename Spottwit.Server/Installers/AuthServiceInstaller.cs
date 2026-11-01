using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MuslinkReact.Server.Models.Entities;
using MuslinkReact.Server.Services;

namespace Spottwit.Server.Installers
{
    public class AuthServiceInstaller : IServiceInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            var spotifySettings = configuration.GetSection("Authentication:Spotify");
            if (!string.IsNullOrEmpty(spotifySettings["ClientId"]))
            {
                services.AddAuthentication()
                    .AddSpotify(options =>
                    {
                        options.ClientId = spotifySettings["ClientId"]!;
                        options.ClientSecret = spotifySettings["ClientSecret"]!;
                        options.CallbackPath = "/signin-spotify";
                        options.Scope.Add("user-read-email");
                        options.Scope.Add("user-read-private");
                    });
            }

            services.AddAuthorization();
        }
    }
}