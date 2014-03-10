using System.Web;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Buckaroo
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;

    using uWebshop.API;

    public class BuckarooPaymentResponseHandler : BuckarooPaymentBase, IPaymentResponseHandler
	{
		public string HandlePaymentResponse(PaymentProvider paymentProvider)
		{
		    var httpRequest = HttpContext.Current.Request;
            
            Dictionary<string, string> requestParams = new Dictionary<string, string>();
		    NameValueCollection httpRequestParams = httpRequest.HttpMethod == "POST"
		                                                ? httpRequest.Form
		                                                : httpRequest.QueryString;

		    foreach (string key in httpRequestParams)
		    {
		        requestParams.Add(key, httpRequest[key]);
		    }

            BuckarooResponseParameters buckarooParams = new BuckarooResponseParameters(paymentProvider.GetSetting("SecretKey"), requestParams);

		    if (buckarooParams.IsValid())
		    {
                HttpContext.Current.Response.Redirect(paymentProvider.ErrorUrl());
		    }

            string signatureFromBuckaroo = buckarooParams.Signature;
            string calculatedSignature = buckarooParams.GetSignature();

            if (string.Compare(signatureFromBuckaroo, calculatedSignature) != 0)
            {
                HttpContext.Current.Response.Redirect(paymentProvider.ErrorUrl());
            }

			Log.Instance.LogError("Buckaroo returned transactionId: " + buckarooParams.TransactionId + " resultCode: " + buckarooParams.StatusCode + " currency:" + buckarooParams.Currency + " amount: " + buckarooParams.Amount);
			
			if (string.IsNullOrEmpty(buckarooParams.TransactionId))
			{
				HttpContext.Current.Response.Redirect(paymentProvider.ErrorUrl());
			}

            var order = OrderHelper.GetOrder(buckarooParams.TransactionId);

			var localizedPaymentProvider = PaymentProvider.GetPaymentProvider(paymentProvider.Id, order.StoreInfo.Alias);

			var succesUrl = localizedPaymentProvider.SuccessUrl();
			var failUrl = localizedPaymentProvider.ErrorUrl();

			var redirectUrl = succesUrl;

			if (order.Paid == true)
			{
                Log.Instance.LogError("Buckaroo Already PAID returned transactionId: " + buckarooParams.TransactionId + " resultCode: " + buckarooParams.StatusCode + " currency:" + buckarooParams.Currency + " amount: " + buckarooParams.Amount);
				HttpContext.Current.Response.Redirect(succesUrl);
				HttpContext.Current.Response.End();
			}
			else
			{
				redirectUrl = failUrl;
			}

			//190	 Succes: De transactie is geslaagd en de betaling is ontvangen / goedgekeurd.	 Definitief	 Ja
			//490	 Mislukt: De transactie is mislukt.	 Definitief	 Nee
			//491	 Validatie mislukt: De transactie verzoek bevatte fouten en kon niet goed verwerkt worden	 Definitief	 Nee
			//492	 Technische storing: Door een technische storing kon de transactie niet worden afgerond
			//Definitief	 Nee
			//690	 Afgekeurd: De transactie is afgewezen door de (derde) payment provider.	 Definitief	 Nee
			//790	 In afwachting van invoer: De transactie is in de wacht, terwijl de payment enginge staat te wachten
			//op de inbreng van de consument.	 Tijdelijk	 Nee
			//791	 In afwachting van verwerking: De transactie wordt verwerkt.	 Tijdelijk	 Nee
			//792	 In afwachting van de consument: de betaling Engine wacht voor de consument om terug te keren van
			//een website van derden, die nodig is om de transactie te voltooien.	 Tijdelijk	 Nee
			//793	 De transactie is onhold.
			//Tijdelijk	 Nee
			//890	 Geannuleerd door Gebruiker: De transactie is geannuleerd door de klant.
			//Definitief	 Nee
			//891	 Geannuleerd door Merchant: De merchant heeft de transactie geannuleerd.	 Definitief	 Nee

			switch (buckarooParams.StatusCode)
			{
				case "190":
					order.Paid = true;
					order.Status = OrderStatus.ReadyForDispatch;
					redirectUrl = succesUrl;
					break;
				case "490":
					order.Paid = false;
					order.Status = OrderStatus.PaymentFailed;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Transaction Failed (490) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "491":
					order.Paid = false;
					order.Status = OrderStatus.PaymentFailed;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Validation Failed (491) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "492":
					order.Paid = false;
					order.Status = OrderStatus.PaymentFailed;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Technical Failure (492) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "690":
					order.Paid = false;
					order.Status = OrderStatus.PaymentFailed;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Payment Denied by 3rd Party (690) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "790":
					order.Paid = false;
					order.Status = OrderStatus.WaitingForPayment;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Waiting for customer input (790) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "791":
					order.Paid = false;
					order.Status = OrderStatus.WaitingForPayment;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Waiting for transaction handling (791) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "792":
					order.Paid = false;
					order.Status = OrderStatus.WaitingForPayment;                    
					redirectUrl = failUrl;
			        Log.Instance.LogError("Buckaroo ResponseHandler Waiting for customer to return from 3rd party website (792) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "793":
					order.Paid = false;
					order.Status = OrderStatus.WaitingForPayment;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Transaction is On Hold (793) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "890":
					order.Paid = false;
					order.Status = OrderStatus.WaitingForPayment;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Transaction Cancelled by Customer (890) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
				case "891":
					order.Paid = false;
					order.Status = OrderStatus.WaitingForPayment;
					redirectUrl = failUrl;
                    Log.Instance.LogError("Buckaroo ResponseHandler Transaction Cancelled by Merchant (891) for transactionId: " + buckarooParams.TransactionId + " Ordernumber: " + order.OrderNumber);
					break;
			}

			order.Save();
			

			HttpContext.Current.Response.Redirect(redirectUrl);

			return null;
		}
	}

}