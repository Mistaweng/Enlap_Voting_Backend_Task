using Enlap_Voting_Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Enlap_Voting_Backend.Helpers
{
	public static class RoleSeeder
	{
		
		public static async Task SeedRolesAsync(RoleManager<AppUserRole> roleManager)
		{
			//if (!await roleManager.RoleExistsAsync(Roles.Admin))
			//{
			//	await roleManager.CreateAsync(new AppUserRole { Name = Roles.Admin });
			//}

			//if (!await roleManager.RoleExistsAsync(Roles.Voter))
			//{
			//	await roleManager.CreateAsync(new AppUserRole { Name = Roles.Voter });
			//}

			//if (!await roleManager.RoleExistsAsync(Roles.Contestant))
			//{
			//	await roleManager.CreateAsync(new AppUserRole { Name = Roles.Contestant });
			//}
		}
	}
}
