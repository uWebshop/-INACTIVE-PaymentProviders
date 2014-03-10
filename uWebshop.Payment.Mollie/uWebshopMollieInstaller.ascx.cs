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
	public partial class MollieInstaller : UserControl
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

			var molliePartnerId = "#YOUR Mollie PartnerId#";

			if (!string.IsNullOrEmpty(txtMolliePartnerId.Text))
			{
				molliePartnerId = txtMolliePartnerId.Text;
			}

			var paymentProviderXML = HttpContext.Current.Server.MapPath(configfile);

			if (paymentProviderXML == null)
			{
				return;
			}

			var paymentProviderXDoc = XDocument.Load(paymentProviderXML);

			if (!paymentProviderXDoc.Descendants("providers").Any())
			{
				BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "PaymentConfig", "PaymentConfig Providers RootNode Missing!");
				return;
			}

			if (paymentProviderXDoc.Descendants("provider").Any(x =>
				{
					var xAttribute = x.Attribute("title");
					return xAttribute != null && xAttribute.Value == "Mollie";
				}))
			{
				BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "Mollie config", "Mollie config already created");
				return;
			}

			// <provider title="Mollie">
			//  <PartnerId>#YOUR PartnerId#</PartnerId>
			//</provider> 


			var providerNode = new XElement("provider", new XAttribute("title", "Mollie"), new XElement("PartnerId", molliePartnerId));

			paymentProviderXDoc.Descendants("providers").FirstOrDefault().Add(providerNode);

			paymentProviderXDoc.Save(paymentProviderXML);

			var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

			var author = new User(0);

			var uwbsPaymentProviderSectionDoc = Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

			var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

			if (uwbsPaymentProviderSectionDoc == null)
			{
				BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "uWebshop config", "PaymentProvider DocumentType does not Exist");
				return;
			}

			var providerDoc = Document.MakeNew("Mollie", dtuwbsPaymentProvider, author, uwbsPaymentProviderSectionDoc.Id);
			providerDoc.SetProperty("title", "Mollie");
			providerDoc.SetProperty("description", "Mollie Payment Provider for uWebshop");

			providerDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());
			providerDoc.SetProperty("dllName", "uWebshop.Payment.Mollie");

			providerDoc.Save();

			BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "Mollie Installed!", "Mollie config added and nodes created");
		}
	}
}