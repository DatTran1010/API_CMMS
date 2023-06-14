using APITEST.Core.Model;
using APITEST.Core.Reponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITEST.Infrastructure.IServices
{
	public interface IAccountService
	{
		Task<BaseResponse<UserModel>> Login(string emplyeeCode, string password);
	}
}
