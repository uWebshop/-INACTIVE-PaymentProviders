using System.Xml;

namespace uWebshop.Payment.MultiSafePay
{
	public class GatewayRequest
	{
		public long AccountId { get; set; }
		public int SiteId { get; set; }
		public int SiteSecureId { get; set; }
		public string Country { get; set; }

		public string GetXml()
		{
			// <?xml version="1.0" encoding="UTF-8" ?>
			// <gateways ua="Example 1.0">
			//     <merchant>
			//         <account>10011001</account>
			//         <site_id>1234</site_id>
			//         <site_secure_code>123456</site_secure_code>
			//     </merchant>
			//     <customer>
			//         <country>NL</country>
			//     </customer>
			// </gateways>

			var xmlDoc = new XmlDocument();
			var gatewaysNode = (XmlElement) xmlDoc.AppendChild(xmlDoc.CreateElement("gateways"));
			gatewaysNode.SetAttribute("ua", "Example 1.0");

			var merchantNode = (XmlElement) gatewaysNode.AppendChild(xmlDoc.CreateElement("merchant"));
			merchantNode.AppendChild(xmlDoc.CreateElement("account")).InnerText = AccountId.ToString();
			merchantNode.AppendChild(xmlDoc.CreateElement("site_id")).InnerText = SiteId.ToString();
			merchantNode.AppendChild(xmlDoc.CreateElement("site_secure_code")).InnerText = SiteSecureId.ToString();

			var customerNode = (XmlElement) gatewaysNode.AppendChild(xmlDoc.CreateElement("customer"));
			customerNode.AppendChild(xmlDoc.CreateElement("country")).InnerText = Country;

			return xmlDoc.OuterXml;
		}
	}
}