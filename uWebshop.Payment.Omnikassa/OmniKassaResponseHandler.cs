using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Omnikassa
{
	public class OmniKassaResponseHandler : OmnikassaPaymentBase, IPaymentResponseHandler
	{
		public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo orderInfo)
		{
			var data = HttpContext.Current.Request["Data"];
			var seal = HttpContext.Current.Request["Seal"];

			Log.Instance.LogDebug("Omnikassa data: " + data + " Omnikassa seal: " + seal);

			var securityKey = paymentProvider.GetSetting("SecurityKey");
			
			// Verifieer de Seal
			var sha256 = SHA256.Create();
			var hashValue = sha256.ComputeHash(new UTF8Encoding().GetBytes(data + securityKey));

			if (seal.ToLower() == ByteArrayToHexString(hashValue).ToLower()) // Seal is goed
			{
				// Lees de gewenste waarden uit de server response
				var dataItems = ParseData(data);
				var transactionCode = dataItems.First(i => i.Key == "transactionReference").Value;
				var responseCode = dataItems.First(i => i.Key == "responseCode").Value;

				Log.Instance.LogDebug("Omnikassa transactionCode: " + transactionCode + " responseCode: " + responseCode);
				
				orderInfo = OrderHelper.GetOrder(transactionCode);

				Log.Instance.LogDebug("Omnikassa orderInfo: " + orderInfo.OrderNumber);

				var successUrl = paymentProvider.SuccessUrl();
				var cancelUrl = paymentProvider.ErrorUrl();
				
				var redirectUrl = successUrl;

				//Code	Beschrijving
				//00	Transaction success, authorization accepted (transactie gelukt, autorisatie geaccepteerd).
				//02	Please call the bank because the authorization limit on the card has been exceeded (neem contact op met de bank; de autorisatielimiet op de kaart is overschreden)
				//03	Invalid merchant contract (ongeldig contract webwinkel)
				//05	Do not honor, authorization refused (niet inwilligen, autorisatie geweigerd)
				//12	Invalid transaction, check the parameters sent in the request (ongeldige transactie, controleer de in het verzoek verzonden parameters).
				//14	Invalid card number or invalid Card Security Code or Card (for
				//MasterCard) or invalid Card Verification Value (for Visa/MAESTRO) (ongeldig kaartnummer of ongeldige beveiligingscode of kaart (voor MasterCard) of ongeldige waarde kaartverificatie (voor Visa/MAESTRO))
				//17	Cancellation of payment by the end user (betaling geannuleerd door eindgebruiker/klant)
				//24	Invalid status (ongeldige status).
				//25	Transaction not found in database (transactie niet gevonden in database)
				//30	Invalid format (ongeldig formaat)
				//34	Fraud suspicion (vermoeden van fraude)
				//40	Operation not allowed to this Merchant (handeling niet toegestaan voor deze webwinkel)
				//60	Pending transaction (transactie in behandeling)
				//63	Security breach detected, transaction stopped (beveiligingsprobleem gedetecteerd, transactie gestopt).
				//75	The number of attempts to enter the card number has been exceeded (three tries exhausted) (het aantal beschikbare pogingen om het card‐nummer in te geven is overschreden (max. drie))
				//90	Acquirer server temporarily unavailable (server acquirer tijdelijk onbeschikbaar)
				//94	Duplicate transaction (duplicaattransactie). (transactiereferentie al gereserveerd)
				//97	Request time‐out; transaction refused (time‐out voor verzoek; transactie geweigerd)
				//99	Payment page temporarily unavailable (betaalpagina tijdelijk niet beschikbaar)


				if (orderInfo.Paid == false)
				{
					switch (responseCode)
					{
						case "00":
							orderInfo.Paid = true;
							orderInfo.Status = OrderStatus.ReadyForDispatch;
							break;
						case "60":
						case "90":
							orderInfo.Paid = false;
							orderInfo.Status = OrderStatus.WaitingForPaymentProvider;
							break;
						case "17":
							orderInfo.Paid = false;
							orderInfo.Status = OrderStatus.PaymentFailed;
							orderInfo.PaymentInfo.ErrorMessage = "CANCELLED";
							redirectUrl = cancelUrl;
							break;
						case "EXPIRED":
							orderInfo.Paid = false;
							orderInfo.Status = OrderStatus.PaymentFailed;
							orderInfo.PaymentInfo.ErrorMessage = "EXPIRED";

							redirectUrl = cancelUrl;
							break;
						default:
							orderInfo.Paid = false;
							orderInfo.Status = OrderStatus.WaitingForPaymentProvider;

							redirectUrl = cancelUrl;
							break;
					}

					orderInfo.Save();
				}

				HttpContext.Current.Response.Redirect(redirectUrl);
			}
			else
			{
				throw new Exception("SEAL BROKEN");
			}

			return orderInfo;
		}

		private List<KeyValuePair<String, String>> ParseData(String data)
		{
			return (from part in data.Split('|') let key = part.Split('=')[0] let value = part.Split('=')[1] select new KeyValuePair<String, String>(key, value)).ToList();
		}

		// Converteer een String naar Hexadecimale waarde
		public String ByteArrayToHexString(byte[] bytes)
		{
			var result = new StringBuilder(bytes.Length*2);
			const String hexAlphabet = "0123456789ABCDEF";

			foreach (var b in bytes)
			{
				result.Append(hexAlphabet[b >> 4]);
				result.Append(hexAlphabet[b & 0xF]);
			}

			return result.ToString();
		}
	}
}