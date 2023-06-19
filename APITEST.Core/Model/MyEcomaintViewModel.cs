using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITEST.Core.Model
{
	public class MyEcomaintViewModel
	{
		public string MS_MAY { get; set; }
		public string TEN_MAY { get; set; }
		public int TREGS { get; set; }
        public string sListYC { get; set; }
        public string sListBT { get; set; }
        public IEnumerable<MyEcomaintYeuCauModel> ListYC { get; set; }
		public IEnumerable<MyEcomaintBaoTriModel> ListBT { get; set; }
	}

	public class MyEcomaintYeuCauModel
	{
		public string MS_YEU_CAU { get; set; }

		public int TREYC { get; set; }
		public int MUC_YC { get; set; }
		public int DUYET_YC { get; set; }
	}
	public class MyEcomaintBaoTriModel
	{
		public string MS_PHIEU_BAO_TRI { get; set; }

		public int TREBT { get; set; }
		public int MUC_BT { get; set; }
		public int HH_BT { get; set; }
	}
}
