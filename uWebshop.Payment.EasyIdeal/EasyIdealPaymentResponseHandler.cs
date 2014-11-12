using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Web;
using System.Web.Configuration;
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

            #endregion
            try
            {
                var transactionId = HttpContext.Current.Request["id"];
                var paymentStatus = HttpContext.Current.Request["status"];
                var salt = HttpContext.Current.Request["salt"];
                var checksum = HttpContext.Current.Request["checksum"];

                if (string.IsNullOrEmpty(transactionId))
                {
                    Log.Instance.LogError("Easy iDeal response: TransactionId IsNullOrEmpty");
                    return null;
                }

                if (orderInfo == null)
                {
                    Log.Instance.LogError("Easy iDeal response: Order Not Found For TransactionId: " + transactionId);
                    return null;
                }

                if (orderInfo.Paid != false)
                {
                    Log.Instance.LogDebug("Easy iDeal response: Order already paid! TransactionId: " + transactionId);
                    return null;
                }

                var transactionCode = OrderHelper.ExtraInformationValue(orderInfo, "extraTransactionCode");

                if (string.IsNullOrEmpty(transactionCode))
                {
                    Log.Instance.LogDebug("Easy iDeal response: OrderTransactionCode IsNullOrEmpty ");
                    return null;
                }

                //check validity of request
                if (checkChecksumPaymentStatus(transactionId, transactionCode, paymentStatus, salt, checksum)) //only check for payment status if request is valid.
                    //This is a bit redundant, since you allready now the paymentStatus. But we could choose to do something with the extra info you get from this request e.g. IBAN
                {

                    var args = new SortedList<string, string>();
                    args.Add("TransactionID", transactionId);
                    args.Add("TransactionCode", transactionCode);


                    var xmlRequest = getXML(TRANSACTIONSTATUS, args, merchantId, merchantKey, merchantSecret);

                    XDocument xmlResponse = XDocument.Parse(postXML(xmlRequest, url));

                    var responseStatus = xmlResponse.Element("Response").Element("Status").FirstNode.ToString();

                    if (responseStatus == "OK")
                    {
                        if (xmlResponse.Element("Response").Element("Transaction").Element("Paid").FirstNode.ToString() == "Y")
                        {
                            orderInfo.Paid = true;
                            orderInfo.Status = OrderStatus.ReadyForDispatch;
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

            return orderInfo;
		}
	}
}