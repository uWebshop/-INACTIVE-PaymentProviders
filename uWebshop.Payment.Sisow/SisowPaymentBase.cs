using System.IO;
using System.Net;

namespace uWebshop.Payment.Sisow
{
	public class SisowPaymentBase
	{
		public string GetName()
		{
			return "Sisow";
		}

		internal static string HttpGet(string url)
		{
			var req = WebRequest.Create(url) as HttpWebRequest;
			string result;
			using (var resp = req.GetResponse() as HttpWebResponse)
			{
				var reader = new StreamReader(resp.GetResponseStream());
				result = reader.ReadToEnd();
			}
			return result;
		}

		
	}
}
