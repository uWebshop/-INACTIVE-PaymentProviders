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
	public partial class OgoneInstaller : UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}


		protected void InstallOgoneConfig(object sender, EventArgs e)
		{
			var configfile = PaymentConfigHelper.GetPaymentProviderConfig();

			if (configfile == null)
			{
				BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Error!", "PaymentProviderConfig not found!");
				return;
			}

			var ogoneAccountId = "#YOUR OGONE PSPID#";
			var ogoneSHASignature = "#YOUR SHA SIGNATURE#";

			if (!string.IsNullOrEmpty(txtOgonePSPID.Text))
			{
				ogoneAccountId = txtOgonePSPID.Text;
			}

			if (!string.IsNullOrEmpty(txtSHA.Text))
			{
				ogoneSHASignature = txtSHA.Text;
			}

			var paymentProviderXML = HttpContext.Current.Server.MapPath(configfile);

			if (paymentProviderXML != null)
			{
				var paymentProviderXDoc = XDocument.Load(paymentProviderXML);

				if (paymentProviderXDoc.Descendants("provider").Any(x =>
					{
						var xAttribute = x.Attribute("title");
						return xAttribute != null && xAttribute.Value == "Ogone";
					}))
				{
					BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "Ogone config", "Ogone config already created");
				}
				else
				{
					//         <provider title="Ogone">
					//  <PSPID>#YOUR PSID#</PSPID>
					//  <SecureHashAlgorithm>SHA256</SecureHashAlgorithm>
					//  <SHAInSignature>#YOUR SHA SIGNATURE</SHAInSignature>
					//      <url>https://secure.ogone.com/ncol/prod/orderstandard.asp</url>
					//  <testURL>https://secure.ogone.com/ncol/test/orderstandard.asp</testURL>
					//</provider> 


					var oGoneConfig = new XElement("provider", new XAttribute("title", "Ogone"), new XElement("PSPID", ogoneAccountId), new XElement("SecureHashAlgorithm", "SHA256"), new XElement("SHAInSignature", ogoneSHASignature));

					paymentProviderXDoc.Descendants("providers").FirstOrDefault().Add(oGoneConfig);

					paymentProviderXDoc.Save(paymentProviderXML);

					var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

					var author = new User(0);

					var uwbsPaymentProviderSectionDoc = Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

					var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

					if (uwbsPaymentProviderSectionDoc != null)
					{
						var providerDoc = Document.MakeNew("Ogone", dtuwbsPaymentProvider, author, uwbsPaymentProviderSectionDoc.Id);
						providerDoc.SetProperty("title", "Ogone");
						providerDoc.SetProperty("description", "Ogone Payment Provider for uWebshop");
                        providerDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());
						providerDoc.Save();

                        //var dtuwbsPaymentProviderMethod = DocumentType.GetByAlias(PaymentProviderMethod.NodeAlias);

                        //var methodDocMasterCard = Document.MakeNew("CreditCardMasterCard", dtuwbsPaymentProviderMethod, author, providerDoc.Id);
                        //methodDocMasterCard.SetProperty("title", "CreditCard|MasterCard");
                        //methodDocMasterCard.SetProperty("description", "Mastercard Payment Method using Ogone");

                        //methodDocMasterCard.Save();

                        //var methodDocVisa = Document.MakeNew("CreditCardVisa", dtuwbsPaymentProviderMethod, author, providerDoc.Id);
                        //methodDocVisa.SetProperty("title", "CreditCard|Visa");
                        //methodDocVisa.SetProperty("description", "Visa Payment Method using Ogone");

                        //methodDocVisa.Save();


						BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "Ogone Installed!", "Ogone config added and nodes created");
					}
				}
			}
		}
	}
}