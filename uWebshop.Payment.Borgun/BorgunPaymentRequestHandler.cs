using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Borgun
{
    public class BorgunPaymentRequestHandler : BorgunPaymentBase, IPaymentRequestHandler
    {
        public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
        {
            var uniqueId = orderInfo.OrderNumber + "x" + DateTime.Now.ToString("hhmmss");

            uniqueId = (uniqueId.Length > 12) ? uniqueId.Substring(0, 12) : uniqueId.PadRight(12, '0');

            var request = new PaymentRequest();

            PaymentProviderHelper.SetTransactionId(orderInfo, uniqueId);

            HttpContext.Current.Session.Add("TransactionId", uniqueId);

            orderInfo.PaymentInfo.Url = request.PaymentUrl;
            orderInfo.PaymentInfo.Parameters = request.ParametersAsString;

            orderInfo.Save();

            return request;
        }

        public string GetPaymentUrl(OrderInfo orderInfo)
        {
            return orderInfo.PaymentInfo.Url;
        }
    }
}
