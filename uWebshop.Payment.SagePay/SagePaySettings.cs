using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using SagePay.IntegrationKit;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;

namespace uWebshop.Payment.SagePay
{
	public static class SagePaySettings
	{
		public static string Environment
		{
			get
			{
				List<string> environmentTypes = new List<string>() { "LIVE", "TEST" };

				if (!environmentTypes.Contains(GetValue("sagepay.api.env")))
				{
					throw new ArgumentOutOfRangeException("sagepay.api.env", "The specified environment property does not match an acceptable value");
				}

				return GetValue("sagepay.api.env");
			}
		}

		public static string ConnectionString
		{
			get
			{
				return GetValue("sagepay.api.connectionString");
			}
		}
		public static ProtocolVersion ProtocolVersion
		{
			get
			{
				return (ProtocolVersion)Enum.Parse(typeof(ProtocolVersion), "V_" + GetValue("sagepay.api.protocolVersion").Replace(".", ""));
			}
		}

		public static bool EnableClientValidation
		{
			get
			{
				return GetBooleanProperty("sagepay.api.enableClientValidation");
			}
		}

		public static string VendorName
		{
			get
			{
				return GetValue("sagepay.kit.vendorName");
			}
		}

		public static string FullUrl
		{
			get
			{
				return GetValue("sagepay.kit.fullUrl");
			}
		}

		public static string Currency
		{
			get
			{
				return GetValue("sagepay.kit.currency");
			}
		}

		public static TransactionType DefaultTransactionType
		{
			get
			{
				return (TransactionType)Enum.Parse(typeof(TransactionType), GetValue("sagepay.kit.defaultTransactionType"));
			}
		}

		public static string SiteFqdn
		{
			get
			{
				string SiteFqdn = GetValue("sagepay.kit.siteFqdn." + Environment);

				if (string.IsNullOrEmpty(SiteFqdn))
				{
					SiteFqdn = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
				}

				if (SiteFqdn.EndsWith("/"))
				{
					SiteFqdn = SiteFqdn.Remove(SiteFqdn.Length - 1);
				}

				// Resolves the virtual path used to deploy app
				SiteFqdn += HttpRuntime.AppDomainAppVirtualPath + "/";

				return SiteFqdn;
			}
		}

		public static int ApplyAvsCv2
		{
			get
			{
				return Convert.ToInt16(GetValue("sagepay.kit.applyAvsCv2"));
			}
		}

		public static int Apply3dSecure
		{
			get
			{
				return Convert.ToInt16(GetValue("sagepay.kit.apply3dSecure"));
			}
		}

		public static bool IsBasketXMLDisabled
		{
			get
			{
				return GetBooleanProperty("sagepay.kit.basketxml.disable");
			}
		}

		public static bool IsCollectRecipientDetails
		{
			get
			{
				return GetBooleanProperty("sagepay.kit.collectRecipientDetails");
			}
		}

		public static int AllowGiftAid
		{
			get
			{
				return Convert.ToInt16(GetValue("sagepay.kit.allowGiftAid"));
			}
		}

		public static string EncryptionPassword
		{
			get
			{
				return GetValue("sagepay.kit.form.encryptionPassword." + Environment);
			}
		}

		public static int SendEmail
		{
			get
			{
				return Convert.ToInt16(GetValue("sagepay.kit.form.sendEmail"));
			}
		}
		public static string VendorEmail
		{
			get
			{
				return GetValue("sagepay.kit.form.vendorEmail");
			}
		}
		public static string EmailMessage
		{
			get
			{
				return GetValue("sagepay.kit.form.emailMessage");
			}
		}

		public static string AccountType
		{
			get
			{
				return GetValue("sagepay.kit.accountType");
			}
		}
		public static string ReferrerID
		{
			get
			{
				return GetValue("sagepay.kit.partnerID");
			}
		}

		public static string SurchargeXML
		{
			get
			{
				return HttpUtility.HtmlDecode(GetValue("sagepay.kit.surchargeXML"));
			}
		}

		public static string FormPaymentUrl
		{
			get
			{
				return GetValue("sagepay.api.formPaymentUrl." + Environment);
			}
		}

		public static string ServerPaymentUrl
		{
			get
			{
				return GetValue("sagepay.api.serverPaymentUrl." + Environment);
			}
		}

		public static string ServerTokenRegisterUrl
		{
			get
			{
				return GetValue("sagepay.api.serverTokenRegisterUrl." + Environment);
			}
		}
		public static string DirectPayPalCompleteUrl
		{
			get
			{
				return GetValue("sagepay.api.directPayPalCompleteUrl." + Environment);
			}
		}

		public static string DirectPaymentUrl
		{
			get
			{
				return GetValue("sagepay.api.directPaymentUrl." + Environment);
			}
		}

		public static string DirectTokenRegisterUrl
		{
			get
			{
				return GetValue("sagepay.api.directTokenRegisterUrl." + Environment);
			}
		}

		public static string VoidUrl
		{
			get
			{
				return GetValue("sagepay.api.voidUrl." + Environment);
			}
		}

		public static string AbortUrl
		{
			get
			{
				return GetValue("sagepay.api.abortUrl." + Environment);
			}
		}

		public static string CancelUrl
		{
			get
			{
				return GetValue("sagepay.api.cancelUrl." + Environment);
			}
		}

		public static string ReleaseUrl
		{
			get
			{
				return GetValue("sagepay.api.releaseUrl." + Environment);
			}
		}

		public static string RefundUrl
		{
			get
			{
				return GetValue("sagepay.api.refundUrl." + Environment);
			}
		}
		public static string RepeatUrl
		{
			get
			{
				return GetValue("sagepay.api.repeatUrl." + Environment);
			}
		}
		public static string AuthoriseUrl
		{
			get
			{
				return GetValue("sagepay.api.authoriseUrl." + Environment);
			}
		}
		public static string TokenRemoveUrl
		{
			get
			{
				return GetValue("sagepay.api.tokenRemoveUrl." + Environment);
			}
		}

		public static string Direct3dSecureUrl
		{
			get
			{
				return GetValue("sagepay.api.direct3dSecureUrl." + Environment);
			}
		}


		// **** Helper Methods ****

		public static string GetValue(string key)
		{
			var basket = API.Basket.GetBasket();

			var paymentProvider = new PaymentProvider(basket.Payment.Providers.FirstOrDefault().Id);

			return paymentProvider.GetSetting(key);
			
		}

		public static string GetAccountTypeOrNull()
		{
			return (AccountType == string.Empty) ? null : AccountType;
		}

		public static bool CollectExtraInformation
		{
			get
			{
				return !IsBasketXMLDisabled || IsCollectRecipientDetails;
			}
		}

		private static bool GetBooleanProperty(String property)
		{
			bool booleanProperty = false;
			return Boolean.TryParse(GetValue(property), out booleanProperty) ? booleanProperty : false;
		}
	}


}
