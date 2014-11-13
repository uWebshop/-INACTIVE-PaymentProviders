using System;
using System.Xml.Linq;
using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using System.Collections.Generic;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.EasyIdeal
{
	/// <summary>
	/// Handles the response from the payment provider
	/// </summary>
	public class EasyIdealPaymentResponseHandler : EasyIdealPaymentBase, IPaymentResponseHandler
	{
		/// <summary>
		/// Handles the response
		/// </summary>
        public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo orderInfo)
		{	
            #region config helper

         


            var merchantId = paymentProvider.GetSetting("merchantId");

            var merchantKey = paymentProvider.GetSetting("merchantKey");

            var merchantSecret = paymentProvider.GetSetting("merchantSecret");

            var url = paymentProvider.GetSetting("url");

            var transactionId = HttpContext.Current.Request["id"];

		    

            if (orderInfo == null)
            {
                orderInfo = OrderHelper.GetOrder(transactionId);
            }


            if (orderInfo == null)
            {
                Log.Instance.LogError("Easy iDeal response: Order Not Found For TransactionId: " + transactionId);

                HttpContext.Current.Response.Redirect(paymentProvider.ErrorUrl());
                return null;
            }

            // get the localized (for the right store) payment provider to get the right success/return urls
            var localizedPaymentProvider = PaymentProvider.GetPaymentProvider(paymentProvider.Id, orderInfo.StoreInfo.Alias);

            var redirectUrl = localizedPaymentProvider.ErrorUrl();
            var successUrl = localizedPaymentProvider.SuccessUrl();

            #endregion
            try
            {
                var paymentStatus = HttpContext.Current.Request["status"];
                var salt = HttpContext.Current.Request["salt"];
                var checksum = HttpContext.Current.Request["checksum"];

                if (string.IsNullOrEmpty(transactionId))
                {
                    Log.Instance.LogError("Easy iDeal response: TransactionId IsNullOrEmpty");
                    HttpContext.Current.Response.Redirect(paymentProvider.ErrorUrl());
                    return null;
                }
                if (orderInfo.Paid != false)
                {
                    Log.Instance.LogDebug("Easy iDeal response: Order already paid! TransactionId: " + transactionId);
                    HttpContext.Current.Response.Redirect(paymentProvider.ErrorUrl());
                    return null;
                }

                var transactionCode = OrderHelper.ExtraInformationValue(orderInfo, "extraTransactionCode");

                if (string.IsNullOrEmpty(transactionCode))
                {
                    Log.Instance.LogDebug("Easy iDeal response: OrderTransactionCode IsNullOrEmpty ");
                    HttpContext.Current.Response.Redirect(paymentProvider.ErrorUrl());
                    return null;
                }

                //check validity of request
                if (CheckChecksumPaymentStatus(transactionId, transactionCode, paymentStatus, salt, checksum)) //only check for payment status if request is valid.
                    //This is a bit redundant, since you allready now the paymentStatus. But we could choose to do something with the extra info you get from this request e.g. IBAN
                {

                    var args = new SortedList<string, string>
                        {
                            {"TransactionID", transactionId},
                            {"TransactionCode", transactionCode}
                        };


                    var xmlRequest = GetXml(TRANSACTIONSTATUS, args, merchantId, merchantKey, merchantSecret);

                    XDocument xmlResponse = XDocument.Parse(PostXml(xmlRequest, url));
                    
                    var responseStatus = xmlResponse.Element("Response").Element("Status").FirstNode.ToString();

                    if (responseStatus == "OK")
                    {
                        if (xmlResponse.Element("Response").Element("Transaction").Element("Paid").FirstNode.ToString() == "Y")
                        {
                            orderInfo.Paid = true;
                            orderInfo.Status = OrderStatus.ReadyForDispatch;
                            redirectUrl = successUrl;

                        }
                        else
                        {
                            orderInfo.Paid = false;
                            orderInfo.Status = OrderStatus.PaymentFailed;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Instance.LogError("EasyIdealPaymentResponseHandler.HandlePaymentResponse: " + ex);
            }

            HttpContext.Current.Response.Redirect(redirectUrl);

            return orderInfo;
		}
	}
}