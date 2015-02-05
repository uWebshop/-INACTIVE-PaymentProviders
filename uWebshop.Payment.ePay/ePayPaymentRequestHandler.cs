using System.Text;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.ePay
{
    public class ePayPaymentRequestHandler : ePayPaymentBase, IPaymentRequestHandler
    {
        public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
        {
            // reportUrl  - is a server-to-server call, changes order state, sends out e-mail
            // successUrl - is a browser redirect, page is shown to the card holder after payment success, this is where you show a receipt
            // errorUrl   - is a browser redirect, page is shown to the card holder if a payment error occured

            var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);
            var errorUrl = paymentProvider.ErrorUrl();
            var successUrl = paymentProvider.SuccessUrl();
            var reportUrl = paymentProvider.ReportUrl();

            // payment provider configuration is made in ~\App_Plugins\uWebshop\config\PaymentProviders.config
            // m5 secret is optional but recommended
            //
            // <provider title="ePay">
            //   <merchantnumber>merchantnumber</accountId>
            //   <secret>md5secret</secret>
            //   <url>https://ssl.ditonlinebetalingssystem.dk/integration/ewindow/Default.aspx</url>
            // </provider> 

            var merchantnumber = paymentProvider.GetSetting("merchantnumber");
            var url = paymentProvider.GetSetting("url");
            var secret = paymentProvider.GetSetting("secret");

            var request = new PaymentRequest();
            request.Parameters.Add("merchantnumber", merchantnumber);
            request.Parameters.Add("amount", orderInfo.ChargedAmountInCents.ToString());
            request.Parameters.Add("orderid", orderInfo.OrderNumber);
            request.Parameters.Add("callbackurl", reportUrl);       // server-to-server GET http://host/payment-providers/payment-providers/ePay?txnid=12345678&orderid=W0061..
            request.Parameters.Add("accepturl", successUrl);        // browser-redirect GET http://host/receipt/?txnid=12345678&orderid=W0061..
            request.Parameters.Add("cancelurl", errorUrl);
            request.Parameters.Add("currency", orderInfo.StoreInfo.Store.CurrencyCultureSymbol);
            request.Parameters.Add("windowstate", "3");

            //build MD5 if secret present
            if (secret != string.Empty)
            {
                var sb = new StringBuilder();
                foreach (var parameter in request.Parameters)
                {
                    sb.Append(parameter.Value);
                }
                request.Parameters.Add("hash", ePayPaymentBase.MD5(sb.ToString() + secret));
            }
            request.PaymentUrlBase = url;

            PaymentProviderHelper.SetTransactionId(orderInfo, orderInfo.OrderNumber);
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