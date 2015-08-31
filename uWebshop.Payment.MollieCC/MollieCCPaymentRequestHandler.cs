using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mollie.Api;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.MollieCC
{
    public class MollieCCPaymentRequestHandler : MollieCCPaymentBase, IPaymentRequestHandler
    {
        public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
        {
            var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);

            Log.Instance.LogDebug("Payment Provider Request Handler: "+paymentProvider.DLLName);

            var apiLiveKey = paymentProvider.GetSetting("ApiLiveKey");
            var apiTestKey = paymentProvider.GetSetting("ApiTestKey");
            var returnUrl = paymentProvider.SuccessUrl();
            var reportUrl = paymentProvider.ReportUrl();
            bool testMode = paymentProvider.TestMode;

            string apiKey;
            if (testMode)
                apiKey = apiTestKey;
            else
                apiKey = apiLiveKey;

            MollieClient mollieClient = new MollieClient();
            mollieClient.setApiKey(apiKey);

            Log.Instance.LogDebug("Starting payment with a creditcard method ...");

            string unique = new Guid().ToString();

            var status = mollieClient.StartPayment(new Mollie.Api.Payment
            {
                //amount = orderInfo.ChargedAmount,
                amount = orderInfo.ChargedAmount,
                method = Method.creditcard,
                description = "Order " + orderInfo.OrderNumber,
                redirectUrl = returnUrl + "?order_id=" + orderInfo.UniqueOrderId.ToString(),
                webhookUrl = reportUrl + "?order_id=" + orderInfo.UniqueOrderId.ToString()
            });

            // Set Id in database
            PaymentProviderHelper.SetTransactionId(orderInfo, status.id);

            // Create request
            var request = new PaymentRequest();

            // Pass redirect URL
            request.PaymentUrlBase = status.links.paymentUrl;
            orderInfo.PaymentInfo.Url = status.links.paymentUrl;
            
            Log.Instance.LogDebug("MollieCC status:" + status.status.ToString());

            return request;
        }
        public string GetPaymentUrl(OrderInfo orderInfo)
        {
            return orderInfo.PaymentInfo.Url;
        }
    }
}
