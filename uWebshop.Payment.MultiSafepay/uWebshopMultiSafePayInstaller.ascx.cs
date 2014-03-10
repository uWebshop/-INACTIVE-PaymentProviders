using System;
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
	public partial class MultiSafePayInstaller : UserControl
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

			var accountId = "#YOUR MULTISAFEPAY ACCOUNT ID#";
			var siteId = "#YOUR MULTISAFEPAY SITE ID#";
			var secureId = "#YOUR MULTISAFEPAY SECURE SITE ID#";

			if (!string.IsNullOrEmpty(txtAccountId.Text))
			{
				accountId = txtAccountId.Text;
			}

			if (!string.IsNullOrEmpty(txtSiteId.Text))
			{
				siteId = txtSiteId.Text;
			}

			if (!string.IsNullOrEmpty(txtSecureSiteId.Text))
			{
				secureId = txtSecureSiteId.Text;
			}

			var paymentProviderXml = HttpContext.Current.Server.MapPath(configfile);

			if (paymentProviderXml != null)
			{
				var paymentProviderXDoc = XDocument.Load(paymentProviderXml);

				if (paymentProviderXDoc.Descendants("provider").Any(x =>
					{
						var xAttribute = x.Attribute("title");
						return xAttribute != null && xAttribute.Value == "MultiSafePay";
					}))
				{
					BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "MultiSafePay config", "MultiSafePay config already created");
				}
				else
				{
					//<provider title="MultiSafePay">
					//     <accountId>#YOUR MULTISAFEPAY ACCOUNT ID#</accountId>
					//     <siteId>#YOUR MULTISAFEPAY SITE ID#</siteId>
					//     <siteSecureId>#YOUR MULTISAFEPAY SECURE SITE ID#</siteSecureId>
					//     <url>https://api.multisafepay.com/ewx/</url>
					//     <testURL>https://testapi.multisafepay.com/ewx/</testURL>
					// </provider>


					var ProviderNode = new XElement("provider", new XAttribute("title", "MultiSafePay"), new XElement("accountId", accountId), new XElement("siteId", siteId), new XElement("siteSecureId", secureId), new XElement("url", "https://api.multisafepay.com/ewx/"), new XElement("testURL", "https://testapi.multisafepay.com/ewx/"));

					paymentProviderXDoc.Descendants("providers").FirstOrDefault().Add(ProviderNode);

					paymentProviderXDoc.Save(paymentProviderXml);

					var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

					var author = new User(0);

					var uwbsPaymentProviderSectionDoc = Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

					var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

					if (uwbsPaymentProviderSectionDoc != null)
					{
						var providerDoc = Document.MakeNew("MultiSafePay", dtuwbsPaymentProvider, author, uwbsPaymentProviderSectionDoc.Id);
						providerDoc.SetProperty("title", "MultiSafePay");
						providerDoc.SetProperty("description", "MultiSafePay Payment Provider for uWebshop");

						providerDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());
						providerDoc.SetProperty("dllName", "uWebshop.Payment.MultiSafePay");

						providerDoc.Save();

						BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "MultiSafePay Installed!", "MultiSafePay config added and node created");
					}
				}
			}
		}
	}
}