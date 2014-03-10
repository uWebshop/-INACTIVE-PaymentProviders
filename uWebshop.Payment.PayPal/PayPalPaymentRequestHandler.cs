using System;
using System.Globalization;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.PayPal
{
	/// <summary>
	/// Create request to Payment Provider API
	/// </summary>
	public class PayPalPaymentRequestHandler : PayPalPaymentBase, IPaymentRequestHandler
	{
		/// <summary>
		/// Creates a payment request for this payment provider
		/// </summary>
		/// <param name="orderInfo"> </param>
		/// <returns>Payment request</returns>
		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);

			#region build urls

			var baseUrl = PaymentProviderHelper.GenerateBaseUrl();

			var returnUrl = paymentProvider.SuccessUrl();

			var reportUrl = paymentProvider.ReportUrl();

			#endregion

			#region config helper

			var accountId = paymentProvider.GetSetting("AccountId");
			
			var testUrl = paymentProvider.GetSetting("testUrl");
			
			var liveUrl = paymentProvider.GetSetting("Url");

			#endregion

			var uniqueId = orderInfo.OrderNumber + "x" + DateTime.Now.ToString("hhmmss");


			var request = new PaymentRequest();
			request.Parameters.Add("cmd", "_xclick");

			// retrieve Account ID
			request.Parameters.Add("business", accountId);

			//request.Parameters.Add("invoice", order.OrderInfo.OrderNumber.ToString());
			request.Parameters.Add("invoice", orderInfo.OrderNumber);
			var ci = new CultureInfo("en-US");
			var totalAmountAsString = orderInfo.ChargedAmount.ToString("N", ci);
			request.Parameters.Add("amount", totalAmountAsString);
			request.Parameters.Add("tax_cart", totalAmountAsString);
			request.Parameters.Add("no_note", "0");
			var ri = new RegionInfo(orderInfo.StoreInfo.Store.CurrencyCultureInfo.LCID);
			request.Parameters.Add("currency_code", ri.ISOCurrencySymbol);
			request.Parameters.Add("lc", orderInfo.StoreInfo.CultureInfo.TwoLetterISOLanguageName);

			request.Parameters.Add("return", returnUrl);

			request.Parameters.Add("shopping_url", baseUrl);

			request.Parameters.Add("notify_url", reportUrl);

			#region testmode

			if (paymentProvider.TestMode)
			{
				request.Parameters.Add("cn", "Test");
			}

			#endregion

			// Order as shown with PayPal
			request.Parameters.Add("item_name", orderInfo.OrderNumber);

			// Set GUID to identify order in SSWS
			// Sent GUID for identification to PayPal
			// PayPal will return custom value to validate order

			request.Parameters.Add("custom", uniqueId);

			// check if provider is in testmode to send request to right URL
			request.PaymentUrlBase = paymentProvider.TestMode ? testUrl : liveUrl;

			PaymentProviderHelper.SetTransactionId(orderInfo, uniqueId);

			orderInfo.PaymentInfo.Url = request.PaymentUrl;
			orderInfo.PaymentInfo.Parameters = request.ParametersAsString;

			return request;
		}

		/// <summary>
		/// Returns the URL to redirect to
		/// </summary>
		/// <param name="orderInfo"> </param>
		/// <returns></returns>
		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}
	}
}