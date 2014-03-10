using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Buckaroo
{
	public class BuckarooPaymentRequestHandler : BuckarooPaymentBase, IPaymentRequestHandler
	{
		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);

			var liveUrl = "https://checkout.buckaroo.nl/nvp/";
			var testUrl = "https://testcheckout.buckaroo.nl/nvp/";

			var configLiveUrl = paymentProvider.GetSetting("Url");
			var configTestUrl = paymentProvider.GetSetting("testUrl");

			var lowercaseUrls = paymentProvider.GetSetting("LowercaseUrls");
			var trailingSlash = paymentProvider.GetSetting("AddTrailingSlash");

			var lowercase = false;
			var addslash = false;

			if (!string.IsNullOrEmpty(lowercaseUrls))
			{
				if (lowercaseUrls == "true" || lowercaseUrls == "1" || lowercaseUrls == "on")
				{
					lowercase = true;
				}
			}

			if (!string.IsNullOrEmpty(trailingSlash))
			{
				if (trailingSlash == "true" || trailingSlash == "1" || trailingSlash == "on")
				{
					addslash = true;
				}
			}

			if(!string.IsNullOrEmpty(configLiveUrl))
			{
				liveUrl = configLiveUrl;
			}
			if(!string.IsNullOrEmpty(configTestUrl))
			{
				testUrl = configTestUrl;
			}
			
			var url = paymentProvider.TestMode ? testUrl : liveUrl;


			var providerSettingsXML = paymentProvider.GetSettingsXML();

			var service = orderInfo.PaymentInfo.MethodId;

			string issuer = null;

			var issuerElement = providerSettingsXML.Descendants().FirstOrDefault(x => x.Value.ToLowerInvariant() == orderInfo.PaymentInfo.MethodId.ToLowerInvariant());

			if (issuerElement != null)
			{
				issuer = issuerElement.Value;

				if (issuerElement.Name.LocalName.ToLowerInvariant() != "issuer")
				{
					var firstOrDefault = issuerElement.AncestorsAndSelf("Service").FirstOrDefault();
					if (firstOrDefault != null)
					{
						service = firstOrDefault.Attribute("name").Value;
					}
				}
			}

			var reportUrl = paymentProvider.ReportUrl();
			if (lowercase)
			{
				reportUrl = reportUrl.ToLowerInvariant();
			}

			if (!reportUrl.EndsWith("/") && addslash)
			{
				reportUrl = reportUrl + "/";
			}

			var Brq_amount = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:0.0}", orderInfo.ChargedAmountInCents/100);
			var Brq_currency = orderInfo.StoreInfo.Store.CurrencyCultureSymbol;
			var Brq_invoicenumber = orderInfo.OrderNumber;
			var Brq_payment_method = service;
			var Brq_return = reportUrl;
			var Brq_returncancel = reportUrl;
			var Brq_returnerror = reportUrl;
			var Brq_returnreject = reportUrl;
			var Add_transactionReference = orderInfo.UniqueOrderId.ToString();

			string IssuersServiceKeyName = null;
			string IssuerServiceKeyValue = null;
			string IssuerActionKeyName = null;
			string IssuerActionKeyValue = null;

			if (!string.IsNullOrEmpty(issuer))
			{
				IssuersServiceKeyName = string.Format("brq_service_{0}_issuer", service);
				IssuerServiceKeyValue = issuer;
				IssuerActionKeyName = string.Format("brq_service_{0}_action", service);
				IssuerActionKeyValue = "Pay";
			}

			var Brq_websitekey = paymentProvider.GetSetting("Websitekey");
			var SecretKey = paymentProvider.GetSetting("SecretKey");

			var stringToHash =  "add_transactionReference=" + Add_transactionReference +
								"brq_amount=" + Brq_amount +
			                   "brq_currency=" + Brq_currency +
			                   "brq_invoicenumber=" + Brq_invoicenumber +
			                   "brq_payment_method=" + Brq_payment_method +
			                   "brq_return=" + Brq_return +
			                   "brq_returncancel=" + Brq_returncancel +
			                   "brq_returnerror=" + Brq_returnerror +
			                   "brq_returnreject=" + Brq_returnreject +
			                   "brq_websitekey=" + Brq_websitekey +
			                   SecretKey;
			if (!string.IsNullOrEmpty(issuer))
			{

				stringToHash = "add_transactionReference=" + Add_transactionReference + 
								"brq_amount=" + Brq_amount +
							   "brq_currency=" + Brq_currency +
							   "brq_invoicenumber=" + Brq_invoicenumber +
							   "brq_payment_method=" + Brq_payment_method +
							   "brq_return=" + Brq_return +
							   "brq_returncancel=" + Brq_returncancel +
							   "brq_returnerror=" + Brq_returnerror +
							   "brq_returnreject=" + Brq_returnreject +
							   IssuerActionKeyName + "=" + IssuerActionKeyValue +
							   IssuersServiceKeyName + "=" + IssuerServiceKeyValue +
							   "brq_websitekey=" + Brq_websitekey +
							   SecretKey;
			}

			var Brq_signature = CreateHash(stringToHash);
			
			var request = new PaymentRequest();
			request.Parameters.Add("add_transactionReference", Add_transactionReference);
			request.Parameters.Add("brq_amount", Brq_amount);
			request.Parameters.Add("brq_currency", Brq_currency);
			request.Parameters.Add("brq_invoicenumber", Brq_invoicenumber);
			request.Parameters.Add("brq_payment_method", Brq_payment_method);
			request.Parameters.Add("brq_return", Brq_return);
			request.Parameters.Add("brq_returncancel", Brq_returncancel);
			request.Parameters.Add("brq_returnerror", Brq_returnerror);
			request.Parameters.Add("brq_returnreject", Brq_returnreject);

			if (!string.IsNullOrEmpty(issuer))
			{
				request.Parameters.Add(IssuerActionKeyName, IssuerActionKeyValue);

				request.Parameters.Add(IssuersServiceKeyName, IssuerServiceKeyValue);

			}
			
			request.Parameters.Add("brq_websitekey", Brq_websitekey);
			request.Parameters.Add("brq_signature", Brq_signature);


			var uri = new Uri(url);
			var webrequest = WebRequest.Create(uri);
			var encoding = new UTF8Encoding();
			var requestData = encoding.GetBytes(request.ParametersAsString);

			webrequest.ContentType = "application/x-www-form-urlencoded";

			webrequest.Method = "POST";
			webrequest.ContentLength = requestData.Length;

			using (var stream = webrequest.GetRequestStream())
			{
				stream.Write(requestData, 0, requestData.Length);
			}

			using (var response = webrequest.GetResponse())
			{
				var stream = response.GetResponseStream();
				if (stream == null) throw new Exception("No response from POST request to " + url);
				using (var reader = new StreamReader(stream, Encoding.ASCII))
				{
					var value = reader.ReadToEnd();

					var result = HttpUtility.ParseQueryString(value);

					orderInfo.PaymentInfo.Url = result["brq_redirecturl"];
				}

			}
			
			PaymentProviderHelper.SetTransactionId(orderInfo, Add_transactionReference);
			
			return request;
		}

		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}

		public string CreateHash(string hashString)
		{
			var builder = new StringBuilder();

			var sha1Provider = new SHA1CryptoServiceProvider();

			//convert input to byte array
			byte[] messageArray = Encoding.UTF8.GetBytes(hashString);

			//calculate hash over byte array
			byte[] hash1 = sha1Provider.ComputeHash(messageArray);

			//convert byte array to string by printing each hex value.
			foreach (byte b in hash1)
			{
				builder.Append(b.ToString("x2"));
			}
			//get the result from the string builder object.
			return builder.ToString();
		}
	}
}