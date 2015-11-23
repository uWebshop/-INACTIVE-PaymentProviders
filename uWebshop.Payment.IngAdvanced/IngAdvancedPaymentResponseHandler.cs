using System.Collections.Generic;
using System.Text;
using System.Web;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.IngAdvanced
{
    public class IngAdvancedPaymentResponseHandler: IngAdvancedPaymentBase, IPaymentResponseHandler
    {
        public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo order)
        {
            var transactionId = HttpContext.Current.Request["trxid"];

            if (string.IsNullOrEmpty(transactionId))
            {
                Log.Instance.LogDebug("IngAdvanced IPaymentResponseHandler transactionId == null");
                return null;
            }

            if (order == null)
            {
                order = OrderHelper.GetOrder(transactionId);
            }

            if (order != null)
            {
                var status = RequestTransactionStatus(transactionId, order);

                switch (status)
                {
                    case Transaction.TransactionStatus.Success:
                        order.Paid = true;
                        order.Status = OrderStatus.ReadyForDispatch;
                        break;
                    case Transaction.TransactionStatus.Open:
                        order.Status = OrderStatus.WaitingForPaymentProvider;
                        break;
                    case Transaction.TransactionStatus.Failure:
                    case Transaction.TransactionStatus.Expired:
                    case Transaction.TransactionStatus.Cancelled:
                        order.Paid = false;
                        order.Status = OrderStatus.PaymentFailed;
                        order.PaymentInfo.ErrorMessage = status.ToString();
                        break;
                    default:
                        order.Paid = false;
                        order.Status = OrderStatus.PaymentFailed;
                        order.PaymentInfo.ErrorMessage = status.ToString();
                        break;
                }
                order.Save();
            }

            return order;
        }

        private static Transaction.TransactionStatus RequestTransactionStatus(string transactionId, OrderInfo order)
        {
            try
            {
                var connector = new Connector();
                // Override MerchantId loaded from configuration
                //connector.MerchantId = "025152899";
                var transaction = connector.RequestTransactionStatus(transactionId);

                var acquirerId =  transaction.AcquirerId;
                var status = transaction.Status;
                var consumerName = transaction.ConsumerName;
                var fingerprint = transaction.Fingerprint;
                var consumerIBAN = transaction.ConsumerIBAN;
                var consumerBIC = transaction.ConsumerBIC;
                var amount = transaction.Amount;
                var currency = transaction.Currency;

                var signatureString = ByteArrayToHexString(transaction.SignatureValue);

                // Place newlines in Hex String
                for (int i = 256; i > 0; i -= 32)
                    signatureString = signatureString.Substring(0, i) + "<br />" + signatureString.Substring(i);

                var signatureValue = signatureString;

                return status;
            }
            catch (IDealException ex)
            {
                Log.Instance.LogError("ING Advanced PaymentRequestHander: " + ex);
            }

            return Transaction.TransactionStatus.Failure;
        }
        /// <summary>
        /// Gets the hexadecimal representation of an array of bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes to get a hexadecimal representation for.</param>
        /// <returns>The hexadecimal representation of the byte array.</returns>
        private static string ByteArrayToHexString(IEnumerable<byte> bytes)
        {
            var result = new StringBuilder();

            foreach (var b in bytes)
                result.Append(b.ToString("X2"));

            return result.ToString();
        }
    }
}
