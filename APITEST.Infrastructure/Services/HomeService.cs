using APITEST.Core.Model;
using APITEST.Core.Reponse;
using APITEST.Infrastructure.Database;
using APITEST.Infrastructure.IServices;
using Dapper;
using FirebaseAdmin.Auth;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
		public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string authenEmail, string authenPassword)
		{
			try
			{
				var toKen = await AuthenticateUserAsync(authenEmail, authenPassword);
				if (toKen == "")
				{
					return toKen;
				}
				//_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", toKen);

				var objectName = "image/" + fileName; // Đường dẫn và tên tệp tin trên Firebase Storage
				var contentType = "application/octet-stream"; // Kiểu nội dung của tệp tin

				var uploadObject = await _storageClient.UploadObjectAsync("uploadfile-754fe.appspot.com", objectName, contentType, fileStream, null);

				return uploadObject.MediaLink;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
		public async Task<string> AuthenticateUserAsync(string authName, string authPassword)
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
				if (response.IsSuccessStatusCode)
				{
					var responseJson = JsonDocument.Parse(responseContent);
					var idToken = responseJson.RootElement.GetProperty("idToken").GetString();

					return idToken;
				}
				else
				{
					return $"Failed to sign in with email and password: {responseContent}";
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
		public async Task<IEnumerable<string>> GetFileAsync()
		{
			var objecfiles = _storageClient.ListObjectsAsync(_bucket);
			List<string> fileName = new List<string>();
			await foreach (var obj in objecfiles)
			{
				fileName.Add(obj.MediaLink);
			}
			return fileName;
		}

		public async Task DownloadFileAsync(string fileName, string localPath)
		{
			try
			{
				var storageObject = await _storageClient.GetObjectAsync(_bucket, fileName);

				await using var fileStream = File.Create(localPath);
				await _storageClient.DownloadObjectAsync(storageObject, fileStream);
			}
			catch (Exception ex)
			{

			}

		}
	}
}
