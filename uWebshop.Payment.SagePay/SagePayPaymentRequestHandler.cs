using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.SagePay
{
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

        /// <summary>
        /// Executes the transaction request to SagePay
        /// </summary>
        /// <param name="orderInfo"></param>
        /// <returns></returns>
        public PaymentRequest CreatePaymentRequest(OrderInfo orderInfo)
        {
            // new API Call for easier access to date (but not able to write to)
            var orderAPI = API.Basket.GetBasket(orderInfo.UniqueOrderId);

            var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);
          
            #region config helper

            var vendorName = paymentProvider.GetSetting("VendorName");

            var liveUrl = "https://live.sagepay.com/gateway/service/vspdirect-register.vsp";
            var testUrl = "https://test.sagepay.com/Simulator/VSPDirectGateway.asp";

            var configLiveUrl = paymentProvider.GetSetting("Url");
            var configTestUrl = paymentProvider.GetSetting("testUrl");

            if (!string.IsNullOrEmpty(configLiveUrl))
            {
                liveUrl = configLiveUrl;
            }
            if (!string.IsNullOrEmpty(configTestUrl))
            {
                testUrl = configTestUrl;
            }

            #endregion

            var reportUrl = paymentProvider.ReportUrl();

            var request = new PaymentRequest();

            var cardTypeMethod = orderInfo.PaymentInfo.MethodId;

            // Get the fields from the current request, this way they won't be stored in the order or in uWebshop
            var creditCardHolderName = HttpContext.Current.Request["CardHolder"];
            var creditCardNumber = HttpContext.Current.Request["CardNumber"];
            var creditCardCV2 = HttpContext.Current.Request["CV2"];
            var creditCardExpiryDate = HttpContext.Current.Request["ExpiryDate"];

            if (string.IsNullOrEmpty(creditCardHolderName))
            {
                // if no creditCardHolder field is posted, try to see if the order contains an holder field
                creditCardHolderName = orderAPI.Customer.GetValue<string>("customerCardHolder");

                if (string.IsNullOrEmpty(creditCardHolderName))
                {
                    // if that is not present, use first letter of firstName and full last name
                    creditCardHolderName = orderInfo.CustomerFirstName.Substring(0, 1) + " " +
                                           orderInfo.CustomerLastName;
                }
            }

            if (string.IsNullOrEmpty(creditCardNumber))
            {
                // if no creditCardNumber field is posted, try to see if the order contains an creditcardnumber
                creditCardNumber = orderAPI.Customer.GetValue<string>("customerCardNumber");

                if (string.IsNullOrEmpty(creditCardNumber) && !paymentProvider.TestMode)
                {
                    Log.Instance.LogError("Sagepay CreatePaymentRequest: No Creditcardnumber Given");
                    throw new Exception("Sagepay CreatePaymentRequest: No Creditcardnumber Given");
                }
            }

            if (string.IsNullOrEmpty(creditCardCV2))
            {
                // if no creditCardNumber field is posted, try to see if the order contains an creditcardnumber
                creditCardCV2 = orderAPI.Customer.GetValue<string>("customerCardCV2");

                if (string.IsNullOrEmpty(creditCardCV2) && !paymentProvider.TestMode)
                {
                    Log.Instance.LogError("Sagepay CreatePaymentRequest: No CV2 Given");
                    return null;
                }
            }

            if (string.IsNullOrEmpty(creditCardExpiryDate))
            {
                // if no ExpiryDate field is posted, try to see if the order contains an expirydate
                creditCardExpiryDate = orderAPI.Customer.GetValue<string>("customerCardExpiryDate");

                if (string.IsNullOrEmpty(creditCardExpiryDate) && !paymentProvider.TestMode)
                {
                    Log.Instance.LogError("Sagepay CreatePaymentRequest: No creditCardExpiryDate Given");
                    return null;
                }
            }

            request.Parameters.Add("CardType", paymentProvider.TestMode ? "VISA" : cardTypeMethod);
            request.Parameters.Add("CardHolder", paymentProvider.TestMode ? "T.Ester" : creditCardHolderName);
            request.Parameters.Add("CardNumber", paymentProvider.TestMode ? "4929000000006" : creditCardNumber);
            request.Parameters.Add("CV2", paymentProvider.TestMode ? "123" : creditCardCV2);
            request.Parameters.Add("ExpiryDate", paymentProvider.TestMode ? DateTime.Now.AddMonths(6).ToString("MMyy") : creditCardExpiryDate);

            request.Parameters.Add("VPSProtocol", "2.23");
            request.Parameters.Add("TxType", "PAYMENT");
            request.Parameters.Add("Vendor", vendorName);

            var trasactionId = orderInfo.OrderNumber + "x" + DateTime.Now.ToString("hhmmss");

            request.Parameters.Add("VendorTxCode", trasactionId);

            request.Parameters.Add("Amount", orderInfo.ChargedAmount.ToString(new CultureInfo("en-GB").NumberFormat));
            request.Parameters.Add("Currency", new RegionInfo(orderInfo.StoreInfo.Store.CurrencyCultureInfo.LCID).ISOCurrencySymbol);

            request.Parameters.Add("Description", orderInfo.OrderNumber);

            request.Parameters.Add("NotificationURL", reportUrl);

            request.Parameters.Add("BillingSurname", GetSagePaySafeCustomerInfo(orderAPI.Customer.LastName));
            request.Parameters.Add("BillingFirstnames", GetSagePaySafeCustomerInfo(orderAPI.Customer.FirstName));
            request.Parameters.Add("BillingAddress1", GetSagePaySafeCustomerInfo(orderAPI.Customer.Address1));
            request.Parameters.Add("BillingCity", GetSagePaySafeCustomerInfo(orderAPI.Customer.City));
            request.Parameters.Add("BillingPostCode", GetSagePaySafeCustomerInfo(orderAPI.Customer.ZipCode));
            request.Parameters.Add("BillingCountry", GetSagePaySafeCustomerInfo(orderInfo.CustomerInfo.CountryCode));

            var deliverySurName = string.IsNullOrEmpty(orderAPI.Customer.Shipping.LastName) ? orderAPI.Customer.LastName : orderAPI.Customer.Shipping.LastName;
            var deliveryFirstName = string.IsNullOrEmpty(orderAPI.Customer.Shipping.FirstName) ? orderAPI.Customer.FirstName : orderAPI.Customer.Shipping.FirstName;
            var deliveryAddress1 = string.IsNullOrEmpty(orderAPI.Customer.Shipping.Address1) ? orderAPI.Customer.Address1 : orderAPI.Customer.Shipping.Address1;
            var deliveryPostcode = string.IsNullOrEmpty(orderAPI.Customer.Shipping.ZipCode) ? orderAPI.Customer.ZipCode : orderAPI.Customer.Shipping.ZipCode;
            var deliveryCity = string.IsNullOrEmpty(orderAPI.Customer.Shipping.City) ? orderAPI.Customer.City : orderAPI.Customer.Shipping.City;
            var deliveryCountry = string.IsNullOrEmpty(orderAPI.Customer.Shipping.CountryCode) ? orderAPI.Customer.CountryCode : orderAPI.Customer.Shipping.CountryCode;
            
            request.Parameters.Add("DeliverySurname", GetSagePaySafeCustomerInfo(deliverySurName));
            request.Parameters.Add("DeliveryFirstnames", GetSagePaySafeCustomerInfo(deliveryFirstName));
            request.Parameters.Add("DeliveryAddress1", GetSagePaySafeCustomerInfo(deliveryAddress1));
            request.Parameters.Add("DeliveryCity", GetSagePaySafeCustomerInfo(deliveryCity));
            request.Parameters.Add("DeliveryPostCode", GetSagePaySafeCustomerInfo(deliveryPostcode));
            request.Parameters.Add("DeliveryCountry", GetSagePaySafeCustomerInfo(deliveryCountry));

            // optionele parameters:
            //request.Parameters.Add("BillingAddress2", "");
            //request.Parameters.Add("BillingState", "");
            //request.Parameters.Add("BillingPhone", "");
            //request.Parameters.Add("DeliveryAddress2", "");
            //request.Parameters.Add("DeliveryState", "");
            //request.Parameters.Add("DeliveryPhone", "");
            //request.Parameters.Add("CustomerEMail", "");
            //request.Parameters.Add("Basket", ""); // optioneel info over basket
            //request.Parameters.Add("AllowGiftAid", "0");
            //request.Parameters.Add("Apply3DSecure", "0");

            var sagePayUrl = paymentProvider.TestMode ? testUrl : liveUrl;
            var responseString = _requestSender.SendRequest(sagePayUrl, request.ParametersAsString);

            var deserializer = new ResponseSerializer();
            var response = deserializer.Deserialize<TransactionRegistrationResponse>(responseString);
            
            orderInfo.PaymentInfo.Parameters = response.Status + "&" + response.StatusDetail;
            PaymentProviderHelper.SetTransactionId(orderInfo, response.VPSTxId);

            orderInfo.Save();

            return null;
        }

        private string GetSagePaySafeCustomerInfo(string value)
        {
            return string.IsNullOrEmpty(value) ? "hidden" : value;
        }

        public string GetPaymentUrl(OrderInfo orderInfo)
        {
            var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);
            
            var status = orderInfo.PaymentInfo.Parameters.Split('&')[0];
          
            return status.ToUpperInvariant() == "OK" ? paymentProvider.SuccessUrl() : paymentProvider.ErrorUrl();
        }

        private class TransactionRegistrationResponse
        {
            /// <summary>
            /// Protocol version
            /// </summary>
            public string VPSProtocol { get; set; }

            /// <summary>
            /// Status
            /// </summary>
            public ResponseType Status { get; set; }

            /// <summary>
            /// Additional status details
            /// </summary>
            public string StatusDetail { get; set; }

            /// <summary>
            /// Transaction ID generated by SagePay
            /// </summary>
            public string VPSTxId { get; set; }

            /// <summary>
            /// Security Key
            /// </summary>
            public string SecurityKey { get; set; }

            /// <summary>
            /// Redirect URL
            /// </summary>
            public string NextURL { get; set; }

            public string TxAuthNo { get; set; }
            public string AVSCV2 { get; set; }
            public string AddressResult { get; set; }
            public string PostCodeResult { get; set; }
            public string CV2Result { get; set; }
            public string ThreeDSecureStatus { get; set; }
            public string CAVV { get; set; }
        }

        private enum ResponseType
        {
            Unknown,
            Ok,
            NotAuthed,
            Abort,
            Rejected,
            Authenticated,
            Registered,
            Malformed,
            Error,
            Invalid,
        }

        private class ResponseSerializer
        {
            /// <summary>
            /// Deserializes the response into an instance of type T.
            /// </summary>
            public void Deserialize<T>(string input, T objectToDeserializeInto)
            {
                Deserialize(typeof(T), input, objectToDeserializeInto);
            }

            /// <summary>
            /// Deserializes the response into an object of type T.
            /// </summary>
            public T Deserialize<T>(string input) where T : new()
            {
                var instance = new T();
                Deserialize(typeof(T), input, instance);
                return instance;
            }

            /// <summary>
            /// Deserializes the response into an object of the specified type.
            /// </summary>
            private void Deserialize(Type type, string input, object objectToDeserializeInto)
            {
                if (string.IsNullOrEmpty(input)) return;

                var bits = input.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var nameValuePairCombined in bits)
                {
                    var index = nameValuePairCombined.IndexOf('=');
                    var name = nameValuePairCombined.Substring(0, index);
                    var value = nameValuePairCombined.Substring(index + 1);

                    if (name == "3DSecureStatus") name = "ThreeDSecureStatus";

                    var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

                    if (prop == null)
                    {
                        Log.Instance.LogError("SagePay Payment Provider Error: " + string.Format("Could not find a property on Type '{0}' named '{1}'", type.Name, name));
                        throw new InvalidOperationException(string.Format("Could not find a property on Type '{0}' named '{1}'", type.Name, name));
                    }

                    //TODO: Investigate building a method of defining custom serializers

                    object convertedValue = prop.PropertyType == typeof(ResponseType) ? ConvertStringToSagePayResponseType(value) : Convert.ChangeType(value, prop.PropertyType);

                    prop.SetValue(objectToDeserializeInto, convertedValue, null);
                }
            }

            /// <summary>
            /// Deserializes the response into an object of the specified type.
            /// </summary>
            public object Deserialize(Type type, string input)
            {
                var instance = Activator.CreateInstance(type);
                Deserialize(type, input, instance);
                return instance;
            }

            /// <summary>
            /// Utility method for converting a string into a ResponseType. 
            /// </summary>
            private static ResponseType ConvertStringToSagePayResponseType(string input)
            {
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.StartsWith("OK"))
                    {
                        return ResponseType.Ok;
                    }

                    if (input.StartsWith("NOTAUTHED"))
                    {
                        return ResponseType.NotAuthed;
                    }

                    if (input.StartsWith("ABORT"))
                    {
                        return ResponseType.Abort;
                    }

                    if (input.StartsWith("REJECTED"))
                    {
                        return ResponseType.Rejected;
                    }

                    if (input.StartsWith("MALFORMED"))
                    {
                        return ResponseType.Malformed;
                    }

                    if (input.StartsWith("AUTHENTICATED"))
                    {
                        return ResponseType.Authenticated;
                    }

                    if (input.StartsWith("INVALID"))
                    {
                        return ResponseType.Invalid;
                    }

                    if (input.StartsWith("REGISTERED"))
                    {
                        return ResponseType.Registered;
                    }

                    if (input.StartsWith("ERROR"))
                    {
                        return ResponseType.Error;
                    }
                }
                return ResponseType.Unknown;
            }
        }
    }
}