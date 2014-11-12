using System.Collections.Generic;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using System.Xml.Linq;

namespace uWebshop.Payment.EasyIdeal
{
	/// <summary>
	/// DLL naming must be: uWebshop.Payment.PaymentProviderName
	/// </summary>
    public class EasyIdealPaymentProvider : EasyIdealPaymentBase, IPaymentProvider
	{
		
		public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id = 0)
		{
            var paymentProvider = PaymentProvider.GetPaymentProvider(id);

            var merchantId = paymentProvider.GetSetting("merchantId");

            var merchantKey = paymentProvider.GetSetting("merchantKey");

            var merchantSecret = paymentProvider.GetSetting("merchantSecret");

            var url = paymentProvider.GetSetting("url");

            var xmlRequest = getXML(IDEAL_GETBANKS, new SortedList<string, string>(), merchantId, merchantKey, merchantSecret);

            XDocument xmlResponse = XDocument.Parse(postXML(xmlRequest, url));

            var responseStatus = xmlResponse.Element("Response").Element("Status").FirstNode.ToString();

            var paymentProviderMethodList = new List<PaymentProviderMethod> { };

            if (responseStatus == "OK")
            {
                foreach (XElement bank in xmlResponse.Descendants("Bank"))
                {
                    paymentProviderMethodList.Add(new PaymentProviderMethod
                                                {
                                                    Id = bank.Element("Id").Value, 
                                                    Description = bank.Element("Name").Value,
                                                    Title = bank.Element("Name").Value, 
                                                    Name = "Easy iDeal",
                                                    ProviderName = GetName(),
                                                    ImageId = 0
                                                });
                }
            }

			return paymentProviderMethodList;
		}
		
		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.QueryString;
		}
	}
}