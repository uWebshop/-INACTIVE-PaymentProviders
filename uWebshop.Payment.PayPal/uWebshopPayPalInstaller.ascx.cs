using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;
using umbraco;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.ContentTypes;

namespace uWebshop.Payment
{
	public partial class PayPalInstaller : UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}


		protected void installConfig(object sender, EventArgs e)
		{
            var configfile = PaymentConfigHelper.GetPaymentProviderConfigXml();
            var configfilePath = PaymentConfigHelper.GetPaymentProviderConfig();

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

		    if (configfile.Descendants("provider").Any(x =>
		    {
		        var xAttribute = x.Attribute("title");
		        return xAttribute != null && xAttribute.Value == "PayPal";
		    }))
		    {
		        BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "Paypal config",
		            "PayPal config already created");
		    }
		    else
		    {
		        var createPayPalNode = new XElement("provider", new XAttribute("title", "PayPal"),
		            new XElement("accountId", payPalAccountId));

		        if (configfile.Descendants("providers").Any())
		        {
		            configfile.Descendants("providers").First().Add(createPayPalNode);

		            var paymentProviderPath = HttpContext.Current.Server.MapPath(configfilePath);

		            configfile.Save(paymentProviderPath);
		        }

		        var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

		        var author = new User(0);

		        var uwbsPaymentProviderSectionDoc =
		            Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

		        var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

		        if (uwbsPaymentProviderSectionDoc != null)
		        {
		            var payPalDoc = Document.MakeNew("PayPal", dtuwbsPaymentProvider, author,
		                uwbsPaymentProviderSectionDoc.Id);
		            payPalDoc.SetProperty("title", "PayPal");
		            payPalDoc.SetProperty("description", "PayPal Payment Provider for uWebshop");

		            payPalDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());

		            payPalDoc.Save();

		            BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "PayPal Installed!",
		                "PayPal config added and node created");
		        }

		    }
		}
	}
}