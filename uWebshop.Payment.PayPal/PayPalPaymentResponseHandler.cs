using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.PayPal
{
	/// <summary>
	/// Handles the response from the payment provider
	/// </summary>
	public class PayPalPaymentResponseHandler : PayPalPaymentBase, IPaymentResponseHandler
	{
		/// <summary>
		/// Handles the response
		/// </summary>
		public string HandlePaymentResponse(PaymentProvider paymentProvider)
		{	
			var payPalTestMode = paymentProvider != null && paymentProvider.TestMode;

			var testUrl = paymentProvider.GetSetting("testUrl");

			var liveUrl = paymentProvider.GetSetting("Url");

			var req = payPalTestMode ? (HttpWebRequest)WebRequest.Create(testUrl) : (HttpWebRequest)WebRequest.Create(liveUrl);

			//Set values for the request back
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			var param = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
			var strRequest = Encoding.ASCII.GetString(param);
			strRequest += "&cmd=_notify-validate";
			req.ContentLength = strRequest.Length;

			//for proxy
			//WebProxy proxy = new WebProxy(new Uri("http://url:port#"));
			//req.Proxy = proxy;

			//Send the request to PayPal and get the response
			var streamOut = new StreamWriter(req.GetRequestStream(), Encoding.ASCII);
			streamOut.Write(strRequest);
			streamOut.Close();
			var streamIn = new StreamReader(req.GetResponse().GetResponseStream());
			var strResponse = streamIn.ReadToEnd();
			streamIn.Close();

			Log.Instance.LogDebug("PAYPAL RETURN strResponse: " + strResponse);

			switch (strResponse)
			{
				case "VERIFIED":
					{
						// PayPal POSTS some values
						var paymentStatus = HttpContext.Current.Request["payment_status"];

						Log.Instance.LogDebug("PAYPAL RETURN paymentStatus: " + paymentStatus);

						// Get identifier created with the RequestHandler
						var transactionId = HttpContext.Current.Request["custom"];

						Log.Instance.LogDebug("PAYPAL RETURN transactionId: " + transactionId);

						// Match  identifier to order
						var order = OrderHelper.GetOrder(transactionId);

						// Check for match
						if (order != null && order.Paid == false)
						{
							Log.Instance.LogDebug("PAYPAL RETURN STATUS: " + paymentStatus);
							// Get statusses from payment provider Response
							switch (paymentStatus)
							{
								case "Completed":
									order.Paid = true;
									order.Status = OrderStatus.ReadyForDispatch;
									break;
								case "Failed":
									order.Paid = false;
									order.Status = OrderStatus.PaymentFailed;
									break;
								case "Denied":
									order.Paid = false;
									order.Status = OrderStatus.PaymentFailed;
									break;
								case "Pending":
									order.Status = OrderStatus.WaitingForPaymentProvider;

									break;
							}
							order.Save();
						}
					}
					break;
				case "INVALID":
					break;
			}

			return null;
		}
	}
}