using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITEST.Core.Model
{
	public class UserModel
	{
		public string Token { get; set; }
		public string EmployeeCode { get; set; }
		public string EmployeeCodeFull { get; set; }
		public int Res { get; set; }
		public string Avatar { get; set; }
		public string FullName { get; set; }
		public DateTime StartDate { get; set; }
		public string Department { get; set; }
		public string Group { get; set; }
		public string Position { get; set; }
		public int PositionCategory { get; set; }
		public int TotalRequestPending { get; set; }
		public string Email { get; set; }
		public int EmployeeId { get; set; }
	}
}
