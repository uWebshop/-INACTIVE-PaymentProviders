using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using umbraco;
using umbraco.BusinessLogic;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.Sisow
{
	public class Sisow : SisowPaymentBase, IPaymentProvider
	{
		
		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.QueryString;
		}

		public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
		{
			var paymentMethods = new List<PaymentProviderMethod>();
			var paymentProvider = PaymentProvider.GetPaymentProvider(id);
            
			var directoryLiveUrl = "https://www.sisow.nl/Sisow/iDeal/RestHandler.ashx/DirectoryRequest";
            var directoryTestUrl = "https://www.sisow.nl/Sisow/iDeal/RestHandler.ashx/DirectoryRequest?test=true";

            var configDirectoryLiveUrl = paymentProvider.GetSetting("DirectoryRequestUrl");
            var configDirectoryTestUrl = paymentProvider.GetSetting("DirectoryRequestTestUrl");

            if (!string.IsNullOrEmpty(configDirectoryLiveUrl))
            {
                directoryLiveUrl = configDirectoryLiveUrl;
            }
            if (!string.IsNullOrEmpty(configDirectoryTestUrl))
            {
                directoryTestUrl = configDirectoryTestUrl;
            }

            var apiURL = paymentProvider.TestMode ? directoryTestUrl : directoryLiveUrl;

			var issuerRequest = HttpGet(apiURL);

			XNamespace ns = "https://www.sisow.nl/Sisow/REST";
			var issuerXml = XDocument.Parse(issuerRequest);
            
			foreach (var issuer in issuerXml.Descendants(ns + "issuer"))
			{
				var issuerId = issuer.Element(ns + "issuerid").Value;
				var issuerName = issuer.Element(ns + "issuername").Value;

				var paymentImageId = 0;

				var logoDictionaryItem = library.GetDictionaryItem(issuerId + "LogoId");

				if (string.IsNullOrEmpty(logoDictionaryItem))
				{
					int.TryParse(library.GetDictionaryItem(issuerId + "LogoId"), out paymentImageId);
				}

				paymentMethods.Add(new PaymentProviderMethod
				                   {
					                   Id = issuerId, 
									   Description = issuerName, 
									   Title = issuerName, 
									   Name = issuerName, 
									   ProviderName = GetName(), 
									   ImageId = paymentImageId
				                   });
			}

			return paymentMethods;
		}

		
	}
}