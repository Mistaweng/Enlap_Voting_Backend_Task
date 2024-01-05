using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enlap_Voting_Backend.Controllers
{
	[Authorize(Roles = "Voter")]
	[Route("api/[controller]")]
	[ApiController]
	public class VoterController : ControllerBase
	{
	}




}
