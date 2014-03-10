using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Linq;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.NodeFactory;
using System.Threading;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.MultiSafePay
{
	public class MultiSafePayPaymentRequestHandler : MultiSafePayPaymentBase, IPaymentRequestHandler
	{
		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);
			
			var returnUrl = paymentProvider.SuccessUrl();
			var cancelUrl = paymentProvider.ErrorUrl();

			var reportUrl = paymentProvider.ReportUrl();
			
			var accountId = paymentProvider.GetSetting("accountId");
			var siteId = paymentProvider.GetSetting("siteId");
			var siteSecureId = paymentProvider.GetSetting("siteSecureId");
			var testURL = paymentProvider.GetSetting("testURL");
			var liveUrl = paymentProvider.GetSetting("url");
			

			#region fill transactionrequest object

			var transactionRequest = new TransactionRequest
			                         {
				                         AccountId = long.Parse(accountId), 
										 SiteId = long.Parse(siteId), 
										 SiteSecureId = long.Parse(siteSecureId), 
										 NotificationUrl = reportUrl, 
										 CancelUrl = cancelUrl, 
										 RedirectUrl = returnUrl, 
										 Locale = orderInfo.StoreInfo.CultureInfo.TwoLetterISOLanguageName + "_" + orderInfo.CustomerInfo.CountryCode, 
										 IPAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"], 
										 ForwardedIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"], 
										 FirstName = OrderHelper.CustomerInformationValue(orderInfo, "customerFirstName"), 
										 LastName = OrderHelper.CustomerInformationValue(orderInfo, "customerLastName"), 
										 Country = OrderHelper.CustomerInformationValue(orderInfo, "customerCountry"), 
										 Email = OrderHelper.CustomerInformationValue(orderInfo, "customerEmail")
			                         };

			var amount = orderInfo.ChargedAmountInCents;

			transactionRequest.TransactionId = orderInfo.UniqueOrderId.ToString();
			transactionRequest.Currency = orderInfo.StoreInfo.Store.CurrencyCultureSymbol;
			transactionRequest.Amount = amount;
			transactionRequest.Decription = orderInfo.OrderNumber;
			transactionRequest.Gateway = orderInfo.PaymentInfo.MethodId;

			var stringToHash = amount + transactionRequest.Currency + transactionRequest.AccountId + transactionRequest.SiteId + transactionRequest.TransactionId;

			transactionRequest.Signature = GetMd5Sum(stringToHash);

			#endregion

			var apiURL = paymentProvider.TestMode ? testURL : liveUrl;

			var httpWebRequest = (HttpWebRequest) WebRequest.Create(apiURL);
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentLength = Encoding.UTF8.GetByteCount(transactionRequest.GetXml());
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";

			Log.Instance.LogDebug("MultiSafePay GetAllPaymentMethods transactionRequest GetXml: " + HttpUtility.HtmlEncode(transactionRequest.GetXml()));

			var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
			streamWriter.Write(transactionRequest.GetXml());
			streamWriter.Close();

			var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
			var streamReader = new StreamReader(httpWebResponse.GetResponseStream());

			//<?xml version="1.0" encoding="UTF-8" ?>
			//<redirecttransaction result="ok">
			//  <transaction>
			//      <id>4084044</id>
			//      <payment_url>https://user.multisafepay.com/pay/?trans...</payment_url>
			//  </transaction>
			//</redirecttransaction>
			var xmlDoc = XDocument.Parse(streamReader.ReadToEnd());

			Log.Instance.LogDebug("MultiSafePay xmlDoc: " + xmlDoc);

			if (xmlDoc.Root == null)
			{
				return null;
			}

			var transaction = xmlDoc.Root.Element("transaction");

			if (transaction == null)
			{
				Log.Instance.LogError("MultiSafePay transaction == null");
				return null;
			}

			var paymentUrl = transaction.Element("payment_url");

			if (paymentUrl == null)
			{
				Log.Instance.LogError("MultiSafePay paymentUrl == null");
				return null;
			}

			orderInfo.PaymentInfo.Url = paymentUrl.Value;
			PaymentProviderHelper.SetTransactionId(orderInfo, orderInfo.UniqueOrderId.ToString());

			return null;
		}

		// Create an md5 sum string of this string
		public static string GetMd5Sum(string str)
		{
			var x = new MD5CryptoServiceProvider();

			var data = Encoding.ASCII.GetBytes(str);

			data = x.ComputeHash(data);

			return BitConverter.ToString(data).Replace("-", "").ToLower();
		}


		/// <summary>
		/// Returns the URL to redirect to
		/// </summary>
		/// <param name="orderInfo"></param>
		/// <returns></returns>
		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}
	}
}