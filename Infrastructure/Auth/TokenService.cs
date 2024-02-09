using Application.Auth.Tokens;
using Application.Auth.Users;
using Application.Common.Exceptions;
using Application.Core.Organizations;

using Infrastructure.Auth.Jwt;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultiTenant.Infrastructure.Auth
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        public TokenService(IOptions<JwtSettings> jwtSettings, IUserService userService, IOrganizationService organizationService)
        {
            _jwtSettings = jwtSettings.Value;
            _userService = userService;
            _organizationService = organizationService;
        }

        public async Task<TokenResponse> GetTokenAsync(TokenRequest request)
        {
            var userDetailsDto = await _userService.GetByEmailAsync(request.Email);
            if (userDetailsDto == null)
            {
                throw new UnauthorizedException("User or password incorrect");
            }
            if (!await _userService.CheckPasswordAsync(request.Email, request.Password))
            {
                throw new UnauthorizedException("User or password incorrect");
            }
            var organizationFromRepo = await _organizationService.GetByIdAsync(userDetailsDto.OrganizationId);
            if (organizationFromRepo == null)
            {
                throw new UnauthorizedException("User is not assigned to any organization");
            }

            return await GenerateToken(userDetailsDto, organizationFromRepo.Name);
        }

        private async Task<TokenResponse> GenerateToken(UserDetailsDto user, string organizationName)
        {
            var token = new JwtSecurityToken(
                issuer: "https://localhost:7112/",
                audience: "https://localhost:7112/",
                claims: GetClaims(user, organizationName),
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: GetSigningCredentials());

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenResult = tokenHandler.WriteToken(token);
            return new TokenResponse(tokenResult, organizationName);
        }

        private SigningCredentials GetSigningCredentials()
        {
            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                throw new InvalidOperationException("No Key defined in JwtSettings config.");
            }

            byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }

        private IEnumerable<Claim> GetClaims(UserDetailsDto user, string organizationName) =>
        new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new("OrganizationName", organizationName)
        };
    }
}
