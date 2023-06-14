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
	public class AccountController : Controller
	{
		private readonly IAccountService _accountService;
		public AccountController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		[AllowAnonymous]
		[HttpPost("account/login")]
		public async Task<IActionResult> Login(LoginModel model)
		{
			try
			{
				var result = await _accountService.Login(model.EmployeeCode, model.Password);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpGet("account/get-empploy")]
		public async Task<IActionResult> GetEmpploy()
		{
			try
			{
				var result = 1;
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[HttpGet("account/get-testparams")]
		public async Task<IActionResult> GetEmpploy(int mscn)
		{
			try
			{
				
				var result = 1;
				if (mscn == 1) result = 2;
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}
	}
}
