using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace boat_share.Controllers
{
	[AllowAnonymous]
	[Route("api/[controller]")]
	[ApiController]
	public class EchoController : ControllerBase
	{
		[HttpGet]
		public IActionResult Index()
		{
			return Ok(DateTime.UtcNow);
		}
	}
}