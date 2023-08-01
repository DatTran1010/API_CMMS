using APITEST.Core.Model;
using APITEST.Core.Reponse;
using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static APITEST.Infrastructure.Services.HomeService;

namespace APITEST.Infrastructure.IServices
{
	public interface IHomeService
	{
		public Task<BaseResponse<IEnumerable<LocationModel>>> GetLocation(string UName, int NNgu, int CoAll);
		public Task<BaseResponse<IEnumerable<MachineModel>>> GetMachine(string UName, int NNgu, int CoAll);
		public Task<BaseResponse<IEnumerable<MyEcomaintViewModel>>> GetMyEcomain(string username, int languages, DateTime? dngay, string ms_nx, string mslmay, bool xuly, int pageIndex, int pageSize);
		public Task<BaseResponse<object>> AuthenticateUserAsync(string authName, string authPassword);
		public Task<BaseResponse<string>> UploadFileAsync(Stream fileStream, string fileName);
		public Task<BaseResponse<IEnumerable<string>>> GetFileAsync();
		public Task<BaseResponse<string>> DownloadFileAsync(string fileName, string localPath, string pathFile);
		public Task<BaseResponse<TeamviewerHelper>> GetInfoUltraViewer();
	}
}
