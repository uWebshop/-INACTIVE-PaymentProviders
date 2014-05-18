using System;
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
	public partial class SagePayInstaller : UserControl
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

			var vendorName = "#YOUR VENDORNAME#";

			if (!string.IsNullOrEmpty(txtSagePayVendorName.Text))
			{
				vendorName = txtSagePayVendorName.Text;
			}

			var paymentProviderXML = HttpContext.Current.Server.MapPath(configfile);

			if (paymentProviderXML != null)
			{
				var paymentProviderXDoc = XDocument.Load(paymentProviderXML);

				if (paymentProviderXDoc.Descendants("provider").Any(x =>
					{
						var xAttribute = x.Attribute("title");
						return xAttribute != null && xAttribute.Value == "SagePay";
					}))
				{
					BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "SagePay config", "SagePay config already created");
				}
				else
				{
					//<provider title="SagePay">
					//	<VendorName>uwebshop</VendorName>
					//	<DirectUrl>https://test.sagepay.com/Simulator/VSPDirectGateway.asp</DirectUrl>
					//	<DirectTestURL>https://test.sagepay.com/Simulator/VSPDirectGateway.asp</DirectTestURL>
					//  </provider> 


					var paymentNode = new XElement("provider", new XAttribute("title", "SagePay"), new XElement("VendorName", vendorName));

					paymentProviderXDoc.Descendants("providers").FirstOrDefault().Add(paymentNode);

					paymentProviderXDoc.Save(paymentProviderXML);

					var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

					var author = new User(0);

					var uwbsPaymentProviderSectionDoc = Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

					var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

					if (uwbsPaymentProviderSectionDoc != null)
					{
						var providerDoc = Document.MakeNew("SagePay", dtuwbsPaymentProvider, author, uwbsPaymentProviderSectionDoc.Id);
						providerDoc.SetProperty("title", "SagePay");
						providerDoc.SetProperty("description", "SagePay Payment Provider for uWebshop");

						providerDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());

						providerDoc.Save();

						BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "SagePay Installed!", "SagePay config added and nodes created");
					}
				}
			}
		}
	}
}