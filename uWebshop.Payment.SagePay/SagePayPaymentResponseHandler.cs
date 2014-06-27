using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.SagePay
{
    public class SagePayPaymentResponseHandler : IPaymentResponseHandler
    {
        public string GetName()
        {
            return "SagePay";
        }

        public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo orderInfo)
        {
         
            // Check for match
            if (orderInfo != null && orderInfo.Paid == false)
            {
                if (!string.IsNullOrEmpty(orderInfo.PaymentInfo.Parameters))
                {
                    var status = orderInfo.PaymentInfo.Parameters.Split('&')[0];
                    var message = orderInfo.PaymentInfo.Parameters.Split('&')[1];

                    // Get statusses from payment provider Response
                    switch (status.ToUpperInvariant())
                    {
                        case "OK":
                            orderInfo.Paid = true;
                            orderInfo.Status = OrderStatus.ReadyForDispatch;

                            break;
                        case "MALFORMED":
                            orderInfo.Paid = false;
                            orderInfo.Status = OrderStatus.PaymentFailed;
                            orderInfo.PaymentInfo.ErrorMessage = message;
                            Log.Instance.LogError("SagePay Payment Error: " + message);

                            break;
                        case "INVALID":
                            orderInfo.Paid = false;
                            orderInfo.Status = OrderStatus.PaymentFailed;
                            orderInfo.PaymentInfo.ErrorMessage = message;
                            Log.Instance.LogError("SagePay Payment Error: " + message);

                            break;
                        case "ERROR":
                            orderInfo.Paid = false;
                            orderInfo.Status = OrderStatus.Incomplete;
                            orderInfo.PaymentInfo.ErrorMessage = message;
                            Log.Instance.LogError("SagePay Payment Error: " + message);

                            break;
                    }
                }

                orderInfo.Save();
            }

            return orderInfo;
        }

        #region acknowledge receipt

        //public PaymentRequest CreateAcknowlegeReceipt(OrderInfo orderInfo, string status, string errorDetails)
        //{
        //    var response = new PaymentRequest();
        //    var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);
            
        //    var returnUrl = paymentProvider.SuccessUrl();
        //    var cancelUrl = paymentProvider.ErrorUrl();

        //    HttpContext.Current.Response.Clear();
        //    HttpContext.Current.Response.ContentType = "text/plain";
        //    HttpContext.Current.Response.Output.WriteLine("Status=" + status);
        //    // STATUS CAN BE:
        //    // - OK
        //    // - INVALID
        //    // - ERROR
        //    HttpContext.Current.Response.Output.WriteLine("RedirectURL={0}", status == "OK" ? returnUrl : cancelUrl);

        //    HttpContext.Current.Response.Output.WriteLine("StatusDetail=" + errorDetails);
        //    return response;
        //}

        #endregion
    }
}