using Enlap_Voting_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Enlap_Voting_Backend.Dtos;
using Enlap_Voting_Backend.Services;
using Microsoft.AspNetCore.Identity;
using Enlap_Voting_Backend.Enums;

namespace Enlap_Voting_Backend.Controllers
{		
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{

		private readonly IAdminSignupService _adminSignupService;
		private readonly IVoterSignupService _voterSignupService;
		private readonly IContestantSignupService _contestantSignupService;
		private readonly IAuthService _authService;
		private readonly UserManager<AppUser> _userManager;

		public AuthController(
			UserManager<AppUser> userManager,
			IAuthService authService,
			IAdminSignupService adminSignupService,
			IVoterSignupService voterSignupService,
			IContestantSignupService contestantSignupService)
		{
			_userManager = userManager;
			_adminSignupService = adminSignupService;
			_voterSignupService = voterSignupService;
			_contestantSignupService = contestantSignupService;
			_authService = authService;
		}


		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto login)
		{
			try
			{
				var token = await _authService.AuthenticateAsync(login.Email, login.Password);

				if (token == null)
					return Unauthorized(ApiResponse.Failed("Invalid credentials"));

				// Get the user based on the email
				var user = await _userManager.FindByEmailAsync(login.Email);

				// Check if the user is found
				if (user == null)
					return Unauthorized(ApiResponse.Failed("User not found"));

				// Create a response object with the required user information
				var response = new
				{
					UserId = user.Id,
					FirstName = user.FirstName,
					LastName = user.LastName,
					UserRole = user.UserRole, // Replace with the actual property name for the user role
					Token = token
				};

				// Return the response with the additional user information
				return Ok(ApiResponse.Success(response));
			}
			catch (Exception ex)
			{
				// Log the exception
				return StatusCode(500, ApiResponse.Failed("Internal server error"));
			}
		}

		[HttpPost("signup/Admin")]
		public async Task<IActionResult> CreateAdmin([FromBody] AdminSignupDto adminSignupDto)
		{
			var res = await _adminSignupService.SignUpAsync(adminSignupDto);
			if (res.Succeeded) return Ok(res);

			return BadRequest(res);
		}

		[HttpPost("signup/contestant")]
		public async Task<IActionResult> CreateUser([FromBody] ContestantSignupDto contestantSignupDto)
		{
			var res = await _contestantSignupService.SignUpAsync(contestantSignupDto);
			if (res.Succeeded) return Ok(res);

			return BadRequest(res);
		}


		[HttpPost("signup/voter")]
		public async Task<IActionResult> CreateVoter([FromBody] VoterSignupDto voterSignupDto)
		{
			var res = await _voterSignupService.SignUpAsync(voterSignupDto);
			if (res.Succeeded) return Ok(res);

			return BadRequest(res);
		}


	}



}
