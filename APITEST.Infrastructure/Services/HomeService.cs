using APITEST.Core.Model;
using APITEST.Core.Reponse;
using APITEST.Infrastructure.Database;
using APITEST.Infrastructure.IServices;
using Dapper;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;

namespace APITEST.Infrastructure.Services
{
	public class HomeService : IHomeService
	{
		private readonly IDapperService _dapper;
		private readonly StorageClient _storageClient;
		private readonly FirebaseAuth _firebaseAuth;
		private readonly HttpClient _httpClient;

		private readonly string apiKey = "AIzaSyDln2NkoGScX86HiXaLMhLdKM-9Mihm7VM";
		private readonly string _bucket = "uploadfile-754fe.appspot.com";
		public HomeService(IDapperService dapper)
		{
			_dapper = dapper;
			_storageClient = StorageClient.Create();
			_firebaseAuth = FirebaseAuth.DefaultInstance;
			_httpClient = new HttpClient();
		}

		public async Task<BaseResponse<IEnumerable<LocationModel>>> GetLocation(string UName, int NNgu, int CoAll)
		{
			try
			{
				DynamicParameters? p = new DynamicParameters();
				p.Add("@UserName", UName);
				p.Add("@NNgu", NNgu);
				p.Add("@CoAll", CoAll);

				List<LocationModel>? res = await _dapper.GetAll<LocationModel>("GetNhaXuongAll", p, System.Data.CommandType.StoredProcedure);
				IEnumerable<LocationModel>? location = res.Select(x => new LocationModel()
				{
					MS_N_XUONG = x.MS_N_XUONG,
					TEN_N_XUONG = x.TEN_N_XUONG
				}).AsEnumerable();

				return BaseResponse<IEnumerable<LocationModel>>.Success(location);
			}
			catch (Exception ex)
			{
				return BaseResponse<IEnumerable<LocationModel>>.InternalServerError(ex);
			}
		}
		public async Task<BaseResponse<IEnumerable<MachineModel>>> GetMachine(string UName, int NNgu, int CoAll)
		{
			try
			{
				DynamicParameters? p = new DynamicParameters();
				p.Add("@UserName", UName);
				p.Add("@NNgu", NNgu);
				p.Add("@CoAll", CoAll);

				List<MachineModel>? res = await _dapper.GetAll<MachineModel>("GetLoaiMayAll", p, System.Data.CommandType.StoredProcedure);
				IEnumerable<MachineModel>? machines = res.Select(x => new MachineModel()
				{
					MS_LOAI_MAY = x.MS_LOAI_MAY,
					TEN_LOAI_MAY = x.TEN_LOAI_MAY
				}).AsEnumerable();

				return BaseResponse<IEnumerable<MachineModel>>.Success(machines);
			}
			catch (Exception ex)
			{
				return BaseResponse<IEnumerable<MachineModel>>.InternalServerError(ex);
			}
		}
		public async Task<BaseResponse<IEnumerable<MyEcomaintViewModel>>> GetMyEcomain(string username, int languages, DateTime? dngay, string ms_nx, string mslmay, bool xuly, int pageIndex, int pageSize)
		{
			try
			{
				var p = new DynamicParameters();
				p.Add("@sDanhMuc", "GET_MYECOMAINT");
				p.Add("@DNgay", dngay);
				p.Add("@UserName", username);
				p.Add("@MsNXuong", ms_nx);
				p.Add("@sCot1", mslmay);
				p.Add("@NNgu", languages);
				p.Add("@bcot1", xuly);
				//int TotalRows = p.Get<int>("@TotalRows");

				List<MyEcomaintViewModel>? myEcomaint = new List<MyEcomaintViewModel>();
				List<MyEcomaintYeuCauModel>? myEYeuCau = new List<MyEcomaintYeuCauModel>();
				List<MyEcomaintBaoTriModel>? myEBaoTri = new List<MyEcomaintBaoTriModel>();

				await _dapper.QueryMultipleAsync("spCMMSWEB", p, CommandType.StoredProcedure, resultDataTable =>
				{
					myEcomaint = resultDataTable.Read<MyEcomaintViewModel>().AsList();
					//myEYeuCau = resultDataTable.Read<MyEcomaintYeuCauModel>().AsList();
					//myEBaoTri = resultDataTable.Read<MyEcomaintBaoTriModel>().AsList();
				});

				IEnumerable<MyEcomaintViewModel> reslut = myEcomaint.Select(x => new MyEcomaintViewModel()
				{
					MS_MAY = x.MS_MAY,
					TEN_MAY = x.TEN_MAY,
					TREGS = x.TREGS,
					ListYC = JsonConvert.DeserializeObject<IEnumerable<MyEcomaintYeuCauModel>>(x.sListYC).AsEnumerable(),
					ListBT = JsonConvert.DeserializeObject<IEnumerable<MyEcomaintBaoTriModel>>(x.sListBT).AsEnumerable(),
				});


				return BaseResponse<IEnumerable<MyEcomaintViewModel>>.Success(reslut);

				//List<MyEcomaintViewModel>? res = await _dapper.QueryMultipleAsync<MyEcomaintViewModel>("spCMMSWEB", p, CommandType.StoredProcedure);
				//res.Where(x => x.sListBT != "").ToList().ForEach(r => r.ListBT = JsonConvert.DeserializeObject<List<MyEcomaintBaoTriModel>>(r.sListBT));
				//return res.OrderBy(x => x.MS_MAY).ToList();

			}
			catch (Exception ex)
			{
				return BaseResponse<IEnumerable<MyEcomaintViewModel>>.InternalServerError(ex);
			}
		}
		public async Task<BaseResponse<string>> UploadFileAsync(Stream fileStream, string fileName)
		{
			try
			{
				var objectName = fileName; // Đường dẫn và tên tệp tin trên Firebase Storage
				var contentType = "application/octet-stream"; // Kiểu nội dung của tệp tin

				// kiểm tồn tại
				BaseResponse<IEnumerable<string>> listFiles = await GetFileAsync();
				foreach(var files in listFiles.ResponseData)
				{
					if(objectName == files) //nếu tồn tại thì + thêm ngày vào file
					{
						objectName = DateTime.Now.ToString("ddMMyyyy-") + files;
					}
				}

				//var uploadObjectOptions = new UploadObjectOptions
				//{
				//	PredefinedAcl = PredefinedObjectAcl.PublicRead, // Cấp quyền truy cập công khai cho tệp tin (tùy chọn)
				//	IfGenerationMatch = 0, // Kiểm tra thế hệ tệp tin (tùy chọn)
				//	IfMetagenerationMatch = 0, // Kiểm tra thế hệ metadata (tùy chọn)
				//};

				//// Gắn idToken vào header
				//var credential = GoogleCredential.FromAccessToken(token);
				//var storageClient = StorageClient.Create(credential);
				var uploadObject = await _storageClient.UploadObjectAsync(_bucket, objectName, contentType, fileStream);

				return BaseResponse<string>.Success(uploadObject.MediaLink);
			}
			catch (Exception ex)
			{
				return BaseResponse<string>.InternalServerError(ex);
			}
		}
		public async Task<BaseResponse<object>> AuthenticateUserAsync(string authName, string authPassword)
		{
			try
			{
				var request = new
				{
					email = authName,
					password = authPassword,
					returnSecureToken = true
				};

				var requestBody = System.Text.Json.JsonSerializer.Serialize(request);
				var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

				var signInUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";

				var response = await _httpClient.PostAsync(signInUrl, content);
				var responseContent = await response.Content.ReadAsStringAsync();
				var responseJson = JsonDocument.Parse(responseContent);
				if (response.IsSuccessStatusCode)
				{
					var idToken = responseJson.RootElement.GetProperty("idToken").GetString();

					return BaseResponse<object>.Success(idToken);
				}
				else
				{

					return BaseResponse<object>.BadRequest(responseJson.RootElement.GetProperty("error").GetProperty("message").GetString());
				}
			}
			catch (Exception ex)
			{
				return BaseResponse<object>.InternalServerError(ex);
			}
		}
		public async Task<BaseResponse<IEnumerable<string>>> GetFileAsync()
		{
			try
			{
				var objecfiles = _storageClient.ListObjectsAsync(_bucket);
				List<string> fileName = new List<string>();
				await foreach (var obj in objecfiles)
				{
					fileName.Add(obj.Name);
				}
				return BaseResponse<IEnumerable<string>>.Success(fileName);
			}
			catch (Exception ex)
			{
				return BaseResponse<IEnumerable<string>>.InternalServerError(ex);
			}

		}

		public async Task<BaseResponse<string>> DownloadFileAsync(string fileName, string localPath, string pathFile)
		{
			try
			{
				//var storageObject = await _storageClient.GetObjectAsync(_bucket, fileName);

				var source = pathFile;
				var destination = $"{localPath}\\{fileName}";


				using (var stream = File.Create(destination))
				{
					// IDownloadProgress defined in Google.Apis.Download namespace
					//var progress = new Progress<IDownloadProgress>(
					//    p => Console.WriteLine($"bytes: {p.BytesDownloaded}, status: {p.Status}")
					//);

					// Download source object from bucket to local file system
					var result = await _storageClient.DownloadObjectAsync(_bucket, source, stream, null);
					return BaseResponse<string>.Success("Download Sucess");
				}
			}
			catch (Exception ex)
			{
				return BaseResponse<string>.BadRequest(ex.Message);
			}
		}

		public async Task<BaseResponse<TeamviewerHelper>> GetInfoUltraViewer()
		{
			try
			{
				Process.Start("C:\\Program Files (x86)\\UltraViewer\\UltraViewer_Desktop.exe");
				// Tạm dừng thực thi trong 5 giây
				Thread.Sleep(5000);

				var userInfo = GetUser("UltraViewer 6.4 - Free", "WindowsForms10.EDIT.app.0.34f5582_r14_ad1");

				return BaseResponse<TeamviewerHelper>.Success(userInfo);
			}
			catch(Exception ex)
			{
				return BaseResponse<TeamviewerHelper>.InternalServerError(ex);
			}
		}
		public class WindowsApi
		{
			[DllImport("User32.dll", EntryPoint = "FindWindow")]
			public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

			[DllImport("User32.dll", EntryPoint = "FindWindowEx")]
			public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);


			[DllImport("User32.dll", EntryPoint = "SendMessage")]
			public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

			[DllImport("user32.dll", EntryPoint = "GetWindowText")]
			public static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int cch);

			[DllImport("user32.dll", SetLastError = true)]
			public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

			[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

			[DllImport("user32.dll", EntryPoint = "ShowWindow")]
			public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		}
		public enum GetWindowCmd : uint
		{
			GW_HWNDFIRST = 0,
			GW_HWNDLAST = 1,
			GW_HWNDNEXT = 2,
			GW_HWNDPREV = 3,
			GW_OWNER = 4,
			GW_CHILD = 5,
			GW_ENABLEDPOPUP = 6
		}
		private static Regex userReg;
		public class TeamviewerHelper
		{
			static TeamviewerHelper()
			{
				userReg = new Regex(@"\d+ \d+ \d+", RegexOptions.Singleline | RegexOptions.Compiled);
			}
			public TeamviewerHelper()
			{
				Username = string.Empty;
				Password = string.Empty;
				Holder = string.Empty;
			}
			internal int _count;
			public string Username;
			public string Password;
			public string Holder;
		}
		public static TeamviewerHelper GetUser(string titleApp, string className)
		{
			TeamviewerHelper user = new TeamviewerHelper();
			IntPtr tvHwnd = WindowsApi.FindWindow(null, titleApp);
			if (tvHwnd != IntPtr.Zero)
			{
				IntPtr winParentPtr = WindowsApi.GetWindow(tvHwnd, GetWindowCmd.GW_CHILD);
				while (winParentPtr != IntPtr.Zero)
				{

					IntPtr winSubPtr = WindowsApi.GetWindow(winParentPtr, GetWindowCmd.GW_CHILD);
					while (winSubPtr != IntPtr.Zero)
					{
						StringBuilder controlName = new StringBuilder(512);
						WindowsApi.GetClassName(winSubPtr, controlName, controlName.Capacity);

						if (controlName.ToString() == className)
						{
							var a = controlName;
							StringBuilder winMessage = new StringBuilder(512);
							WindowsApi.SendMessage(winSubPtr, 0xD, (IntPtr)winMessage.Capacity, winMessage);
							string message = winMessage.ToString();
							if (userReg.IsMatch(message))
							{
								user.Username = message;
								user._count += 1;

							}
							else if (user.Password != string.Empty)
							{
								user.Holder = message;
								user._count += 1;
							}
							else
							{
								user.Password = message;
								user._count += 1;
							}
							if (user._count == 100)
							{
								return user;
							}
						}
						winSubPtr = WindowsApi.GetWindow(winSubPtr, GetWindowCmd.GW_HWNDNEXT);
					}
					winParentPtr = WindowsApi.GetWindow(winParentPtr, GetWindowCmd.GW_HWNDNEXT);
				}
			}
			return user;
		}
	}
}
