using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using uWebshop.Common;
using uWebshop.DataAccess;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.NodeFactory;
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
			var paymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id);
			var helper = new PaymentConfigHelper(paymentProvider);

			#region config helper

			var vendorName = helper.Settings["VendorName"];
			if (string.IsNullOrEmpty(vendorName))
			{
				Log.Instance.LogError("SagePay: Missing PaymentProvider.Config  field with name: VendorName, paymentProviderNodeId: " + paymentProvider.Id);
			}

			var directTestUrl = helper.Settings["DirectTestURL"];
			if (string.IsNullOrEmpty(directTestUrl))
			{
				Log.Instance.LogError("SagePay: Missing PaymentProvider.Config  field with name: DirectTestURL, paymentProviderNodeId: " + paymentProvider.Id);
			}

			var directUrl = helper.Settings["DirectUrl"];
			if (string.IsNullOrEmpty(directUrl))
			{
				Log.Instance.LogError("SagePay: Missing PaymentProvider.Config  field with name: DirectUrl, paymentProviderNodeId: " + paymentProvider.Id);
			}

			#endregion

			var baseUrl = PaymentProviderHelper.GenerateBaseUrl(Node.GetCurrent().Id);
			var reportUrl = string.Format("{0}{1}", baseUrl, library.NiceUrl(paymentProvider.Id));

			var request = new PaymentRequest();

			var cardTypeMethod = new Node(int.Parse(orderInfo.PaymentInfo.MethodId));

			request.Parameters.Add("CardType", cardTypeMethod.Name);
			request.Parameters.Add("CardHolder", HttpContext.Current.Request["CardHolder"]);
			request.Parameters.Add("CardNumber", HttpContext.Current.Request["CardNumber"]); // test visa = 4929000000006
			request.Parameters.Add("CV2", HttpContext.Current.Request["CV2"]);
			request.Parameters.Add("ExpiryDate", HttpContext.Current.Request["ExpiryDate"]);

			request.Parameters.Add("VPSProtocol", "2.23");
			request.Parameters.Add("TxType", "PAYMENT");
			request.Parameters.Add("Vendor", vendorName);
			request.Parameters.Add("VendorTxCode", orderInfo.UniqueOrderId.ToString());

			request.Parameters.Add("Amount", orderInfo.ChargedAmount.ToString(new CultureInfo("en-GB").NumberFormat));
			request.Parameters.Add("Currency", new RegionInfo(orderInfo.StoreInfo.Store.CurrencyCultureInfo.LCID).ISOCurrencySymbol);

			request.Parameters.Add("Description", orderInfo.OrderNumber); // beschrijving van gekochte producten

			request.Parameters.Add("NotificationURL", reportUrl);

			request.Parameters.Add("BillingSurname", GetSagePaySafeCustomerInfo(orderInfo.CustomerLastName));
			request.Parameters.Add("BillingFirstnames", GetSagePaySafeCustomerInfo(orderInfo.CustomerFirstName));
			request.Parameters.Add("BillingAddress1", "hidden");
			request.Parameters.Add("BillingCity", "hidden");
			request.Parameters.Add("BillingPostCode", "hidden");
			request.Parameters.Add("BillingCountry", GetSagePaySafeCustomerInfo(orderInfo.CustomerInfo.CountryCode)); // ISO 3166-1

			request.Parameters.Add("DeliverySurname", GetSagePaySafeCustomerInfo(orderInfo.CustomerLastName));
			request.Parameters.Add("DeliveryFirstnames", GetSagePaySafeCustomerInfo(orderInfo.CustomerFirstName));
			request.Parameters.Add("DeliveryAddress1", "hidden");
			request.Parameters.Add("DeliveryCity", "hidden");
			request.Parameters.Add("DeliveryPostCode", "hidden");
			request.Parameters.Add("DeliveryCountry", GetSagePaySafeCustomerInfo(orderInfo.CustomerInfo.CountryCode));

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
			// request.Parameters.Add("Profile", ""); not in official spec!

			var sagePayUrl = paymentProvider.TestMode ? directTestUrl : directUrl;
			var responseString = _requestSender.SendRequest(sagePayUrl, request.ParametersAsString);

			var deserializer = new ResponseSerializer();
			var response = deserializer.Deserialize<TransactionRegistrationResponse>(responseString);

			if (!(response.Status == ResponseType.Ok || response.Status == ResponseType.Registered))
				throw new Exception("SagePay transaction failed with status " + response.Status + ". " + response.StatusDetail);

			orderInfo.PaymentInfo.Url = response.NextURL; //var decodeUrl = Uri.UnescapeDataString(url.Value);
			orderInfo.PaymentInfo.TransactionId = response.VPSTxId; // trxId.Value;

			Log.Instance.LogDebug("SagePay PaymentTransactionMethod nextURL: " + response.NextURL);

			uWebshopOrders.SetTransactionId(orderInfo.UniqueOrderId, response.VPSTxId);

			orderInfo.Save();

			return null;
		}

		private string GetSagePaySafeCustomerInfo(string value)
		{
			return string.IsNullOrEmpty(value) ? "-" : value;
		}

		public string GetPaymentUrl(OrderInfo orderInfo)
		{
			if (!string.IsNullOrEmpty(orderInfo.PaymentInfo.Url))
			{
				return orderInfo.PaymentInfo.Url;
			}

			return library.NiceUrl(int.Parse(PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id).SuccesNodeId));
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
				Deserialize(typeof (T), input, objectToDeserializeInto);
			}

			/// <summary>
			/// Deserializes the response into an object of type T.
			/// </summary>
			public T Deserialize<T>(string input) where T : new()
			{
				var instance = new T();
				Deserialize(typeof (T), input, instance);
				return instance;
			}

			/// <summary>
			/// Deserializes the response into an object of the specified type.
			/// </summary>
			private void Deserialize(Type type, string input, object objectToDeserializeInto)
			{
				if (string.IsNullOrEmpty(input)) return;

				var bits = input.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

				foreach (var nameValuePairCombined in bits)
				{
					var index = nameValuePairCombined.IndexOf('=');
					var name = nameValuePairCombined.Substring(0, index);
					var value = nameValuePairCombined.Substring(index + 1);

					if (name == "3DSecureStatus") name = "ThreeDSecureStatus";

					var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

					if (prop == null)
					{
						throw new InvalidOperationException(string.Format("Could not find a property on Type '{0}' named '{1}'", type.Name, name));
					}

					//TODO: Investigate building a method of defining custom serializers

					object convertedValue = prop.PropertyType == typeof (ResponseType) ? ConvertStringToSagePayResponseType(value) : Convert.ChangeType(value, prop.PropertyType);

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