using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITEST.Core.Model
{
	public class User
	{
		public string HINH_CN { get; set; }
		public int ID_CN { get; set; }
		public string MS_CN { get; set; }
		public string HO_TEN { get; set; }
		public string TEN_TO { get; set; }	
		public string TEN_XN { get; set; }
		public DateTime NGAY_VAO_CTY { get; set; }
		public int LOAI_CHUC_VU { get; set; }	
		public int SL_CHUA_DUYET { get; set; }
		public int ID_DV { get; set; }	
		public string TEN_CV { get; set; }	
	}
}
