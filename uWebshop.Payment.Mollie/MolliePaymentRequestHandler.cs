using System;
using Mollie.iDEAL;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Mollie
{
	public class MolliePaymentRequestHandler : MolliePaymentBase, IPaymentRequestHandler
	{

		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);

			var partnerId = paymentProvider.GetSetting("PartnerId");
			var returnUrl = paymentProvider.SuccessUrl();
			var reportUrl = paymentProvider.ReportUrl();
			var testMode = paymentProvider.TestMode;

			var idealFetch = new IdealFetch(partnerId, testMode, orderInfo.OrderNumber, reportUrl, returnUrl, orderInfo.PaymentInfo.MethodId, orderInfo.ChargedAmount);

			if (idealFetch.Error)
			{
				Log.Instance.LogError("Mollie PaymentRequestHandler.CreatePaymentRequest: idealFetch.Error: " + idealFetch.ErrorMessage);
				return null;
			}

			var transactionId = idealFetch.TransactionId;
			orderInfo.PaymentInfo.Url = idealFetch.Url;

			PaymentProviderHelper.SetTransactionId(orderInfo, transactionId);
			
			return new PaymentRequest();
		}

		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}
	}
}