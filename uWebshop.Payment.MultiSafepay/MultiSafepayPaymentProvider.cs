using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using uWebshop.API;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;
using umbraco.BusinessLogic;
using uWebshop.Domain.Helpers;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.MultiSafePay
{
	public class MultiSafePayPaymentProvider : MultiSafePayPaymentBase, IPaymentProvider
	{
		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.ServerPost;
		}

		public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
		{
			var basket = Basket.GetBasket();

			var paymentProvider = PaymentProvider.GetPaymentProvider(id);
			if (paymentProvider == null)
			{
				throw new Exception("PaymentProvider with id " + id + " not found.");
			}

			var customerCountryCode = API.Store.GetStore().CountryCode;

			if (basket != null && !string.IsNullOrEmpty(basket.Customer.CountryCode))
			{
				customerCountryCode = basket.Customer.CountryCode;
			}

			var accountId = paymentProvider.GetSetting("accountId");
			var siteId = paymentProvider.GetSetting("siteId");
			var siteSecureId = paymentProvider.GetSetting("siteSecureId");

			var url = paymentProvider.GetSetting("Url");
			var testUrl = paymentProvider.GetSetting("testUrl");

			var gatewaysRequest = new GatewayRequest
			                      {
				                      AccountId = long.Parse(accountId), 
									  Country = customerCountryCode,
									  SiteId = int.Parse(siteId),
									  SiteSecureId = int.Parse(siteSecureId)
			                      };

			var apiURL = paymentProvider.TestMode ? testUrl : url;

			var httpWebRequest = (HttpWebRequest) WebRequest.Create(apiURL);
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentLength = Encoding.UTF8.GetByteCount(gatewaysRequest.GetXml());
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";

			var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
			streamWriter.Write(gatewaysRequest.GetXml());
			streamWriter.Close();

			var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
			
			var streamReader = new StreamReader(httpWebResponse.GetResponseStream());
			var xmlDoc = XDocument.Parse(streamReader.ReadToEnd());

			Log.Instance.LogDebug("MultiSafePay GetAllPaymentMethods MultiSafePay XML Answer: " + HttpUtility.HtmlEncode(xmlDoc.ToString(SaveOptions.None)));

			var gateways = xmlDoc.Descendants("gateway");

			// example answer from MultiSafepay
			//<?xml version="1.0" encoding="UTF-8"?>
			//<gateways result="ok">
			//    <gateways>
			//      <gateway>
			//        <id>IDEAL</id>
			//        <description>iDeal</description>
			//      </gateway>
			//      <gateway>
			//        <id> MASTERCARD</id>
			//        <description>Visa via Multipay</description>
			//      </gateway>
			//      <gateway>
			//        <id> BANKTRANS</id>
			//       <description> Bank Transfer</description>
			//      </gateway>
			//      <gateway>
			//        <id> VISA</id>
			//        <description> Visa CreditCardsdescription>
			//      </gateway>
			//    </gateways>
			//</gateways>
			//int paymentImageId;
			//int.TryParse(umbraco.library.GetDictionaryItem(gateway.Element("id").Value + "LogoId"), out paymentImageId);
			return gateways.Select(gateway => new PaymentProviderMethod {Id = gateway.Element("id").Value, Description = gateway.Element("description").Value, Title = gateway.Element("description").Value, Name = gateway.Element("description").Value, ProviderName = GetName()}).ToList();
		}
	}
}