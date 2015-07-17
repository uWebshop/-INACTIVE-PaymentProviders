using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using SagePay.IntegrationKit;
using SagePay.IntegrationKit.Messages;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.SagePay
{
	public class SagePayAPIIntegration : SagePayIntegration
	{
		static Random Random = new Random();

		public static string GetNewVendorTxCode()
		{
			var ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			// 18 char max -13 chars - 6 chars
			return string.Format("{0}-{1}-{2}",
				SagePaySettings.VendorName.Substring(0, Math.Min(18, SagePaySettings.VendorName.Length)),
				(long)ts.TotalMilliseconds, Random.Next(100000, 999999));
		}

		public static string GetNewRelatedVtx(string pref, string vtx)
		{
			var ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			return string.Format("{0}{1}-{2}{3}", pref, vtx.Substring(0, 15), (long)ts.TotalMilliseconds, RandomString(3));
		}

		public static string RandomString(int length)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < length; i++)
			{
				sb.Append(Convert.ToChar(Random.Next(65, 90)));
			}
			return sb.ToString();
		}
	}
	
    public class SagePayPaymentRequestHandler : IPaymentRequestHandler
    {
        private readonly IHttpRequestSender _requestSender;

        public SagePayPaymentRequestHandler()
        {
            _requestSender = new HttpRequestSender();
        }

        public SagePayPaymentRequestHandler(IHttpRequestSender requestSender = null)
        {
            _requestSender = requestSender ?? new HttpRequestSender();
        }

        public string GetName()
        {
            return "SagePay";
        }

		public string Crypt { get; set; }
        
        /// <summary>
        /// Executes the transaction request to SagePay
        /// </summary>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
        {
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);

		var sagePayDirectIntegration = new SagePayDirectIntegration();

			var request = sagePayDirectIntegration.DirectPaymentRequest();
			SetRequestData(request, orderInfo);

			var result = sagePayDirectIntegration.ProcessDirectPaymentRequest(request, SagePaySettings.DirectPaymentUrl);

			if (result.Status == ResponseStatus.OK || (SagePaySettings.DefaultTransactionType == TransactionType.AUTHENTICATE && result.Status == ResponseStatus.REGISTERED))
			{
				orderInfo.PaymentInfo.Parameters = result.Status + "&" + result.StatusDetail;

				PaymentProviderHelper.SetTransactionId(orderInfo, result.VpsTxId);
				//Response.Redirect(string.Format("Result.aspx?vendorTxCode={0}", request.VendorTxCode));
			}
			else if (result.Status == ResponseStatus.THREEDAUTH)
			{
				// todo: not supported yet! code below is work in progress
				//var threeDSecureRequest = new PaymentRequest();

				//threeDSecureRequest.Parameters.Add("paReq", result.PaReq.Replace(" ", "+"));
				//threeDSecureRequest.Parameters.Add("acsUrl", result.AcsUrl);
				//threeDSecureRequest.Parameters.Add("md", result.Md);
				//threeDSecureRequest.Parameters.Add("vendorTxCode", request.VendorTxCode);
				
				//// Save Order with Order status in 3D Process
				//var threeDAuth = new NameValueCollection
				//{
				//	{"paReq", result.PaReq.Replace(" ", "+")},
				//	{"acsUrl", result.AcsUrl},
				//	{"md", result.Md},
				//	{"vendorTxCode", request.VendorTxCode}
				//};
				//if (HttpContext.Current.Session["USER_3DAUTH"] != null)
				//{
				//	HttpContext.Current.Session["USER_3DAUTH"] = null;
				//}
				//HttpContext.Current.Session.Add("USER_3DAUTH", threeDAuth);
				
				////<form action="<%= ACSUrl %>" method="post">
				////<input type="hidden" name="PaReq" value="<%= PaReq %>" />
				////<input type="hidden" name="TermUrl" value="<%= TermUrl %>" />
				////<input type="hidden" name="MD" value="<%= MD %>" />
				////<input type="submit" value="Go" />
				////</form>

				//var responseString = _requestSender.SendRequest(result.AcsUrl, threeDSecureRequest.ParametersAsString);
				

				//var threeDAuthRequest = sagePayDirectIntegration.ThreeDAuthRequest();
				//threeDAuthRequest.Md = Request.Form["MD"];
				//threeDAuthRequest.PaRes = Request.Form["PaRes"];
				//var directPaymentResult = sagePayDirectIntegration.ProcessDirect3D(threeDAuthRequest);

				//if (directPaymentResult.Status == ResponseStatus.OK)
				//{
				//	orderInfo.Paid = true;
				//	orderInfo.Status = OrderStatus.ReadyForDispatch;
				//}
				//else
				//{

				//	Log.Instance.LogError("SagePay Did not return a proper directPaymentResult status: " + directPaymentResult.StatusDetail);
				//	PaymentProviderHelper.AddValidationResult(orderInfo, orderInfo.PaymentInfo.Id, "SagePayReturnedError", directPaymentResult.StatusDetail);
				//}


			}
			else
			{
				Log.Instance.LogError("SagePay Did not return a proper status: " + result.StatusDetail);
				PaymentProviderHelper.AddValidationResult(orderInfo, orderInfo.PaymentInfo.Id, "SagePayReturnedError", result.StatusDetail);
				
			}

         
			orderInfo.Save();

	        return new PaymentRequest();
        }

        private string GetSagePaySafeCustomerInfo(string value)
        {
            return string.IsNullOrEmpty(value) ? "hidden" : value;
        }

        public string GetPaymentUrl(OrderInfo orderInfo)
        {
            var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);

            if (!string.IsNullOrEmpty(orderInfo.PaymentInfo.Parameters))
            {
                var status = orderInfo.PaymentInfo.Parameters.Split('&')[0];

                return status.ToUpperInvariant() == "OK" ? paymentProvider.SuccessUrl() : paymentProvider.ErrorUrl();
            }

            return paymentProvider.ErrorUrl();
        }

		private void SetRequestData(IDirectPayment request, OrderInfo orderInfo)
		{
			 // API Call for easier access (but not able to write to)
            var basket = API.Basket.GetBasket(orderInfo.UniqueOrderId);

			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);

			var vendorName = paymentProvider.GetSetting("VendorName");
			
			request.VpsProtocol = SagePaySettings.ProtocolVersion;
			request.TransactionType = SagePaySettings.DefaultTransactionType;
			request.Vendor = SagePaySettings.VendorName;

			//Assign Vendor tx Code.
			request.VendorTxCode = SagePayAPIIntegration.GetNewVendorTxCode();
			
			request.Amount = orderInfo.ChargedAmount;
			request.Currency = new RegionInfo(orderInfo.StoreInfo.Store.CurrencyCultureInfo.LCID).ISOCurrencySymbol;
			request.Description = orderInfo.OrderNumber + " " + vendorName;
			request.CustomerEmail = GetSagePaySafeCustomerInfo(basket.Customer.Email);

			request.BillingSurname =GetSagePaySafeCustomerInfo(basket.Customer.LastName);
			request.BillingFirstnames = GetSagePaySafeCustomerInfo(basket.Customer.FirstName);
			request.BillingAddress1 = GetSagePaySafeCustomerInfo(basket.Customer.Address1);
			request.BillingPostCode = GetSagePaySafeCustomerInfo(basket.Customer.ZipCode);
			request.BillingCity = GetSagePaySafeCustomerInfo(basket.Customer.City);
			request.BillingCountry = string.IsNullOrEmpty(basket.Customer.Country) ? "GB" : basket.Customer.Country;

			request.DeliverySurname = GetSagePaySafeCustomerInfo(basket.Customer.Shipping.LastName);
			request.DeliveryFirstnames = GetSagePaySafeCustomerInfo(basket.Customer.Shipping.FirstName);
			request.DeliveryAddress1 = GetSagePaySafeCustomerInfo(basket.Customer.Shipping.Address1);
			request.DeliveryPostCode = GetSagePaySafeCustomerInfo(basket.Customer.Shipping.ZipCode);
			request.DeliveryCity = GetSagePaySafeCustomerInfo(basket.Customer.Shipping.City);
			request.DeliveryCountry = string.IsNullOrEmpty(basket.Customer.Shipping.Country) ? "GB" : basket.Customer.Shipping.Country;


			var useToken = false;
			bool.TryParse(HttpContext.Current.Request["useToken"], out useToken);


			
			var cardTypeMethod = orderInfo.PaymentInfo.MethodId;
			var creditCardNumber = string.Empty;

			if (paymentProvider.TestMode)
			{
				switch (cardTypeMethod.ToUpperInvariant())
				{
					case "VISA":
						creditCardNumber = "4929000000006";
						break;
					case "MC":
						creditCardNumber = "5404000000000001";
						break;
					case "MCDEBIT":
						creditCardNumber = "5573470000000001";
						break;
					case "DELTA":
						creditCardNumber = "4462000000000003";
						break;
					case "MAESTRO":
						creditCardNumber = "5641820000000005";
						break;
					case "UKE":
						creditCardNumber = "4917300000000008";
						break;
					case "AMEX":
						creditCardNumber = "374200000000004";
						break;
					case "DINERS":
						creditCardNumber = "36000000000008";
						break;
					case "DC":
						creditCardNumber = "4929000000006";
						break;
					case "JCB":
						creditCardNumber = "3569990000000009";
						break;
					case "LASER":
						creditCardNumber = "6304990000000000044";
						break;
				}
			}
			else
			{
				creditCardNumber = HttpContext.Current.Request["cardNumber"];
			}

			// uWebshop does not store card data, therefore the data is taken from the current Request
			if (useToken == false)
			{
				request.CardType = (CardType)Enum.Parse(typeof(CardType), cardTypeMethod);
				request.CardHolder = paymentProvider.TestMode ? "T.Ester" : HttpContext.Current.Request["cardHolder"];
				request.CardNumber = creditCardNumber;
				//request.StartDate = HttpContext.Current.Request["cardStartDate"];
				request.ExpiryDate = paymentProvider.TestMode ? DateTime.Now.AddMonths(6).ToString("MMyy") : HttpContext.Current.Request["cardExpiryDate"];
				request.Cv2 = paymentProvider.TestMode ? "123" : HttpContext.Current.Request["cardCV2"]; ;
			}
			else
			{
				request.Token = HttpContext.Current.Request["cardToken"];
				request.Cv2 = paymentProvider.TestMode ? "123" : HttpContext.Current.Request["cardCV2"]; ;
				request.StoreToken = 1;
			}
		}
    }
}
