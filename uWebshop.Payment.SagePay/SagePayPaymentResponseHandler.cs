using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.NodeFactory;

namespace uWebshop.Payment.SagePay
{
	public class SagePayPaymentResponseHandler : IPaymentResponseHandler
	{
		public string GetName()
		{
			return "SagePay";
		}

		public string HandlePaymentResponse(PaymentProvider paymentProvider)
		{
			// PayPal POSTS some values
			string paymentStatus = library.Request("Status");

			// Get identifier created with the RequestHandler
			string transactionId = library.Request("VPSTxId");

			// Match  identifier to order
			var orderInfo = OrderHelper.GetOrderInfo(transactionId);

			// Check for match
			if (orderInfo != null && orderInfo.Paid == false)
			{
				// Get statusses from payment provider Response
				switch (paymentStatus)
				{
					case "OK":
						orderInfo.Paid = true;
						orderInfo.Status = OrderStatus.ReadyForDispatch;
						CreateAcknowlegeReceipt(orderInfo, "OK", "");

						break;
					case "MALFORMED":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.PaymentFailed;
						orderInfo.PaymentInfo.ErrorMessage = library.Request("StatusDetail");

						CreateAcknowlegeReceipt(orderInfo, "INVALID", "Malformed");

						break;
					case "INVALID":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.PaymentFailed;
						orderInfo.PaymentInfo.ErrorMessage = library.Request("StatusDetail");

						CreateAcknowlegeReceipt(orderInfo, "INVALID", "");

						break;
					case "ERROR":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.PaymentFailed;
						orderInfo.PaymentInfo.ErrorMessage = library.Request("StatusDetail");

						CreateAcknowlegeReceipt(orderInfo, "ERROR", "");

						break;
				}

				orderInfo.Save();
			}

			return null;
		}

		#region acknowledge receipt

		public PaymentRequest CreateAcknowlegeReceipt(OrderInfo orderInfo, string status, string errorDetails)
		{
			var response = new PaymentRequest();

			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);
			var helper = new PaymentConfigHelper(paymentProvider);

			#region build urls

			var currentNodeId = Node.GetCurrent().Id;
			var baseUrl = PaymentProviderHelper.GenerateBaseUrl(currentNodeId);

			var successNodeId = 0;
			int.TryParse(paymentProvider.SuccesNodeId, out successNodeId);

			var returnUrl = baseUrl;

			if (successNodeId != 0)
			{
				returnUrl = string.Format("{0}{1}", baseUrl, library.NiceUrl(successNodeId));
			}

			var errorNodeId = 0;
			int.TryParse(paymentProvider.ErrorNodeId, out errorNodeId);

			var cancelUrl = baseUrl;

			cancelUrl = string.Format("{0}{1}", baseUrl, library.NiceUrl(int.Parse(paymentProvider.ErrorNodeId)));

			#endregion

			HttpContext.Current.Response.Clear();
			HttpContext.Current.Response.ContentType = "text/plain";
			HttpContext.Current.Response.Output.WriteLine("Status=" + status);
			// STATUS CAN BE:
			// - OK
			// - INVALID
			// - ERROR
			HttpContext.Current.Response.Output.WriteLine("RedirectURL={0}", status == "OK" ? returnUrl : cancelUrl);

			HttpContext.Current.Response.Output.WriteLine("StatusDetail=" + errorDetails);
			return response;
		}

		#endregion
	}
}