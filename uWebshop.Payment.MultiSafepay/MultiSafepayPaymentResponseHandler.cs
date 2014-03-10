using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.MultiSafePay
{
	public class MultiSafePayPaymentResponseHandler : MultiSafePayPaymentBase, IPaymentResponseHandler
	{
		#region IPaymentResponseHandler Members

		public string HandlePaymentResponse(PaymentProvider paymentProvider)
		{
			var transactionId = HttpContext.Current.Request["transactionid"];

			if (string.IsNullOrEmpty(transactionId))
			{
				Log.Instance.LogDebug("MultiSafePay IPaymentResponseHandler transactionId == null");
				return null;
			}

			var orderInfo = OrderHelper.GetOrder(transactionId);

			if (orderInfo.Paid != false)
			{
				Log.Instance.LogDebug("MultiSafePay IPaymentResponseHandler Order Already Paid for transactionId: " + transactionId);
				return null;
			}

			var accountId = paymentProvider.GetSetting("accountId");
			var siteId = paymentProvider.GetSetting("siteId");
			var siteSecureId = paymentProvider.GetSetting("siteSecureId");
			var testURL = paymentProvider.GetSetting("testURL");
			var liveUrl = paymentProvider.GetSetting("url");
				
			var statusRequest = new StatusRequest
			                    {
				                    AccountId = long.Parse(accountId), 
				                    TransactionId = transactionId, 
				                    SiteId = long.Parse(siteId),
				                    SiteSecureId = long.Parse(siteSecureId)
			                    };

			var apiURL = paymentProvider.TestMode ? testURL : liveUrl;

			try
			{
				var httpWebRequest = (HttpWebRequest) WebRequest.Create(apiURL);
				httpWebRequest.Method = "POST";
				httpWebRequest.ContentLength = Encoding.UTF8.GetByteCount(statusRequest.GetXml());
				httpWebRequest.ContentType = "application/x-www-form-urlencoded";

				var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
				streamWriter.Write(statusRequest.GetXml());
				streamWriter.Close();

				var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
				var streamReader = new StreamReader(httpWebResponse.GetResponseStream());
				var xmlDoc = XDocument.Parse(streamReader.ReadToEnd());

				Log.Instance.LogDebug("MultiSafePay IPaymentResponseHandler XML Answer: " + HttpUtility.HtmlEncode(xmlDoc.ToString(SaveOptions.None)));

				var ewallet = xmlDoc.Root.Element("ewallet");
				var status = ewallet.Element("status").Value;

				orderInfo.Status = OrderStatus.WaitingForPayment;

				//– completed: succesvol voltooid
				//– initialized: aangemaakt, maar nog niet voltooid
				//– uncleared: aangemaakt, maar nog niet vrijgesteld (credit cards)
				//– void: geannuleerd
				//– declined: afgewezen
				//– refunded: terugbetaald
				//– expired: verlopen

				switch (status)
				{
					case "completed":
						orderInfo.Paid = true;
						orderInfo.Status = OrderStatus.ReadyForDispatch;
						break;
					case "uncleared":
						orderInfo.Status = OrderStatus.WaitingForPaymentProvider;
						break;
					case "declined":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.PaymentFailed;
						orderInfo.PaymentInfo.ErrorMessage = status;
						break;
					case "expired":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.PaymentFailed;
						orderInfo.PaymentInfo.ErrorMessage = status;
						break;
					case "void":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.PaymentFailed;
						orderInfo.PaymentInfo.ErrorMessage = status;
						break;
				}
				
				orderInfo.Save();
			}
			catch (Exception ex)
			{
				Log.Instance.LogError("MultiSafePayPaymentResponseHandler.HandlePaymentResponse: " + ex);
			}

			return null;
		}

		#endregion
	}
}