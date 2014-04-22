using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using umbraco;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.Ogone
{
	public class OgonePaymentRequestHandler : OgonePaymentBase, IPaymentRequestHandler
	{
		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);
			
			var reportUrl = paymentProvider.ReportUrl();
			
			var pspId = paymentProvider.GetSetting("PSPID");
			var shaInSignature = paymentProvider.GetSetting("SHAInSignature");
			var secureHashAlgorithm = paymentProvider.GetSetting("SecureHashAlgorithm");

            var liveUrl = "https://secure.ogone.com/ncol/prod/orderstandard.asp";
            var testUrl = "https://secure.ogone.com/ncol/test/orderstandard.asp";

            var configLiveUrl = paymentProvider.GetSetting("Url");
            var configTestUrl = paymentProvider.GetSetting("testUrl");

            if (!string.IsNullOrEmpty(configLiveUrl))
            {
                liveUrl = configLiveUrl;
            }
            if (!string.IsNullOrEmpty(configTestUrl))
            {
                testUrl = configTestUrl;
            }

            var url = paymentProvider.TestMode ? testUrl : liveUrl;

			var request = new PaymentRequest();
			request.Parameters.Add("PSPID", pspId);
			request.Parameters.Add("ORDERID", orderInfo.OrderNumber);

			var amount = orderInfo.ChargedAmountInCents;

			request.Parameters.Add("AMOUNT", amount.ToString());
			request.Parameters.Add("CURRENCY", orderInfo.StoreInfo.Store.CurrencyCultureSymbol);
			request.Parameters.Add("LANGUAGE", orderInfo.StoreInfo.LanguageCode + "_" + orderInfo.StoreInfo.CountryCode);
			request.Parameters.Add("EMAIL", OrderHelper.CustomerInformationValue(orderInfo, "customerEmail"));

			request.Parameters.Add("ACCEPTURL", reportUrl);
			request.Parameters.Add("DECLINEURL", reportUrl);
			request.Parameters.Add("EXCEPTIONURL", reportUrl);
			request.Parameters.Add("CANCELURL", reportUrl);

			//action => 'Normal Authorization' (with operation => 'SAL' (default))
			//action => 'Authorization Only' (with operation => 'RES' (default)) then action => 'Post Authorization' (with operation => 'SAS' (default))
			request.Parameters.Add("OPERATION", "SAL");

			var transactionId = orderInfo.UniqueOrderId.ToString().Replace("-", "");

			request.Parameters.Add("PARAMPLUS", string.Format("TransactionId={0}", transactionId));


			if (orderInfo.PaymentInfo.MethodId.Contains('-'))
			{
                var splitarray = orderInfo.PaymentInfo.MethodId.Split('-');

                var pm = splitarray[0];
                var brand = splitarray[1];

                request.Parameters.Add("PM", pm);
				request.Parameters.Add("BRAND", brand);
			}
			else
			{
				request.Parameters.Add("PM", orderInfo.PaymentInfo.MethodTitle);
			}

			var stringToHash = string.Empty;
			var sortedParameters = request.Parameters.OrderBy(x => x.Key);

			foreach (var parameter in sortedParameters)
			{
				stringToHash += string.Format("{0}={1}{2}", parameter.Key, parameter.Value, shaInSignature);
			}

			switch (secureHashAlgorithm)
			{
				case "SHA1":
					request.Parameters.Add("SHASIGN", GetSHA1Hash(stringToHash).ToUpper());
					break;
				case "SHA256":
					request.Parameters.Add("SHASIGN", GetSHA256Hash(stringToHash).ToUpper());
					break;
				case "SHA512":
					request.Parameters.Add("SHASIGN", GetSHA512Hash(stringToHash).ToUpper());
					break;
			}

		    request.PaymentUrlBase = url;

			orderInfo.PaymentInfo.Url = request.PaymentUrl;
			orderInfo.PaymentInfo.Parameters = request.ParametersAsString;
			PaymentProviderHelper.SetTransactionId(orderInfo, transactionId);

			return request;
		}

		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}


		// Create an sha1 hash string of this string
		public static string GetSHA1Hash(string str)
		{
			var x = new SHA1Managed();

			byte[] data = Encoding.ASCII.GetBytes(str);

			data = x.ComputeHash(data);

			return BitConverter.ToString(data).Replace("-", "").ToLower();
		}

		// Create an sha256 hash string of this string
		public static string GetSHA256Hash(string str)
		{
			var x = new SHA256Managed();

			byte[] data = Encoding.ASCII.GetBytes(str);

			data = x.ComputeHash(data);

			return BitConverter.ToString(data).Replace("-", "").ToLower();
		}

		// Create an sha512 hash string of this string
		public static string GetSHA512Hash(string str)
		{
			var x = new SHA512Managed();

			byte[] data = Encoding.ASCII.GetBytes(str);

			data = x.ComputeHash(data);

			return BitConverter.ToString(data).Replace("-", "").ToLower();
		}
	}
}