using APITEST.Core.Model;
using APITEST.Core.Reponse;
using APITEST.Infrastructure.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APITEST.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("api")]
	[ApiController]
	public class HomeController : Controller
	{
		private readonly IHomeService _homeService;
		public HomeController(IHomeService homeService)
		{
			_homeService = homeService;
		}

		[HttpGet("home/get-location")]
		public async Task<ActionResult> GetLocation(string UName = "admin", int NNgu = 0, int CoAll = 1)
		{
			try
			{
				BaseResponse<IEnumerable< LocationModel >> result = await _homeService.GetLocation(UName, NNgu, CoAll);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpGet("home/get-machine")]
		public async Task<ActionResult> GetMachine(string UName = "admin", int NNgu = 0, int CoAll = 1)
		{
			try
			{
				BaseResponse<IEnumerable<MachineModel>> result = await _homeService.GetMachine(UName, NNgu, CoAll);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpGet("home/get-myecomaint")]
		public async Task<ActionResult> GetMyEcomain(string username, int languages, DateTime? dngay, string ms_nx, string mslmay, bool xuly, int pageIndex, int pageSize)
		{
			try
			{
				BaseResponse<IEnumerable<MyEcomaintViewModel>> result = await _homeService.GetMyEcomain(username, languages, dngay,ms_nx, mslmay, xuly, pageIndex, pageSize);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}
	}
}
