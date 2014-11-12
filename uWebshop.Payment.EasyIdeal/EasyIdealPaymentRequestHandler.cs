using System;
using System.Globalization;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Web;

namespace uWebshop.Payment.EasyIdeal
{
	/// <summary>
	/// Create request to Payment Provider API
	/// </summary>
	public class EasyIdealPaymentRequestHandler : EasyIdealPaymentBase, IPaymentRequestHandler
	{
		/// <summary>
		/// Creates a payment request for this payment provider
		/// </summary>
		/// <param name="orderInfo"> </param>
		/// <returns>Payment request</returns>
		public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
		{
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);

			#region build urls

			var returnUrl = paymentProvider.SuccessUrl();

			var reportUrl = paymentProvider.ReportUrl();

            var testMode = paymentProvider.TestMode;  //currently we don't have a testmode for easy-ideal this var is unused
            
			#endregion

			#region config helper

            var merchantId = paymentProvider.GetSetting("merchantId");

            var merchantKey = paymentProvider.GetSetting("merchantKey");

            var merchantSecret = paymentProvider.GetSetting("merchantSecret");

            var url = paymentProvider.GetSetting("url");

			#endregion

            var args = new SortedList<string, string>();
            var ci = new CultureInfo("en-US");
            args.Add("Amount", orderInfo.ChargedAmount.ToString("G", ci));
            args.Add("Currency", "EUR");
            args.Add("Bank", orderInfo.PaymentInfo.MethodId);
            args.Add("Description", orderInfo.OrderNumber);
            args.Add("Return", returnUrl);

            var xmlRequest = getXML(IDEAL_EXECUTE, args, merchantId, merchantKey, merchantSecret);

            XDocument xmlResponse = XDocument.Parse(postXML(xmlRequest, url));

            var responseStatus = xmlResponse.Element("Response").Element("Status").FirstNode.ToString();

            var transactionId = xmlResponse.Element("Response").Element("Response").Element("TransactionID").FirstNode.ToString();
            var transactionCode = xmlResponse.Element("Response").Element("Response").Element("Code").FirstNode.ToString();
            var bankUrl = HttpUtility.HtmlDecode(xmlResponse.Element("Response").Element("Response").Element("BankURL").FirstNode.ToString());

            orderInfo.PaymentInfo.Url = bankUrl;

            PaymentProviderHelper.SetTransactionId(orderInfo, transactionId); //transactionCode hierin verwerken??

           // IO.Container.Resolve<IOrderUpdatingService>().AddCustomerFields(order, new Dictionary<string, string>({ "extraBilling", value }), CustomerDatatypes.Extra);
            orderInfo.AddCustomerFields(new Dictionary<string, string> { { "extraTransactionCode", transactionCode } }, Common.CustomerDatatypes.Extra);
            orderInfo.Save();

            return new PaymentRequest();
		}

		/// <summary>
		/// Returns the URL to redirect to
		/// </summary>
		/// <param name="orderInfo"> </param>
		/// <returns></returns>
		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			return orderInfo.PaymentInfo.Url;
		}
	}
}