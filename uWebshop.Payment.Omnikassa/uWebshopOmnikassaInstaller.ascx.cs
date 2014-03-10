using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.ContentTypes;
using umbraco;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;

namespace uWebshop.Payment
{
	public partial class OmnikassaInstaller : UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}


		protected void InstallConfig(object sender, EventArgs e)
		{
			var configfile = PaymentConfigHelper.GetPaymentProviderConfig();

			if (configfile == null)
			{
				BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Error!", "PaymentProviderConfig not found!");
				return;
			}

			var accountId = "#YOUR MERCHANTID#";
			var securityKey = "#YOUR SECURITYKEY#";

			if (!string.IsNullOrEmpty(txtMerchantId.Text))
			{
				accountId = txtMerchantId.Text;
			}

			if (!string.IsNullOrEmpty(txtSecurityKey.Text))
			{
				securityKey = txtSecurityKey.Text;
			}

			var paymentProviderXML = HttpContext.Current.Server.MapPath(configfile);

			if (paymentProviderXML != null)
			{
				var paymentProviderXDoc = XDocument.Load(paymentProviderXML);

				if (paymentProviderXDoc.Descendants("provider").Any(x =>
					{
						var xAttribute = x.Attribute("title");
						return xAttribute != null && xAttribute.Value == "Omnikassa";
					}))
				{
					BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "Omnikassa config", "Omnikassa config already created");
				}
				else
				{
					//<provider title="OmniKassa">
					//	   <MerchantId>#YOUR OmniKassa MerchantId#</MerchantId>      
					//	   <CurrencyCode>978</CurrencyCode>
					//	   <normalReturnUrl>http://www.yoursite.com</normalReturnUrl>
					//	   <KeyVersion>1</KeyVersion>   
					//	   <TestAmount>56</TestAmount>
					//	   <Url>https://payment-webinit.omnikassa.rabobank.nl/paymentServlet</Url>
					//	   <TestUrl>https://payment-webinit.simu.omnikassa.rabobank.nl/paymentServlet</TestUrl>  
					//<ForwardUrl>https://payment-web.omnikassa.rabobank.nl/payment</ForwardUrl>
					//  <TestForwardUrl>https://payment-web.simu.omnikassa.rabobank.nl/payment</TestForwardUrl>
					//</provider>
					//   </provider>  


					var paymentNode = new XElement("provider", new XAttribute("title", "Omnikassa"), new XElement("MerchantId", accountId), new XElement("CurrencyCode", "978"), new XElement("normalReturnUrl", HttpContext.Current.Request.Url.Authority), new XElement("KeyVersion", "1"), new XElement("TestAmount", "1000"), new XElement("url", "https://payment-webinit.omnikassa.rabobank.nl/paymentServlet"), new XElement("testURL", "https://payment-webinit.simu.omnikassa.rabobank.nl/paymentServlet"), new XElement("ForwardUrl", "https://payment-webinit.omnikassa.rabobank.nl/payment"), new XElement("TestForwardUrl", "https://payment-webinit.simu.omnikassa.rabobank.nl/payment"));

					paymentProviderXDoc.Descendants("providers").FirstOrDefault().Add(paymentNode);

					paymentProviderXDoc.Save(paymentProviderXML);

					var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

					var author = new User(0);

					var uwbsPaymentProviderSectionDoc = Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

					var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

					if (uwbsPaymentProviderSectionDoc != null)
					{
						var providerDoc = Document.MakeNew("Omnikassa", dtuwbsPaymentProvider, author, uwbsPaymentProviderSectionDoc.Id);
						providerDoc.SetProperty("title", "Omnikassa");
						providerDoc.SetProperty("description", "Omnikassa Payment Provider for uWebshop");

						providerDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());
						providerDoc.SetProperty("dllName", "uWebshop.Payment.Omnikassa");

						providerDoc.Save();

						BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "Omnikassa Installed!", "Omnikassa config added and nodes created");
					}
				}
			}

			var webConfigValue = ConfigurationManager.AppSettings["SecurityKey"];

			if (string.IsNullOrEmpty(webConfigValue))
			{
				try
				{
					ConfigurationManager.AppSettings.Add("SecurityKey", securityKey);
				}
				catch
				{
					BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Write to Web.config failed!", "Add: add key='SecurityKey' value='" + securityKey + "' to web.config AppSettings");
				}
			}
		}
	}
}