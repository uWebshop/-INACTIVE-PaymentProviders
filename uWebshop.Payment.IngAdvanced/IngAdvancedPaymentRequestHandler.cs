using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.IngAdvanced
{
    public class IngAdvancedPaymentRequestHandler: IngAdvancedPaymentBase, IPaymentRequestHandler
    {
        public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
        {
            try
            {
                var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);

                var reportUrl = paymentProvider.ReportUrl();

                //Use https://idealtest.secure-ing.com/ideal/iDEALv3 during integration/test
                //Use https://ideal.secure-ing.com/ideal/iDEALv3 only for production

                //	<provider title="IngAdvanced">
                //    <IssuerId>1111111</IssuerId>
                //    <MerchantId>1111111</MerchantId>
                //    <EntranceCode>22222222</EntranceCode>
                //  </provider>

                var issuerId = paymentProvider.GetSetting("IssuerId");
                var merchantId = paymentProvider.GetSetting("MerchantId");
                var entranceCode = paymentProvider.GetSetting("EntranceCode");
                

                var transaction = new Transaction
                {
                    Amount = orderInfo.ChargedAmount,
                    Description = orderInfo.OrderNumber,
                    PurchaseId = orderInfo.OrderNumber,
                    IssuerId = issuerId,
                    EntranceCode = entranceCode
                };
                
                var connector = new Connector
                {   
                    MerchantReturnUrl = new Uri(reportUrl),
                    MerchantId = merchantId,
                    SubId = "0",
                    ExpirationPeriod = "PT10M"
                };
              
                transaction = connector.RequestTransaction(transaction);

                if (transaction.Status == Transaction.TransactionStatus.Success)
                {
                    var transactionId = transaction.Id;
                    var authenticateUrl = transaction.IssuerAuthenticationUrl.ToString();
                    var acquirerId = transaction.AcquirerId;

                    PaymentProviderHelper.SetTransactionId(orderInfo, transactionId);
                    orderInfo.PaymentInfo.Url = authenticateUrl;
                    orderInfo.PaymentInfo.Parameters = acquirerId;

                    orderInfo.Save();
                }
                else
                {
                    // todo: failure handling, so don't change anything, user will not be redirected
                }
            }
            catch (IDealException ex)
            {
                Log.Instance.LogError("ING Advanced PaymentRequestHander: " + ex);
            }

            var request = new PaymentRequest();

            return request;
        }

        public string GetPaymentUrl(OrderInfo orderInfo)
        {
            return orderInfo.PaymentInfo.Url;
        }
    }
}
