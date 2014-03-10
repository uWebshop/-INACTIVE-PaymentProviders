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

            BuckarooRequestParameters requestParams = new BuckarooRequestParameters(paymentProvider.GetSetting("SecretKey"));
            requestParams.Amount = orderInfo.ChargedAmountInCents / 100;
            requestParams.Culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;	
            requestParams.Currency = orderInfo.StoreInfo.Store.CurrencyCultureSymbol;
		    requestParams.InvoiceNumber = orderInfo.OrderNumber;
		    requestParams.PaymentMethod = service;
		    requestParams.ReturnUrl = reportUrl;
		    requestParams.ReturnCancelUrl = reportUrl;
		    requestParams.ReturnErrorUrl = reportUrl;
		    requestParams.ReturnReject = reportUrl;
		    requestParams.WebsiteKey = paymentProvider.GetSetting("Websitekey");

		    var transactionId = orderInfo.UniqueOrderId.ToString();		   
		    requestParams.TransactionId = transactionId;
			
			
			
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

                requestParams.AddCustomParameter(IssuersServiceKeyName, IssuerServiceKeyValue);
                requestParams.AddCustomParameter(IssuerActionKeyName, IssuerActionKeyValue);
			}

		    if (service.Equals("transfer"))
		    {
		        requestParams.AddCustomParameter("brq_service_transfer_customeremail", orderInfo.CustomerEmail);
                requestParams.AddCustomParameter("brq_service_transfer_customerfirstname", orderInfo.CustomerFirstName);
                requestParams.AddCustomParameter("brq_service_transfer_customerlastname", orderInfo.CustomerLastName);
		    }

		    if (service.Equals("onlinegiro"))
		    {
                requestParams.AddCustomParameter("brq_service_onlinegiro_customergender", "9");
                requestParams.AddCustomParameter("brq_service_onlinegiro_customeremail", orderInfo.CustomerEmail);
                requestParams.AddCustomParameter("brq_service_onlinegiro_customerfirstname", orderInfo.CustomerFirstName);
                requestParams.AddCustomParameter("brq_service_onlinegiro_customerlastname", orderInfo.CustomerLastName);
		    }

			requestParams.Sign();

			
			var request = new PaymentRequest();
            //request.Parameters.Add("add_transactionReference", Add_transactionReference);
            //request.Parameters.Add("brq_amount", Brq_amount);
            //request.Parameters.Add("brq_currency", Brq_currency);
            //request.Parameters.Add("brq_invoicenumber", Brq_invoicenumber);
            //request.Parameters.Add("brq_payment_method", Brq_payment_method);
            //request.Parameters.Add("brq_return", Brq_return);
            //request.Parameters.Add("brq_returncancel", Brq_returncancel);
            //request.Parameters.Add("brq_returnerror", Brq_returnerror);
            //request.Parameters.Add("brq_returnreject", Brq_returnreject);
            

            //if (!string.IsNullOrEmpty(issuer))
            //{
            //    request.Parameters.Add(IssuerActionKeyName, IssuerActionKeyValue);

            //    request.Parameters.Add(IssuersServiceKeyName, IssuerServiceKeyValue);

            //}
   
			
            //request.Parameters.Add("brq_websitekey", Brq_websitekey);
            //request.Parameters.Add("brq_signature", Brq_signature);
		    request.Parameters = requestParams.GetParameters();


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

				    if (service == "onlinegiro" || service == "transfer")
				    {
				        orderInfo.PaymentInfo.Url = reportUrl + "?" + value;
				    }
				}

			}
			
			PaymentProviderHelper.SetTransactionId(orderInfo, transactionId);
			
			return request;
		}

		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}		
	}
}