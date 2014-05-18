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
	        var configfile = PaymentConfigHelper.GetPaymentProviderConfigXml();
	        var configfilePath = PaymentConfigHelper.GetPaymentProviderConfig();

	        if (configfile == null)
	        {
	            BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Error!",
	                "PaymentProviderConfig not found!");
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

            if (configfile.Descendants("provider").Any(x =>
	        {
	            var xAttribute = x.Attribute("title");
	            return xAttribute != null && xAttribute.Value == "Ogone";
	        }))
	        {
	            BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.info, "Ogone config",
	                "Ogone config already created");
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


	            var oGoneConfig = new XElement("provider", new XAttribute("title", "Ogone"),
	                new XElement("PSPID", ogoneAccountId), new XElement("SecureHashAlgorithm", "SHA256"),
	                new XElement("SHAInSignature", ogoneSHASignature));

                configfile.Descendants("providers").First().Add(oGoneConfig);

                var servicesNode = new XElement("Services");
                oGoneConfig.Add(servicesNode);

                #region creditcards
                var serviceCreditCardNode = new XElement("Service", new XAttribute("name", "CreditCard"));
                servicesNode.Add(serviceCreditCardNode);

                var issuersNode = new XElement("Issuers");
                serviceCreditCardNode.Add(issuersNode);

                var issuerNode = new XElement("Issuer");
                issuersNode.Add(issuerNode);

                var codeNodeMasterCard = new XElement("Code", "MasterCard");
                issuerNode.Add(codeNodeMasterCard);

                var nameNodeMasterCard = new XElement("Name", "MasterCard");
                issuerNode.Add(nameNodeMasterCard);

                var issuer2Node = new XElement("Issuer");
                issuersNode.Add(issuer2Node);

                var codeNodeVisa = new XElement("Code", "VISA");
                issuer2Node.Add(codeNodeVisa);

                var nameNodeVisa = new XElement("Name", "VISA");
                issuer2Node.Add(nameNodeVisa);
                #endregion

                #region ideal
                var serviceiDealNode = new XElement("Service", new XAttribute("name", "iDEAL"));
                servicesNode.Add(serviceiDealNode);

                var issuersiDealNode = new XElement("Issuers");
                serviceiDealNode.Add(issuersiDealNode);

                var issueriDealNode = new XElement("Issuer");
                issuersiDealNode.Add(issueriDealNode);

                var codeNodeiDeal = new XElement("Code", "iDEAL");
                issueriDealNode.Add(codeNodeiDeal);

                var nameNodeiDeal = new XElement("Name", "iDeal");
                issueriDealNode.Add(nameNodeiDeal);
                #endregion

                var paymentProviderPath = HttpContext.Current.Server.MapPath(configfilePath);
                configfile.Save(paymentProviderPath);

	            var dtuwbsPaymentProviderSection = DocumentType.GetByAlias(PaymentProviderSectionContentType.NodeAlias);

	            var author = new User(0);

	            var uwbsPaymentProviderSectionDoc =
	                Document.GetDocumentsOfDocumentType(dtuwbsPaymentProviderSection.Id).FirstOrDefault();

	            var dtuwbsPaymentProvider = DocumentType.GetByAlias(PaymentProvider.NodeAlias);

	            if (uwbsPaymentProviderSectionDoc != null)
	            {
	                var providerDoc = Document.MakeNew("Ogone", dtuwbsPaymentProvider, author,
	                    uwbsPaymentProviderSectionDoc.Id);
	                providerDoc.SetProperty("title", "Ogone");
	                providerDoc.SetProperty("description", "Ogone Payment Provider for uWebshop");
	                providerDoc.SetProperty("type", PaymentProviderType.OnlinePayment.ToString());
	                providerDoc.Save();

	                BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.success, "Ogone Installed!",
	                    "Ogone config added and nodes created");
	            }
	        }
	    }
	}
}