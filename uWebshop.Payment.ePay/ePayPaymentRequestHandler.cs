using System.Collections.Generic;
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
            // successUrl - is a browser redirect, page is shown to card holder after payment success, this is where you show a receipt
            // errorUrl   - is a browser redirect, page is shown to card holder if a payment error occured
            //
            // parameters http://tech.epay.dk/da/betalingsvindue-parametre
            // md5 secret is optional but recommended
            //
            // payment provider configuration is made in ~\App_Plugins\uWebshop\config\PaymentProviders.config
            // example:
            // <provider title="ePay">
            //   <merchantnumber>12345678</merchantnumber>
            //   <secret>xc78ekjY3H!K</secret>
            //   <language>0</language>
            //   <url>https://ssl.ditonlinebetalingssystem.dk/integration/ewindow/Default.aspx</url>
            // </provider> 

            var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);
            var request = new PaymentRequest();
            request.Parameters.Add("currency", orderInfo.StoreInfo.Store.CurrencyCultureSymbol);
            request.Parameters.Add("amount", orderInfo.ChargedAmountInCents.ToString());
            request.Parameters.Add("orderid", orderInfo.OrderNumber);
            request.Parameters.Add("windowstate", "3");
            request.Parameters.Add("instantcallback", "1");
            request.Parameters.Add("callbackurl", paymentProvider.ReportUrl());     // server-to-server GET http://host/payment-providers/payment-providers/ePay?txnid=12345678&orderid=W0061..
            request.Parameters.Add("accepturl", paymentProvider.SuccessUrl());      // browser redirect GET http://host/receipt/?txnid=12345678&orderid=W0061..
            request.Parameters.Add("cancelurl", paymentProvider.ErrorUrl());

            //filter params
            var filter = new List<string> { "hash", "secret", "url" };
            var settings = paymentProvider.GetSettingsXML();
            foreach (var setting in settings.Descendants())
            {
                var key = setting.Name.LocalName.ToLowerInvariant();
                if (!request.Parameters.ContainsKey(key) && !string.IsNullOrEmpty(setting.Value) && (!filter.Contains(key)))
                {
                    request.Parameters.Add(key, setting.Value);
                }
            }

            //build MD5 if secret present
            var secret = paymentProvider.GetSetting("secret");
            if (secret != string.Empty)
            {
                var sb = new StringBuilder();
                foreach (var parameter in request.Parameters)
                {
                    sb.Append(parameter.Value);
                }
                request.Parameters.Add("hash", ePayPaymentBase.MD5(sb.ToString() + secret));
            }
            request.PaymentUrlBase = paymentProvider.GetSetting("url");

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