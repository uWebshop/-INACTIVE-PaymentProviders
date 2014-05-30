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

        public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo orderInfo)
		{
			var orderId = library.Request("ec");
			var transactionId = library.Request("trxid");
			var status = library.Request("status");

			orderInfo = OrderHelper.GetOrder(transactionId);

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