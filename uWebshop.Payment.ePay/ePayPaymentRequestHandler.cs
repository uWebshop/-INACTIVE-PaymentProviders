using System;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.ePay
{
	public class ePayPaymentRequestHandler : ePayPaymentBase, IPaymentRequestHandler
	{
		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);
			
			var failedUrl = paymentProvider.ErrorUrl();

			var reportUrl = paymentProvider.ReportUrl();
			
			#region config helper

			var merchantnumber = paymentProvider.GetSetting("merchantnumber");

			var url = paymentProvider.GetSetting("url");
			
			var uniqueId = orderInfo.OrderNumber + "x" + DateTime.Now.ToString("hhmmss");

			Log.Instance.LogDebug("ePay uniqueId " + uniqueId + ", paymentProviderNodeId: " + paymentProvider.Id);

			//<provider title="ePay">
			//  <merchantnumber>#YOUR Merchant Number#</accountId>
			//  <url>https://ssl.ditonlinebetalingssystem.dk/integration/ewindow/Default.aspx</url>
			//</provider> 

			#endregion

			var request = new PaymentRequest();

			// retrieve Account ID
			request.Parameters.Add("merchantnumber", merchantnumber);
			request.Parameters.Add("amount", orderInfo.ChargedAmountInCents.ToString());
			request.Parameters.Add("orderid", uniqueId);

			//request.Parameters.Add("callbackurl", reportUrl);
			request.Parameters.Add("accepturl", reportUrl);
			request.Parameters.Add("cancelurl", failedUrl);
			request.Parameters.Add("currency", orderInfo.StoreInfo.Store.CurrencyCultureSymbol);
			request.Parameters.Add("windowstate", "3");
			request.Parameters.Add("ownreceipt", "1");
			request.PaymentUrlBase = url;

			PaymentProviderHelper.SetTransactionId(orderInfo, uniqueId);

			orderInfo.PaymentInfo.Url = request.PaymentUrl;
			orderInfo.PaymentInfo.Parameters = request.ParametersAsString;

			return request;
		}

		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}
	}
}
