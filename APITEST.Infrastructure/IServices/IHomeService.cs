using APITEST.Core.Model;
using APITEST.Core.Reponse;
using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITEST.Infrastructure.IServices
{
	public interface IHomeService
	{
		public Task<BaseResponse<IEnumerable<LocationModel>>> GetLocation(string UName, int NNgu, int CoAll);
		public Task<BaseResponse<IEnumerable<MachineModel>>> GetMachine(string UName, int NNgu, int CoAll);
		public Task<BaseResponse<IEnumerable<MyEcomaintViewModel>>> GetMyEcomain(string username, int languages, DateTime? dngay, string ms_nx, string mslmay, bool xuly, int pageIndex, int pageSize);
		public Task<string> AuthenticateUserAsync(string authName, string authPassword);
		public Task<string> UploadFileAsync(Stream fileStream, string fileName, string authenEmail, string authenPassword);
		public Task<IEnumerable<string>> GetFileAsync();
		public Task DownloadFileAsync(string fileName, string localPath);
	}
}
