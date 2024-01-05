using Enlap_Voting_Backend.Dtos;
using Enlap_Voting_Backend.Enums;
using Enlap_Voting_Backend.Models;
using Enlap_Voting_Backend.Services;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Enlap_Voting_Backend.Implementations
{
	public class VoterSignupService : IVoterSignupService
	{
		private readonly UserManager<AppUser> _userManager;

		public VoterSignupService(UserManager<AppUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<ApiResponse> SignUpAsync(VoterSignupDto voterSignupDto)
		{
			// Check if the email is already associated with an existing user
			var existingUser = await _userManager.FindByEmailAsync(voterSignupDto.Email);
			if (existingUser != null)
			{
				return ApiResponse.Failed("User already exists");
			}

			// Generate a unique username based on the user's first and last names
			string generatedUsername = await GenerateUniqueUsernameAsync(
				voterSignupDto.Firstname, voterSignupDto.Lastname);

			// Create a new user with the generated username
			var user = new AppUser()
			{
				FirstName = voterSignupDto.Firstname,
				LastName = voterSignupDto.Lastname,
				Email = voterSignupDto.Email.Trim(),
				UserName = generatedUsername.Trim(),
				Password = voterSignupDto.Password.Trim(),
				EmailConfirmed = false,
				UserRole = UserRoles.Voter,
				PhoneNumber = voterSignupDto.PhoneNumber.Trim()
			};

			using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
			{
				// Check if the generated username is already in use
				bool isUsernameInUse = await IsUsernameInUseAsync(generatedUsername);
				if (isUsernameInUse)
				{
					return ApiResponse.Failed("Generated username is already in use");
				}

				// Attempt to create the user
				var createUser = await _userManager.CreateAsync(user, voterSignupDto.Password);
				if (!createUser.Succeeded)
				{
					// Handle the case where user creation failed
					transaction.Complete();
					return ApiResponse.Failed(createUser.Errors);
				}

				// If successful, complete the transaction
				transaction.Complete();
				return ApiResponse.Success("User added successfully");
			}
		}


		public async Task<bool> IsUsernameInUseAsync(string username)
		{
			var existingUser = await _userManager.FindByNameAsync(username);
			return existingUser != null;
		}


		public async Task<string> GenerateUniqueUsernameAsync(string firstName, string lastName)
		{
			string baseUsername = $"{firstName.ToLower()}_{lastName.ToLower()}";
			string uniqueUsername = baseUsername;

			int suffix = 1;
			while (await IsUsernameInUseAsync(uniqueUsername))
			{
				uniqueUsername = $"{baseUsername}_{suffix}";
				suffix++;
			}

			return uniqueUsername;
		}
	}

}

