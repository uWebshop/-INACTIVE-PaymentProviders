using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.ePay
{
    public class ePayPaymentResponseHandler : ePayPaymentBase, IPaymentResponseHandler
    {
        public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo orderInfo)
        {
            var orderId = HttpContext.Current.Request.QueryString["orderid"] ?? "";
            var transactionId = HttpContext.Current.Request.QueryString["txnid"] ?? "0";
            if (paymentProvider == null || string.IsNullOrEmpty(orderId))
            {
                return null;
            }
            orderInfo = OrderHelper.GetOrder(orderId);
            if (orderInfo != null)
            {
                var localizedPaymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);
                var secret = paymentProvider.GetSetting("secret");
                var amount = HttpContext.Current.Request.QueryString["amount"] ?? "0";
                var validated = true;
                if (secret != string.Empty)
                {
                    var values = string.Empty;
                    foreach (var key in HttpContext.Current.Request.QueryString.AllKeys)
                    {
                        if (key != "hash")
                        {
                            values += HttpContext.Current.Request.QueryString[key];
                        }
                    }
                    var calculated = ePayPaymentBase.MD5(values + secret).ToUpperInvariant();
                    var incoming = (HttpContext.Current.Request.QueryString["hash"] ?? "").ToUpperInvariant();
                    validated = calculated == incoming;
                    if (!validated)
                    {
                        //checksum error
                        Log.Instance.LogError("Payment provider (ePay) error : Orderid " + orderId + " - incoming hash " + incoming + " - calculated hash " + calculated);
                    }
                }
                if (validated && (amount == orderInfo.ChargedAmountInCents.ToString()))
                {
                    orderInfo.Paid = true;
                    orderInfo.Status = OrderStatus.ReadyForDispatch;
                    orderInfo.Save();
                }
                else
                {
                    orderInfo.Paid = false;
                    orderInfo.Status = OrderStatus.PaymentFailed;
                    orderInfo.Save();
                    if (validated)
                    {
                        //checksum already logged, must be problem with amount
                        Log.Instance.LogError("Payment provider (ePay) error : Orderid " + orderId + " - incoming amount " + amount.ToString() + " - order amount " + orderInfo.ChargedAmountInCents.ToString());
                    }
                }
            }
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write("OK");
            HttpContext.Current.Response.Flush();
            return orderInfo;
        }
    }
}