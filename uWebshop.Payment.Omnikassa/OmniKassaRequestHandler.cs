using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Omnikassa
{
	public class OmniKassaRequestHandler : OmnikassaPaymentBase, IPaymentRequestHandler
	{
		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);

			var reportUrl = paymentProvider.ReportUrl();

			//	<provider title="OmniKassa">
			//    <MerchantId>002020000000001</MerchantId>
			//    <CurrencyCode>978</CurrencyCode>
			//    <normalReturnUrl>http://www.hetanker.tv</normalReturnUrl>
			//    <KeyVersion>1</KeyVersion>
			//    <TestAmount>56</TestAmount>
			//  </provider>

			var merchantId = paymentProvider.GetSetting("MerchantId");
			var keyVersion = paymentProvider.GetSetting("KeyVersion");
			var currencyCode = paymentProvider.GetSetting("CurrencyCode");
			var testAmount = paymentProvider.GetSetting("testAmount");
            
            var liveUrl = "https://payment-webinit.omnikassa.rabobank.nl/paymentServlet";
		    var testUrl = "https://payment-webinit.simu.omnikassa.rabobank.nl/paymentServlet";
		    
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

            var liveForwardUrl = "https://payment-web.omnikassa.rabobank.nl/payment";
		    var testForwardUrl = "https://payment-web.simu.omnikassa.rabobank.nl/payment";

            var configForwardLiveUrl = paymentProvider.GetSetting("forwardUrl");
            var configForwardTestUrl = paymentProvider.GetSetting("testForwardUrl");

            if (!string.IsNullOrEmpty(configForwardLiveUrl))
            {
                liveForwardUrl = configForwardLiveUrl;
            }
            if (!string.IsNullOrEmpty(configForwardTestUrl))
            {
                testForwardUrl = configForwardTestUrl;
            }


			var securityKey = paymentProvider.GetSetting("SecurityKey");
			
			var postUrl = paymentProvider.TestMode ? testUrl : liveUrl;

			var forwardUrl = paymentProvider.TestMode ? testForwardUrl : liveForwardUrl;

			var amount = paymentProvider.TestMode ? testAmount : orderInfo.ChargedAmountInCents.ToString();

			var orderId = orderInfo.OrderNumber;

			if (orderId.Length > 32)
			{
				Log.Instance.LogError("OmniKassa: orderInfo.OrderNumber: Too Long, Max 32 Characters! orderInfo.OrderNumber: " + orderInfo.OrderNumber);
				orderId = orderInfo.OrderNumber.Substring(0, 31);
			}

			var transactionId = orderId + "x" + DateTime.Now.ToString("hhmmss");

			var rgx = new Regex("[^a-zA-Z0-9]");
			transactionId = rgx.Replace(transactionId, "");

			if (transactionId.Length > 35)
			{
				Log.Instance.LogError("OmniKassa: uniqueId (orderId + 'x' + DateTime.Now.ToString('hhmmss')): Too Long, Max 35 Characters! uniqueId: " + transactionId);
				transactionId = transactionId.Substring(0, 34);
			}
			
			if (reportUrl.Length > 512)
			{
				Log.Instance.LogError("OmniKassa: reportUrl: Too Long, Max 512 Characters! reportUrl: " + reportUrl);
			}

			// Data-veld samenstellen
			var data = string.Format("merchantId={0}", merchantId) + string.Format("|orderId={0}", orderId) + string.Format("|amount={0}", amount) + string.Format("|customerLanguage={0}", "NL") + string.Format("|keyVersion={0}", keyVersion) + string.Format("|currencyCode={0}", currencyCode) // + string.Format("|PaymentMeanBrandList={0}", "IDEAL")
			           + string.Format("|normalReturnUrl={0}", reportUrl) + string.Format("|automaticResponseUrl={0}", reportUrl) + string.Format("|transactionReference={0}", transactionId);

			// Seal-veld berekenen
			var sha256 = SHA256.Create();
			var hashValue = sha256.ComputeHash(new UTF8Encoding().GetBytes(data + securityKey));
			
			// POST data samenstellen
			var postData = new NameValueCollection {{"Data", data}, {"Seal", ByteArrayToHexString(hashValue)}, {"InterfaceVersion", "HP_1.0"}};

			
		
			//// Posten van data 
			byte[] response;
			using (var client = new WebClient())
				response = client.UploadValues(postUrl, postData);

			try
			{
				var responseData = Encoding.UTF8.GetString(response);
				var trimmedResponse = responseData.Trim();
				var matchedHiddenfield = Regex.Matches(trimmedResponse, "<input type=HIDDEN.+/>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				var postValueFromHiddenField = Regex.Matches(matchedHiddenfield[0].Value, "(?<=\\bvalue=\")[^\"]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				var redirectionDataField = string.Format("redirectionData={0}", postValueFromHiddenField[0].Value);

				PaymentProviderHelper.SetTransactionId(orderInfo, transactionId);
				orderInfo.PaymentInfo.Url = forwardUrl;
				orderInfo.PaymentInfo.Parameters = redirectionDataField;

				orderInfo.Save();

				var request = new PaymentRequest();

				return request;
			}
			catch
			{
				var responseResult = Encoding.UTF8.GetString(response);
				Log.Instance.LogError("Omnikassa: " + responseResult);
				throw new Exception("OmniKassa Issue Please Notice Shopowner");
			}
		}

		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}

		// Converteer een String naar Hexadecimale waarde
		public string ByteArrayToHexString(byte[] bytes)
		{
			var result = new StringBuilder(bytes.Length*2);
			const string hexAlphabet = "0123456789ABCDEF";

			foreach (var b in bytes)
			{
				result.Append(hexAlphabet[b >> 4]);
				result.Append(hexAlphabet[b & 0xF]);
			}

			return result.ToString();
		}
	}
}
