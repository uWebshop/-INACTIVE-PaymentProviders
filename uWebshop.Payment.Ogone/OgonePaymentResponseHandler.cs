using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Ogone
{
	public class OgonePaymentResponseHandler : OgonePaymentBase, IPaymentResponseHandler
	{
		public OrderInfo HandlePaymentResponse(PaymentProvider paymentProvider, OrderInfo orderInfo)
		{
			// Ogone POSTS some values
			var transactionId = HttpContext.Current.Request["TransactionId"];
			var status = HttpContext.Current.Request["STATUS"];

			if (string.IsNullOrEmpty(transactionId))
			{
				Log.Instance.LogError("Ogone TransactionId not Found!");
			    return null;
			}
			
			orderInfo = OrderHelper.GetOrder(transactionId);

			Log.Instance.LogDebug("OGONE OrderNumber: " + orderInfo.OrderNumber + " TransactionID: " + transactionId + " Status: " + status);

			var localizedPaymentProvider = PaymentProvider.GetPaymentProvider(orderInfo.PaymentInfo.Id, orderInfo.StoreInfo.Alias);

			var returnUrl = localizedPaymentProvider.SuccessUrl();
			var cancelUrl = localizedPaymentProvider.ErrorUrl();
			var redirectUrl = returnUrl;

			//0	Ongeldig of onvolledig
			//1	Geannuleerd door de klant
			//2	Autorisatie geweigerd
			//4	Opgeslagen bestelling
			//40	
			//41	Wachten op klantbetaling
			//5	Geautoriseerd
			//50	
			//51	Autorisatie pending
			//52	Autorisatie onzeker
			//55	Stand-by
			//56	OK met geplande betalingen
			//57	
			//59	Manueel te bekomen autorisatie
			//6	Geautoriseerd en geannuleerd
			//61	Annul. autor. pending
			//62	Annul. autor. onzeker
			//63	Annul. autor. geweigerd
			//64	Geautoriseerd en geannuleerd
			//7	Betaling geannuleerd
			//71	Annul.betaling pending
			//72	Annul. betaling onzeker
			//73	Annul betaling geweigerd
			//74	Betaling geannuleerd
			//75	Annul. betaling verwerkt door merch
			//8	Terugbetaald
			//81	Terugbetaling pending
			//82	Terugbetaling onzeker
			//83	Terugbetaling geweigerd
			//84	Betaling geweigerd door de bank
			//85	Terugbet. verwerkt door merchant
			//9	Betaling aangevraagd
			//91	Betaling pending
			//92	Betaling onzeker
			//93	Betaling geweigerd
			//94	Terubetaling geweigerd door de bank
			//95	Betaling verwerkt door merchant
			//99	Wordt verwerkt

			if (orderInfo.Paid == false)
			{
				switch (status)
				{
					case "5":
					case "9":
						orderInfo.Paid = true;
						orderInfo.Status = OrderStatus.ReadyForDispatch;
						break;
					case "0":
					case "1":
					case "2":
					case "61":
					case "62":
					case "63":
					case "71":
					case "72":
					case "73":
					case "74":
					case "75":
					case "93":
						orderInfo.Paid = false;
						orderInfo.Status = OrderStatus.PaymentFailed;
						orderInfo.PaymentInfo.ErrorMessage = status;
						redirectUrl = cancelUrl;
						break;
					default:
						orderInfo.Status = OrderStatus.WaitingForPaymentProvider;
						break;
				}

				orderInfo.Save();
			}
            
			HttpContext.Current.Response.Redirect(redirectUrl);

            return orderInfo;
		}
	}
}