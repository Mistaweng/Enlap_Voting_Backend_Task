using Enlap_Voting_Backend.Context;
using Enlap_Voting_Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Enlap_Voting_Backend.Services;
using Enlap_Voting_Backend.Helpers;
using RandomNumberGenerator = Enlap_Voting_Backend.Helpers.RandomNumberGenerator;
using Microsoft.EntityFrameworkCore;

namespace Enlap_Voting_Backend.Implementations
{

	public class AuthRepository : IAuthRepository
	{
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _db;
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<AppUserRole> _roleManager;
		private readonly SignInManager<AppUser> _signInManager;

		public AuthRepository(IConfiguration configuration, RoleManager<AppUserRole> roleManager, ApplicationDbContext Db, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
		{
			_configuration = configuration;
			_userManager = userManager;
			_roleManager = roleManager;
			_db = Db;
			_signInManager = signInManager;
		}


		public async Task<AppUser> GetUserByEmailAsync(string email)
		{
			return await _userManager.FindByEmailAsync(email);
		}

		public async Task<bool> CheckPasswordAsync(AppUser user, string password)
		{
			return await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false) == SignInResult.Success;
		}


		public async Task<bool> UpdatePasswordAsync(string email, string oldPassword, string newPassword)
		{
			var user = await _userManager.FindByEmailAsync(email);

			if (user != null)
			{
				var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

				if (result.Succeeded)
				{
					return true;
				}
			}
			return false;
		}

		public string GenerateJwtToken(AppUser user)
		{
			var jwtSettings = _configuration.GetSection("JwtSettings");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			// Set a default expiration in minutes if "AccessTokenExpiration" is missing or not a valid numeric value.
			if (!double.TryParse(jwtSettings["AccessTokenExpiration"], out double accessTokenExpirationMinutes))
			{
				accessTokenExpirationMinutes = 30; // Default expiration of 30 minutes.
			}

			var token = new JwtSecurityToken(
				issuer: null,
				audience: null,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}


		public string SendOTPByEmail(string email)
		{
			try
			{
				//generating otp
				var generateRan = new RandomNumberGenerator();
				var otp = generateRan.GenerateOTP().ToString();


				using (MailMessage mailMessage = new MailMessage())
				{
					mailMessage.From = new MailAddress(EmailSettings.SmtpUsername);
					mailMessage.To.Add(email);
					mailMessage.Subject = "One Time Password(OTP)";
					mailMessage.Body = $"Your OTP:{otp}";

					using (SmtpClient smtpClient = new SmtpClient(EmailSettings.SmtpServer, EmailSettings.SmtpPort))
					{
						smtpClient.EnableSsl = true;
						smtpClient.UseDefaultCredentials = false;
						smtpClient.Credentials = new NetworkCredential(EmailSettings.SmtpUsername, EmailSettings.SmtpPassword);
						smtpClient.Send(mailMessage);
					}
				}

				// saving otp
				var userOTP = new UserOTP
				{
					Email = email,
					OTP = otp,
					Expiration = DateTime.UtcNow.AddMinutes(10) // set an expiration time for OTP (e.g 5 minutes)
				};
				//_db.UserOTPs.Add(userOTP);
				_db.SaveChanges();

				return $"OTP sent to {email}";
			}
			catch (Exception ex)
			{
				return $"Faild To Send OTP to {email}, Error, {ex.Message}";
			}
		}


		public async Task<UserOTP?> FindMatchingValidOTP(string otpFromUser)
		{
			var currentTime = DateTime.UtcNow;
			var expirationTime = TimeSpan.FromMinutes(5);

			var result = await _db.userOTPs.FirstOrDefaultAsync(otp => otp.OTP == otpFromUser && (currentTime - otp.Expiration) <= expirationTime);

			return result;
		}


		public async Task<IdentityResult> CreatePermissionAsync(string name)
		{
			var permission = new AppUserPermission { Name = name };
			_db.Permissions.Add(permission);

			try
			{
				await _db.SaveChangesAsync();
				return IdentityResult.Success;
			}
			catch (Exception ex)
			{
				// Handle any exceptions that may occur while saving to the database
				return IdentityResult.Failed(new IdentityError { Description = $"Failed to create permission: {ex.Message}" });
			}
		}


		//public async Task<ApiResponse<string>> UpdateVerifiedStatus(string email)
		//{
		//	var verifiedUser = await _userManager.FindByEmailAsync(email);

		//	if (verifiedUser == null)
		//	{
		//		return new ApiResponse<string>($"{email} Not Found!");
		//	}
		//	verifiedUser .IsVerified = true;

		//	// Save changes to the database
		//	var identityResult = await _userManager.UpdateAsync(verifiedUser);
		//	if (!identityResult.Succeeded)
		//	{
		//		return new ApiResponse<string>("Verification Failed.");
		//	}
		//	return new ApiResponse<string>("Verified successfully.");
		//}


		public async Task<IEnumerable<AppUserPermission>> GetAllPermissionsAsync()
		{
			return await _db.Permissions.ToListAsync();
		}



		public async Task<IEnumerable<AppUserPermission>> GetPermissionsByRoleNameAsync(string roleName)
		{
			var role = await _roleManager.FindByNameAsync(roleName);
			if (role == null)
			{
				return Enumerable.Empty<AppUserPermission>();
			}
			else
			{
				// Get the claims associated with the role
				var claims = await _roleManager.GetClaimsAsync(role);
				var permissionNames = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

				// Find the permissions with matching names
				var permissions = _db.Permissions.Where(p => permissionNames.Contains(p.Name));
				return permissions;
			}
		}




		public async Task AssignPermissionToRoleAsync(string roleName, string permissionName)
		{
			var role = await _roleManager.FindByNameAsync(roleName);
			if (role == null)
			{
				throw new InvalidOperationException("Role not found.");
			}
			else
			{
				var permission = await _db.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
				if (permission == null)
				{
					throw new InvalidOperationException("Permission not found.");
				}
				else
				{
					// Add the new IdentityRoleClaim
					var claim = new Claim("Permission", permission.Name);
					var result = await _roleManager.AddClaimAsync(role, claim);

					if (!result.Succeeded)
					{
						throw new InvalidOperationException("Failed to add permission claim to role.");
					}
				}
			}

		}





		public async Task<IdentityResult> RemovePermissionFromRoleAsync(string roleId, string permissionId)
		{
			var role = await _roleManager.FindByIdAsync(roleId);
			if (role == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Role not found." });
			}

			var permission = await _db.Permissions.FindAsync(permissionId);
			if (permission == null)
			{
				return IdentityResult.Failed(new IdentityError { Description = "Permission not found." });
			}

			// Get the claim associated with the permission and the role
			var claim = (await _roleManager.GetClaimsAsync(role)).FirstOrDefault(c => c.Type == "Permission" && c.Value == permission.Name);
			if (claim != null)
			{
				// Remove the claim from the role
				var result = await _roleManager.RemoveClaimAsync(role, claim);
				if (result.Succeeded)
				{
					return IdentityResult.Success;
				}
				else
				{
					// Handle the case where removing the claim fails
					return IdentityResult.Failed(new IdentityError { Description = "Failed to remove permission from role." });
				}
			}
			return IdentityResult.Failed(new IdentityError { Description = "Permission not assigned to role." });
		}


		//public async Task<string> IsEmailVerifiedAsync(string userId)
		//{
		//	var user = await _userManager.FindByIdAsync(userId);
		//	var isUserVerified = _db.Users.Any(u => u.IsVerified == true);

		//	string message = (user == null)
		//		? "User does not exist"
		//		: (isUserVerified)
		//			? "User is verified"
		//			: "The user has not been verified!";
		//	return message;
		//}



	}
}
