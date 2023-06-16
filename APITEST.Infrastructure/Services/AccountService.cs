using APITEST.Core.ExternalAPI;
using APITEST.Core.InMemoryStore;
using APITEST.Core.Model;
using APITEST.Core.Reponse;
using APITEST.Infrastructure.Database;
using APITEST.Infrastructure.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Http;
using Dapper;
using System.Data;

namespace APITEST.Infrastructure.Services
{
	public class AccountService : IAccountService
	{
		private readonly IDapperService _dapper;
		protected JWTConfig _jwtConfig { get; set; }

		public AccountService(IConfiguration configuration, IDapperService dapper)
		{
			_jwtConfig = configuration.GetSection("JWTConfig").Get<JWTConfig>();
			_dapper = dapper;
		}


		//private async Task<bool> CreateUser(ApplicationUser user)
		//{
		//	IdentityResult result = await _userManager.CreateAsync(user);
		//	if (result.Succeeded)
		//	{
		//		IdentityResult claimResult = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, user.UserName));
		//		return true;
		//	}
		//	return false;
		//}

		//private async Task<string> GenerateToken(string employeeCode)
		//{
		//	bool result = false;
		//	ApplicationUser user = await _userManager.FindByNameAsync(employeeCode.ToString());
		//	if (user == null)
		//	{
		//		user = new ApplicationUser()
		//		{
		//			UserName = employeeCode
		//		};
		//		result = await CreateUser(user ?? new ApplicationUser());
		//	}
		//	else
		//	{
		//		result = true;
		//	}
		//	if (result)
		//	{
		//		await _signInManager.SignInAsync(user, false, null);
		//		JwtSecurityTokenHandler? tokenHandler = new JwtSecurityTokenHandler();
		//		byte[]? key = Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);
		//		SecurityTokenDescriptor? tokenDescriptor = new SecurityTokenDescriptor
		//		{
		//			Subject = new ClaimsIdentity(new Claim[]
		//			{
		//			new Claim(ClaimTypes.Name, employeeCode.ToString())
		//			}),
		//			Expires = DateTime.UtcNow.AddDays(0.5),
		//			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		//		};
		//		SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
		//		string? tokenString = tokenHandler.WriteToken(token);

		//		user.Tokens.Add(new MemoryUserToken
		//		{
		//			UserId = user.Id,
		//			TokenName = "access-token",
		//			TokenValue = tokenString
		//		});
		//		await _userManager.UpdateAsync(user);
		//		return tokenString;
		//	}
		//	else
		//	{
		//		return string.Empty;
		//	}

		//}

		public async Task<BaseResponse<UserModel>> Login(string userName, string password)
		{
			try
			{
				//DynamicParameters? p = new DynamicParameters();
				//p.Add("@sDanhMuc", "LOGIN");
				//p.Add("@MS_CN", userName);
				//p.Add("@MAT_KHAU", "QiLWP9iaketh5qd3IErKTKu46Hr6bERB");
				//UserModel? resultUser = await _dapper.Execute<UserModel>("spWThongTin", p, CommandType.StoredProcedure);
				//if(resultUser.Res == 1)
				//{
				//	string? token = await GeneralToken(userName);
				//	UserModel? userProfile = await GetUserProfile(userName);
				//	if(userProfile != null)
				//	{
				//		userProfile.Token = token;
				//	}
				//	return BaseResponse<UserModel>.Success(userProfile);
				//}
				//else
				//{
				//	return BaseResponse<UserModel>.BadRequest(null);
				//}
				string? token = await GeneralToken(userName);
				UserModel? userProfile = new UserModel();
				userProfile.Token = token;

				return BaseResponse<UserModel>.Success(userProfile);
			}
			catch (Exception ex) {
				return BaseResponse<UserModel>.BadRequest(null, ex.Message);
			}
		}

		private async Task<UserModel> GetUserProfile(string username)
		{
			UserModel? result = null;
			try
			{
				DynamicParameters? p = new DynamicParameters();
				p.Add("@sDanhMuc", "LOGIN_SCREEN");
				p.Add("@MS_CN", username);

				User? user = await _dapper.Execute<User>("spWThongTin", p, CommandType.StoredProcedure);
				if(user!= null)
				{
					result = new UserModel()
					{
						Avatar = user.HINH_CN,
						EmployeeCode = user.MS_CN,
						FullName = user.HO_TEN,
						EmployeeId = user.ID_CN,
						StartDate = user.NGAY_VAO_CTY,
						Group = user.TEN_TO,
						Position = user.TEN_CV,
						PositionCategory = user.LOAI_CHUC_VU,
						TotalRequestPending = user.SL_CHUA_DUYET
					};
				}
				return result;
			}
			catch
			{
				return result;
			}
		}

		#region Token
		private async Task<string> GeneralToken(string username)
		{
			try
			{
				var claims = new[]
					{
					new Claim(JwtRegisteredClaimNames.Sub,username),
                    // this guarantees the token is unique
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				};

				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
				var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

				var securityToken = new JwtSecurityToken(
						issuer: "VietSoft.api",
						audience: "VietSoft.api",
						expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(5)),
						claims: claims,
						signingCredentials: creds
					);
				string? token = new JwtSecurityTokenHandler().WriteToken(securityToken);
				return token;
			}
			catch (Exception ex)
			{
				return String.Empty;
			}
		}
		#endregion

	}
}
