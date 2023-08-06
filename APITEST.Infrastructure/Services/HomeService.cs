using APITEST.Core.Model;
using APITEST.Core.Reponse;
using APITEST.Extensions;
using APITEST.Infrastructure.Database;
using APITEST.Infrastructure.IServices;
using CorePush.Google;
using Dapper;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
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
using static APITEST.Core.Model.GoogleNotification;
using static Google.Apis.Requests.BatchRequest;

namespace APITEST.Infrastructure.Services
{
	public class HomeService : IHomeService
	{
		private readonly IDapperService _dapper;
		private readonly StorageClient _storageClient;
		private readonly FirebaseAuth _firebaseAuth;
		private readonly HttpClient _httpClient;


        private readonly FcmNotificationSetting _fcmNotificationSetting;


        private readonly string apiKey = "AIzaSyDln2NkoGScX86HiXaLMhLdKM-9Mihm7VM";
		private readonly string _bucket = "uploadfile-754fe.appspot.com";
		public HomeService(IDapperService dapper, IOptions<FcmNotificationSetting> settings)
		{
			_dapper = dapper;
			_storageClient = StorageClient.Create();
			_firebaseAuth = FirebaseAuth.DefaultInstance;
			_httpClient = new HttpClient();
            _fcmNotificationSetting = settings.Value;

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


        #region notification
        public async Task<ResponseModel> SendNotification(NotificationModel notificationModel)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                if (notificationModel.IsAndroiodDevice)
                {
                    /* FCM Sender (Android Device) */
                    FcmSettings settings = new FcmSettings()
                    {
                        SenderId = _fcmNotificationSetting.SenderId,
                        ServerKey = _fcmNotificationSetting.ServerKey
                    };
                    HttpClient httpClient = new HttpClient();

                    string authorizationKey = string.Format("keyy={0}", settings.ServerKey);
                    string deviceToken = notificationModel.DeviceId;

                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
                    httpClient.DefaultRequestHeaders.Accept
                            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    DataPayload dataPayload = new DataPayload();
                    dataPayload.Title = notificationModel.Title;
                    dataPayload.Body = notificationModel.Body;

                    GoogleNotification notification = new GoogleNotification();
                    notification.Data = dataPayload;
                    notification.Notification = dataPayload;

                    var fcm = new FcmSender(settings, httpClient);
                    var fcmSendResponse = await fcm.SendAsync(deviceToken, notification);

                    if (fcmSendResponse.IsSuccess())
                    {
                        response.IsSuccess = true;
                        response.Message = "Notification sent successfully";
                        return response;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = fcmSendResponse.Results[0].Error;
                        return response;
                    }
                }
                else
                {

                    FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
    .VerifyIdTokenAsync(idToken);
                    string uid = decodedToken.Uid;


                    /* Code here for APN Sender (iOS Device) */
                    //var apn = new ApnSender(apnSettings, httpClient);
                    //await apn.SendAsync(notification, deviceToken);
                }
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Something went wrong";
                return response;
            }
        }
        #endregion
    }
}
