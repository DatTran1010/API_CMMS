namespace APITEST.Core.ExternalAPI
{
	public class JWTConfig
	{
		public string SecretKey { get; set; }
		public string Iss { get; set; }
		public double AccessTokenExpire { get; set; }
	}
}
