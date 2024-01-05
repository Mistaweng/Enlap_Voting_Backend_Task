using Enlap_Voting_Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Enlap_Voting_Backend.Services;

namespace Enlap_Voting_Backend.Implementations
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IConfiguration _configuration;

		public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
		{
			_userManager = userManager;
			_configuration = configuration;
		}

		public async Task<string> AuthenticateAsync(string email, string password)
		{
			var user = await _userManager.FindByEmailAsync(email);

			if (user == null || !await _userManager.CheckPasswordAsync(user, password))
			{
				throw new UnauthorizedAccessException("Invalid credentials");
			}

			// Generate JWT token
			var token = GenerateJwtToken(user);

			return token;
		}

		public string GenerateJwtToken(AppUser user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Role, user.UserRole.ToString())
                // Add other claims as needed
            };

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _configuration["JwtSettings:Issuer"],
				audience: _configuration["JwtSettings:Audience"],
				claims: claims,
				expires: DateTime.Now.AddHours(1), // Token expiration time
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
