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
	public partial class SisowInstaller : UserControl
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

			var merchandId = "#YOUR SISOW MERCHANT ID#";
			var merchantKey = "#YOUR SISOW MERCHANT KEY#";

			if (!string.IsNullOrEmpty(txtId.Text))
			{
				merchandId = txtId.Text;
			}

			if (!string.IsNullOrEmpty(txtKey.Text))
			{
				merchantKey = txtKey.Text;
			}

			var paymentProviderXml = HttpContext.Current.Server.MapPath(configfile);

			if (paymentProviderXml != null)
			{
				var paymentProviderXDoc = XDocument.Load(paymentProviderXml);

				if (paymentProviderXDoc.Descendants("provider").Any(x =>
					{
						var xAttribute = x.Attribute("title");
						return xAttribute != null && xAttribute.Value == "Sisow";
					}))
				{
					BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "Sisow config", "Sisow config already created");
				}
				else
				{
					//       <provider title="Sisow">
					//  <merchantid>#YOUR SISOW MERCHANT ID#</merchantid>
					//      <merchantkey>#YOUR SISOW MERCHANT KEY#</merchantkey>
					//  <DirectoryRequestUrl>https://www.sisow.nl/Sisow/iDeal/RestHandler.ashx/DirectoryRequest</DirectoryRequestUrl>
					//  <DirectoryRequestTestUrl>https://www.sisow.nl/Sisow/iDeal/RestHandler.ashx/DirectoryRequest?test=true</DirectoryRequestTestUrl>
					//      <TransactionRequestUrl>https://www.sisow.nl/Sisow/iDeal/RestHandler.ashx/TransactionRequest</TransactionRequestUrl>
					//</provider> 


					var ProviderNode = new XElement("provider", new XAttribute("title", "Sisow"), new XElement("merchantid", merchandId), new XElement("merchantkey", merchantKey), new XElement("DirectoryRequestUrl", "https://www.sisow.nl/Sisow/iDeal/RestHandler.ashx/DirectoryRequest"), new XElement("DirectoryRequestTestUrl", "https://www.sisow.nl/Sisow/iDeal/RestHandler.ashx/DirectoryRequest?test=true"), new XElement("TransactionRequestUrl", "https://www.sisow.nl/Sisow/iDeal/RestHandler.ashx/TransactionRequest"));

					paymentProviderXDoc.Descendants("providers").FirstOrDefault().Add(ProviderNode);

					paymentProviderXDoc.Save(paymentProviderXml);

					var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

					var author = new User(0);

					var uwbsPaymentProviderSectionDoc = Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

					var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

					if (uwbsPaymentProviderSectionDoc != null)
					{
						var providerDoc = Document.MakeNew("Sisow", dtuwbsPaymentProvider, author, uwbsPaymentProviderSectionDoc.Id);
						providerDoc.SetProperty("title", "Sisow");
						providerDoc.SetProperty("description", "Sisow Payment Provider for uWebshop");

						providerDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());
						providerDoc.SetProperty("dllName", "uWebshop.Payment.Sisow");

						providerDoc.Save();

						BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "Sisow Installed!", "Sisow config added and node created");
					}
				}
			}
		}
	}
}