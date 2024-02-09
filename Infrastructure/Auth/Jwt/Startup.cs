using Application.Common.Exceptions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using System.Text;

namespace Infrastructure.Auth.Jwt
{
    internal static class Startup
    {
        internal static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            var jwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>();
            if (jwtSettings == null)
                throw new InvalidOperationException("No Key defined in JwtSettings config.");
            if (string.IsNullOrEmpty(jwtSettings.Key))
                throw new InvalidOperationException("No Key defined in JwtSettings config.");

            return services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(bearer =>
                {
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidIssuer = "https://localhost:7112/",
                        ValidAudience = "https://localhost:7112/"
                    };
                    
                })
                .Services;
        }
    }
}
