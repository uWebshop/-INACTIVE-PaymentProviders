using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Borgun
{
    public class BorgunPaymentResponseHandler : BorgunPaymentBase, IPaymentResponseHandler
    {
        public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo order)
        {
            order.Paid = true;
            order.Status = OrderStatus.ReadyForDispatch;
            order.Save();
            // if success then succes, otherwise error (todo)

            order.RedirectUrl = paymentProvider.SuccessUrl();

            return order;

        }

    }
}
