using Enlap_Voting_Backend.Dtos;
using Enlap_Voting_Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Enlap_Voting_Backend.Services
{
	public interface IAdminSignupService
	{
		Task<ApiResponse> SignUpAsync(AdminSignupDto adminSignupDto);

	}
}
