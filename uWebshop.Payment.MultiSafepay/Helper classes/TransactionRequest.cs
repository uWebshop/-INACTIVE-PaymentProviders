using System.Xml;

namespace uWebshop.Payment.MultiSafePay
{
	public class TransactionRequest
	{
		public long AccountId { get; set; }
		public long SiteId { get; set; }
		public long SiteSecureId { get; set; }

		public string NotificationUrl { get; set; }
		public string CancelUrl { get; set; }
		public string RedirectUrl { get; set; }
		public bool CloseWindow { get; set; }

		public string Locale { get; set; }
		public string IPAddress { get; set; }
		public string ForwardedIP { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string Housenumber { get; set; }
		public string Zipcode { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }

		public string TransactionId { get; set; }
		public string Currency { get; set; }
		public decimal Amount { get; set; }
		public string Decription { get; set; }
		public bool Manual { get; set; }
		public string Gateway { get; set; }

		public string Signature { get; set; }

		public string GetXml()
		{
			// <?xml version="1.0" encoding="UTF-8" ?>
			// <redirecttransaction ua="Example 1.0">
			//     <merchant>
			//         <account>10011001</account>
			//         <site_id>1234</site_id>
			//         <site_secure_code>123456</site_secure_code>
			//         <notification_url>http://www.example.com/notify/</notification_url>
			//         <cancel_url>http://www.example.com/cancel/</cancel_url>
			//         <redirect_url>http://www.example.com/redirect/</redirect_url>
			//         <close_window>false</close_window>
			//     </merchant>
			//     <customer>
			//         <locale>nl_NL</locale>
			//         <ipaddress>85.92.148.67</ipaddress>
			//         <forwardedip></forwardedip>
			//         <firstname>Jan</firstname>
			//         <lastname>Modaal</lastname>
			//         <address1>Teststraat</address1>
			//         <address2></address2>
			//         <housenumber>12</housenumber>
			//         <zipcode>1234AB</zipcode>
			//         <city>Amsterdam</city>
			//         <state>NH</state>
			//         <country>NL</country>
			//         <phone>012-3456789</phone>
			//         <email>info@example.com</email>
			//     </customer>
			//     <transaction>
			//         <id>4084044</id>
			//         <currency>EUR</currency>
			//         <amount>1000</amount>
			//         <description>Test transaction</description>
			//         <var1></var1> <var2></var2>
			//         <var3></var3>
			//         <items></items>
			//         <manual>false</manual>
			//     </transaction>
			//     <signature>d54e019e2bc1a9de0cae1286d388f423</signature>
			// </redirecttransaction>

			var xmlDoc = new XmlDocument();
			var gatewaysNode = (XmlElement) xmlDoc.AppendChild(xmlDoc.CreateElement("redirecttransaction"));
			gatewaysNode.SetAttribute("ua", "Example 1.0");

			var merchantNode = (XmlElement) gatewaysNode.AppendChild(xmlDoc.CreateElement("merchant"));
			merchantNode.AppendChild(xmlDoc.CreateElement("account")).InnerText = AccountId.ToString();
			merchantNode.AppendChild(xmlDoc.CreateElement("site_id")).InnerText = SiteId.ToString();
			merchantNode.AppendChild(xmlDoc.CreateElement("site_secure_code")).InnerText = SiteSecureId.ToString();
			merchantNode.AppendChild(xmlDoc.CreateElement("notification_url")).InnerText = NotificationUrl;
			merchantNode.AppendChild(xmlDoc.CreateElement("cancel_url")).InnerText = CancelUrl;
			merchantNode.AppendChild(xmlDoc.CreateElement("redirect_url")).InnerText = RedirectUrl;
			merchantNode.AppendChild(xmlDoc.CreateElement("close_window")).InnerText = CloseWindow.ToString();

			var customerNode = (XmlElement) gatewaysNode.AppendChild(xmlDoc.CreateElement("customer"));
			customerNode.AppendChild(xmlDoc.CreateElement("locale")).InnerText = Locale;
			customerNode.AppendChild(xmlDoc.CreateElement("ipaddress")).InnerText = IPAddress;
			//customerNode.AppendChild(xmlDoc.CreateElement("forwardedip")).InnerText = ForwardedIP;
			customerNode.AppendChild(xmlDoc.CreateElement("firstname")).InnerText = FirstName;
			customerNode.AppendChild(xmlDoc.CreateElement("lastname")).InnerText = LastName;
			//customerNode.AppendChild(xmlDoc.CreateElement("address1")).InnerText = Address1;
			//customerNode.AppendChild(xmlDoc.CreateElement("address2")).InnerText = Address2;
			//customerNode.AppendChild(xmlDoc.CreateElement("housenumber")).InnerText = Housenumber;
			//customerNode.AppendChild(xmlDoc.CreateElement("zipcode")).InnerText = Zipcode;
			//customerNode.AppendChild(xmlDoc.CreateElement("city")).InnerText = City;
			//customerNode.AppendChild(xmlDoc.CreateElement("state")).InnerText = State;
			customerNode.AppendChild(xmlDoc.CreateElement("country")).InnerText = Country;
			//customerNode.AppendChild(xmlDoc.CreateElement("phone")).InnerText = Phone;
			customerNode.AppendChild(xmlDoc.CreateElement("email")).InnerText = Email;

			var transactionNode = (XmlElement) gatewaysNode.AppendChild(xmlDoc.CreateElement("transaction"));
			transactionNode.AppendChild(xmlDoc.CreateElement("id")).InnerText = TransactionId;
			transactionNode.AppendChild(xmlDoc.CreateElement("currency")).InnerText = Currency;
			transactionNode.AppendChild(xmlDoc.CreateElement("amount")).InnerText = Amount.ToString();
			transactionNode.AppendChild(xmlDoc.CreateElement("description")).InnerText = Decription;
			transactionNode.AppendChild(xmlDoc.CreateElement("manual")).InnerText = Manual.ToString();
			transactionNode.AppendChild(xmlDoc.CreateElement("gateway")).InnerText = Gateway;

			var SignatureNode = (XmlElement) gatewaysNode.AppendChild(xmlDoc.CreateElement("signature"));
			SignatureNode.InnerText = Signature;

			return xmlDoc.OuterXml;
		}
	}
}