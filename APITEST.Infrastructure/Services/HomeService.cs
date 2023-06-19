using APITEST.Core.Model;
using APITEST.Core.Reponse;
using APITEST.Infrastructure.Database;
using APITEST.Infrastructure.IServices;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITEST.Infrastructure.Services
{
	public class HomeService : IHomeService
	{
		private readonly IDapperService _dapper;
		public HomeService(IDapperService dapper)
		{
			_dapper = dapper;
		}

		public async Task<BaseResponse<IEnumerable<LocationModel>>> GetLocation(string UName , int NNgu ,int CoAll )
		{
			try
			{
				DynamicParameters? p = new DynamicParameters();
				p.Add("@UserName", UName);
				p.Add("@NNgu", NNgu);
				p.Add("@CoAll", CoAll);

				List<LocationModel>? res =  await _dapper.GetAll<LocationModel>("GetNhaXuongAll", p, System.Data.CommandType.StoredProcedure);
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
			catch(Exception ex)
			{
				return BaseResponse<IEnumerable<MyEcomaintViewModel>>.InternalServerError(ex);
			}
		}
	}
}
