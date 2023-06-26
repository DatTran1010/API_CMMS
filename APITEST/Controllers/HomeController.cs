using APITEST.Core.Model;
using APITEST.Core.Reponse;
using APITEST.Infrastructure.IServices;
using Firebase.Auth;
using Firebase.Storage;
using Google.Apis.Storage.v1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Policy;

namespace APITEST.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[Route("api")]
	[ApiController]
	public class HomeController : Controller
	{
		private readonly IHomeService _homeService;
		private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
		private static string apiKey = "AIzaSyDln2NkoGScX86HiXaLMhLdKM-9Mihm7VM";
		private static string Bucket = "uploadfile-754fe.appspot.com";
		private static string AuthEmail = "dattranlfc@gmail.com";
		private static string AuthPassword = "tandat";
		public HomeController(IHomeService homeService, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			_homeService = homeService;
			_env = env;
		}

		[HttpGet("home/get-location")]
		public async Task<ActionResult> GetLocation(string UName = "admin", int NNgu = 0, int CoAll = 1)
		{
			try
			{
				var result = await _homeService.GetLocation(UName, NNgu, CoAll);
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
		public async Task<ActionResult> GetMyEcomain(string username = "admin", int languages = 0, DateTime? dngay = null, string ms_nx = "-1", string mslmay = "-1", bool xuly = true, int pageIndex = 0, int pageSize = 0)
		{
			try
			{
				BaseResponse<IEnumerable<MyEcomaintViewModel>> result = await _homeService.GetMyEcomain(username, languages, dngay, ms_nx, mslmay, xuly, pageIndex, pageSize);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}
		//[AllowAnonymous]
		//[HttpPost("home/upload-file")]
		//public async Task<ActionResult> UpLoadFile()
		//{
		//	try
		//	{
		//		var fileupload = "01.HRM.rar";
		//		FileStream fs;
		//		string foldername = "firebaseFiles";
		//		string path = Path.Combine(_env.ContentRootPath, "E:\\Lamviec\\Lamviec");
		//		fs = new FileStream(Path.Combine(path, fileupload), FileMode.Open);

		//		var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
		//		var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

		//		var cancellation = new CancellationTokenSource();


		//		var task = new FirebaseStorage(
		//			Bucket,
		//			new FirebaseStorageOptions
		//			{
		//				AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
		//				ThrowOnCancel = true
		//			})
		//			.Child("E:\\Lamviec\\Lamviec\\01.HRM.rar")
		//			.PutAsync(fs, cancellation.Token);

		//		string link = await task;

		//		string targetFileName = @"01.HRM.rar";
		//		using (WebClient client = new WebClient())
		//		{
		//			Uri downloadURI = new Uri(link);
		//			client.DownloadFile(downloadURI, targetFileName);
		//		}

		//		return Ok();
		//	}
		//	catch (Exception ex)
		//	{
		//		return BadRequest(ex);
		//	}
		//}
		[HttpGet("home/get-file")]
		[AllowAnonymous]
		public async Task<ActionResult> GetFilesFirebase()
		{
			try
			{
				var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
				var getToken = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

				var task = new FirebaseStorage(
					Bucket,
					new FirebaseStorageOptions
					{
						AuthTokenAsyncFactory = () => Task.FromResult(getToken.FirebaseToken),
						ThrowOnCancel = true
					})
					.Child("image\\Hinh1.png");

				var download = await task.GetDownloadUrlAsync();

				string targetFileName = @"Hinh1.png";
				using (WebClient client = new WebClient())
				{
					Uri downloadURI = new Uri(download);
					client.DownloadFile(downloadURI, targetFileName);
				}
				return Ok();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("home/get-authen")]
		[AllowAnonymous]
		public async Task<ActionResult> CheckAuthen()
		{
			try
			{

				var authen = await _homeService.AuthenticateUserAsync(AuthEmail, AuthPassword);
				return Ok();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[AllowAnonymous]
		[HttpPost("home/upload-file")]
		public async Task<ActionResult> UpLoadFile()
		{
			try
			{
				var fileupload = "TestUpload.txt";
				FileStream fs;
				string path = Path.Combine(_env.ContentRootPath, "E:\\Lamviec\\Lamviec");
				fs = new FileStream(Path.Combine(path, fileupload), FileMode.Open);

				var result = await _homeService.UploadFileAsync(fs, fileupload, AuthEmail, AuthPassword);

				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[AllowAnonymous]
		[HttpGet("home/get-list-file")]
		public async Task<ActionResult> GetListFile()
		{
			try
			{
				var result =  await _homeService.GetFileAsync();
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

		[AllowAnonymous]
		[HttpGet("home/download-file")]
		public async Task<ActionResult> DownLoadFile()
		{
			try
			{
				await _homeService.DownloadFileAsync("image/TestUpload.txt", "E:\\Lamviec");
				return Ok();
			}
			catch (Exception ex)
			{
				return BadRequest(ex);
			}
		}

	}
}
