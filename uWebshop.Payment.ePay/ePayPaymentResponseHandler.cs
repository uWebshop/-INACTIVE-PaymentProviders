using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.ePay
{
	public class ePayPaymentResponseHandler : ePayPaymentBase, IPaymentResponseHandler
	{
        public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo order)
		{
			var orderId = HttpContext.Current.Request.QueryString["orderid"];

			if (paymentProvider == null || string.IsNullOrEmpty(orderId))
			{
				return null;
			}
		
			order = OrderHelper.GetOrder(orderId);
			
			var localizedPaymentProvider = PaymentProvider.GetPaymentProvider(order.PaymentInfo.Id, order.StoreInfo.Alias);

			var acceptUrl = localizedPaymentProvider.SuccessUrl();
			var failedUrl = localizedPaymentProvider.ErrorUrl();

			var redirectUrl = failedUrl;
			
			var amount = HttpContext.Current.Request.QueryString["amount"];

			if (amount == order.ChargedAmountInCents.ToString())
			{
				order.Paid = true;
				order.Status = OrderStatus.ReadyForDispatch;
				order.Save();

				redirectUrl = acceptUrl;
			}
			else
			{
				order.Paid = false;
				order.Save();
			}
			
			HttpContext.Current.Response.Redirect(redirectUrl);

            return order;
		}
	}
}