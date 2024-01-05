using Enlap_Voting_Backend.Implementations;
using Enlap_Voting_Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Enlap_Voting_Backend.Services
{
	public interface IAuthRepository
	{
		Task AssignPermissionToRoleAsync(string roleName, string permissionName);
		Task<bool> CheckPasswordAsync(AppUser user, string password);
		Task<IdentityResult> CreatePermissionAsync(string name);
		Task<UserOTP?> FindMatchingValidOTP(string otpFromUser);
		string GenerateJwtToken(AppUser user);
		Task<IEnumerable<AppUserPermission>> GetAllPermissionsAsync();
		Task<IEnumerable<AppUserPermission>> GetPermissionsByRoleNameAsync(string roleName);
		Task<AppUser> GetUserByEmailAsync(string email);
		Task<IdentityResult> RemovePermissionFromRoleAsync(string roleId, string permissionId);
		string SendOTPByEmail(string email);
		Task<bool> UpdatePasswordAsync(string email, string oldPassword, string newPassword);
	}
}
