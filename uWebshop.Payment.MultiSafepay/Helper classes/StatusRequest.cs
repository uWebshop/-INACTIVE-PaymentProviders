using System.Xml;

namespace uWebshop.Payment.MultiSafePay
{
	public class StatusRequest
	{
		public long AccountId { get; set; }
		public long SiteId { get; set; }
		public long SiteSecureId { get; set; }

		public string TransactionId { get; set; }

		public string GetXml()
		{
			//<?xml version="1.0" encoding="UTF-8"?>
			//<status ua="Example 1.0">
			//  <merchant>
			//    <account>10011001</account>
			//    <site_id>1234</site_id>
			//    <site_secure_code>123456</site_secure_code>
			//  </merchant>
			//  <transaction>
			//    <id>4084044</id>
			//  </transaction>
			//</status>

			var xmlDoc = new XmlDocument();
			var gatewaysNode = (XmlElement) xmlDoc.AppendChild(xmlDoc.CreateElement("status"));
			gatewaysNode.SetAttribute("ua", "Example 1.0");

			var merchantNode = (XmlElement) gatewaysNode.AppendChild(xmlDoc.CreateElement("merchant"));
			merchantNode.AppendChild(xmlDoc.CreateElement("account")).InnerText = AccountId.ToString();
			merchantNode.AppendChild(xmlDoc.CreateElement("site_id")).InnerText = SiteId.ToString();
			merchantNode.AppendChild(xmlDoc.CreateElement("site_secure_code")).InnerText = SiteSecureId.ToString();

			var customerNode = (XmlElement) gatewaysNode.AppendChild(xmlDoc.CreateElement("transaction"));
			customerNode.AppendChild(xmlDoc.CreateElement("id")).InnerText = TransactionId;

			return xmlDoc.OuterXml;
		}
	}
}