using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using uWebshop.Common;
using uWebshop.DataAccess;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using umbraco;
// do not remove the umbraco.interfaces using below
using umbraco.BusinessLogic;
using umbraco.interfaces;
using umbraco.NodeFactory;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.Sisow
{
	public class SisowPaymentRequestHandler : SisowPaymentBase, IPaymentRequestHandler
	{
		private readonly IHttpRequestSender _requestSender;

		public SisowPaymentRequestHandler()
		{
			_requestSender = new HttpRequestSender();
		}

		public SisowPaymentRequestHandler(IHttpRequestSender requestSender = null)
		{
			_requestSender = requestSender ?? new HttpRequestSender();
		}
		
		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);
			
			var returnUrl = paymentProvider.SuccessUrl();
			var cancelUrl = paymentProvider.ErrorUrl();
			var reportUrl = paymentProvider.ReportUrl();
			
			#region config helper

			var apiURL = paymentProvider.GetSetting("TransactionRequestUrl");
			var merchantId = paymentProvider.GetSetting("merchantid");
			var merchantKey = paymentProvider.GetSetting("merchantkey");
			
			#endregion

			var request = new PaymentRequest();

			var orderGuidAsString = orderInfo.UniqueOrderId.ToString();

			var transactionId = orderGuidAsString.Substring(0, 16);

			request.Parameters.Add("shopid", "001");
			request.Parameters.Add("merchantid", merchantId);
			request.Parameters.Add("payment", string.Empty);
			// character purchase ID (max 16 characters)
			request.Parameters.Add("purchaseid", transactionId);

			var totalAmountInCents = orderInfo.ChargedAmountInCents;
			request.Parameters.Add("amount", totalAmountInCents.ToString());
			
			request.Parameters.Add("issuerid", orderInfo.PaymentInfo.MethodId);
			request.Parameters.Add("testmode", paymentProvider.TestMode.ToString());
			request.Parameters.Add("entrancecode", orderInfo.OrderNodeId.ToString());
			request.Parameters.Add("description", orderInfo.OrderNumber);

			request.Parameters.Add("returnurl", returnUrl);
			request.Parameters.Add("cancelurl", cancelUrl);
			request.Parameters.Add("callbackurl", reportUrl);
			request.Parameters.Add("notifyurl", reportUrl);

			#region esend

			//request.Parameters.Add("shipping_firstname", "#todo - optional");
			//request.Parameters.Add("shipping_lastname", "#todo");
			//request.Parameters.Add("shipping_mail", "#todo - optional");
			//request.Parameters.Add("shipping_company", "#todo - optional");
			//request.Parameters.Add("shipping_address1", "#todo");
			//request.Parameters.Add("shipping_address2", "#todo - optional");
			//request.Parameters.Add("shipping_zip", "#todo");
			//request.Parameters.Add("shipping_city", "#todo");
			//request.Parameters.Add("shipping_country", "#todo");
			//request.Parameters.Add("shipping_countrycode", "#todo");
			//request.Parameters.Add("shipping_phone", "#todo -optional");
			//request.Parameters.Add("weight", "#todo  -optional");
			//request.Parameters.Add("shipping", "#todo -optional");
			//request.Parameters.Add("handling", "#todo -optional");

			#endregion

			//de SHA1 waarde van purchaseid/entrancecode/amount/shopid/merchantid/merchantkey
			var sha1Hash = GetSHA1(transactionId + orderInfo.OrderNodeId.ToString() + totalAmountInCents.ToString() + "001" + merchantId + merchantKey);

			request.Parameters.Add("sha1", sha1Hash);

			request.PaymentUrlBase = apiURL;
			
			var responseString = _requestSender.SendRequest(request.PaymentUrlBase, request.ParametersAsString);
			
			XNamespace ns = "https://www.sisow.nl/Sisow/REST";
			if (responseString == null)
			{
				Log.Instance.LogError("SiSow responseString == null orderInfo.UniqueOrderId: " + orderInfo.UniqueOrderId);

				return null;
			}

			Log.Instance.LogDebug("SiSow responseString: " + responseString + ", paymentProviderNodeId: " + paymentProvider.Id);

			var issuerXml = XDocument.Parse(responseString);

			var url = issuerXml.Descendants(ns + "issuerurl").FirstOrDefault();
			var trxId = issuerXml.Descendants(ns + "trxid").FirstOrDefault();

			var decodeUrl = Uri.UnescapeDataString(url.Value);

			if (trxId == null)
			{
				Log.Instance.LogError("SiSow issuerXml: " + issuerXml + ", paymentProviderNodeId: " + paymentProvider.Id);

				return null;
			}

			var returnedTransactionId = trxId.Value;

			orderInfo.PaymentInfo.Url = decodeUrl;
			orderInfo.PaymentInfo.TransactionId = returnedTransactionId;
			
			PaymentProviderHelper.SetTransactionId(orderInfo, returnedTransactionId);

			orderInfo.Save();

			return null;
		}

		// compute SHA1
		private static string GetSHA1(string key)
		{
			var sha = new SHA1Managed();
			var enc = new UTF8Encoding();
			var bytes = sha.ComputeHash(enc.GetBytes(key));
			//string sha1 = System.BitConverter.ToString(sha1).Replace("-", "");
			var sha1 = "";
			for (var j = 0; j < bytes.Length; j++)
				sha1 += bytes[j].ToString("x2");
			return sha1;
		}

		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return string.Empty;
		}
	}
}