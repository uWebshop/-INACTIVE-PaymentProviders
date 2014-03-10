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
	public partial class PayPalInstaller : UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}


		protected void installConfig(object sender, EventArgs e)
		{
			var configfile = PaymentConfigHelper.GetPaymentProviderConfig();

			if (configfile == null)
			{
				BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Error!", "PaymentProviderConfig not found!");
				return;
			}

			var payPalAccountId = "#YOUR PAYPAL ACCOUNT ID/EMAIL#";

			if (!string.IsNullOrEmpty(txtPayPalAccountId.Text))
			{
				payPalAccountId = txtPayPalAccountId.Text;
			}

			var paymentProviderXML = HttpContext.Current.Server.MapPath(configfile);

			if (paymentProviderXML != null)
			{
				var paymentProviderXDoc = XDocument.Load(paymentProviderXML);

				if (paymentProviderXDoc.Descendants("provider").Any(x =>
					{
						var xAttribute = x.Attribute("title");
						return xAttribute != null && xAttribute.Value == "PayPal";
					}))
				{
					BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "Paypal config", "PayPal config already created");
				}
				else
				{
					var PayPalNode = new XElement("provider", new XAttribute("title", "PayPal"), new XElement("accountId", payPalAccountId), new XElement("url", "https://www.paypal.com/cgi-bin/webscr"), new XElement("testURL", "https://www.sandbox.paypal.com/us/cgi-bin/webscr"));

					paymentProviderXDoc.Descendants("providers").FirstOrDefault().Add(PayPalNode);

					paymentProviderXDoc.Save(paymentProviderXML);

					var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

					var author = new User(0);

					var uwbsPaymentProviderSectionDoc = Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

					var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

					if (uwbsPaymentProviderSectionDoc != null)
					{
						var payPalDoc = Document.MakeNew("PayPal", dtuwbsPaymentProvider, author, uwbsPaymentProviderSectionDoc.Id);
						payPalDoc.SetProperty("title", "PayPal");
						payPalDoc.SetProperty("description", "PayPal Payment Provider for uWebshop");

						payPalDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());
						payPalDoc.SetProperty("dllName", "uWebshop.Payment.PayPal");

						payPalDoc.Save();

						BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "PayPal Installed!", "PayPal config added and node created");
					}
				}
			}

			var payPalTestWebConfig = ConfigurationManager.AppSettings["PayPalTestMode"];

			if (string.IsNullOrEmpty(payPalTestWebConfig))
			{
				try
				{
					ConfigurationManager.AppSettings.Add("PayPalTestMode", "true");
				}
				catch
				{
					BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Write to Web.config failed!", "Add: key='PayPalTestMode' with value='true' to web.config AppSettings");
				}
			}
		}
	}
}