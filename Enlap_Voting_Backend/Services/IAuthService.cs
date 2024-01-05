using Enlap_Voting_Backend.Models;

namespace Enlap_Voting_Backend.Services
{
	public interface IAuthService
	{
		Task<string> AuthenticateAsync(string email, string password);
		string GenerateJwtToken(AppUser user);
	}
}
