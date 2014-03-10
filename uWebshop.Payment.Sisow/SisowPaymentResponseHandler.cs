using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using umbraco;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.Sisow
{
	public class SisowPaymentResponseHandler : IPaymentResponseHandler
	{
		public string GetName()
		{
			return "Sisow";
		}

		public string HandlePaymentResponse(PaymentProvider paymentProvider)
		{
			var orderId = library.Request("ec");
			var transactionId = library.Request("trxid");
			var status = library.Request("status");

			var orderInfo = OrderHelper.GetOrder(transactionId);

			if (orderInfo != null && orderInfo.Paid == false)
			{
				switch (status)
				{
					case "Success":
						orderInfo.Paid = true;
						orderInfo.Status = OrderStatus.ReadyForDispatch;
						break;
					case "Cancelled":
					case "Expired":
					case "Failure":
					case "Reversed":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.PaymentFailed;
						orderInfo.PaymentInfo.ErrorMessage = status;
						break;
					case "Open":
					case "Pending":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.WaitingForPaymentProvider;
						break;
				}

				orderInfo.Save();
			}
			else
			{
				Log.Instance.LogDebug("SISOW ORDERINFO == NULL!!!!");
			}

			return null;
		}
	}
}