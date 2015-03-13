using System;
using System.Web;
using Mollie.Api;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;
using uWebshop.Domain.Helpers;
using uWebshop.Common;

namespace uWebshop.Payment.MollieCC
{
    public class MollieCCPaymentResponseHandler : MollieCCPaymentBase, IPaymentResponseHandler
    {
        public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo orderInfo)
        {
            try
            {
                var orderId = HttpContext.Current.Request["order_id"];
                orderInfo = OrderHelper.GetOrder(uniqueOrderId:new Guid(orderId));

                Log.Instance.LogDebug("MollieCC Transaction Id: " + orderInfo.PaymentInfo.TransactionId);
                
                if (orderInfo == null)
                {
                    Log.Instance.LogError("MollieCC: Order Not Found For TransactionId: " + orderInfo.PaymentInfo.TransactionId);
                    return null;
                }

                if (orderInfo.Paid != false)
                {
                    Log.Instance.LogDebug("MollieCC: Order already paid! TransactionId: " + orderInfo.PaymentInfo.TransactionId);
                    return null;
                }

                var apiLiveKey = paymentProvider.GetSetting("ApiLiveKey");
                var apiTestKey = paymentProvider.GetSetting("ApiTestKey");
                bool testMode = paymentProvider.TestMode;

                MollieClient mollieClient = new MollieClient();

                if (testMode)
                    mollieClient.setApiKey(apiTestKey);
                else
                    mollieClient.setApiKey(apiLiveKey);

                PaymentStatus paymentStatus = mollieClient.GetStatus(orderInfo.PaymentInfo.TransactionId);

                if (paymentStatus.status.Value.ToString() == "paid" || paymentStatus.status.Value.ToString() == "paidout")
                {
                    orderInfo.Paid = true;
                    orderInfo.Status = OrderStatus.ReadyForDispatch;
                }
                else
                {
                    orderInfo.Paid = false;
                    orderInfo.Status = OrderStatus.PaymentFailed;
                    Log.Instance.LogError("MollieCCPaymentResponseHandler.HandlePaymentResponse NOT PAID: status:" + paymentStatus.status.Value.ToString() + " id: "+paymentStatus.id);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.LogError("MollieCCPaymentResponseHandler.HandlePaymentResponse: " + ex);
            }

            return null;
        }
    }
}
