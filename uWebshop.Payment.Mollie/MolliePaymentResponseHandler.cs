using System;
using System.Web;
using Mollie.iDEAL;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Mollie
{
	public class MolliePaymentResponseHandler : MolliePaymentBase, IPaymentResponseHandler
	{
		public string HandlePaymentResponse(PaymentProvider paymentProvider)
		{
			try
			{
				var transactionId = HttpContext.Current.Request["transaction_id"];

				Log.Instance.LogDebug("Mollie Transaction Id: " + transactionId);

				if (string.IsNullOrEmpty(transactionId))
				{
					Log.Instance.LogError("Mollie: TransactionId IsNullOrEmpty");
					return string.Empty;
				}

				var orderInfo = OrderHelper.GetOrder(transactionId);

				if (orderInfo == null)
				{
					Log.Instance.LogError("Mollie: Order Not Found For TransactionId: " + transactionId);
					return string.Empty;
				}
				
				if (orderInfo.Paid != false)
				{
					Log.Instance.LogDebug("Mollie: Order already paid! TransactionId: " + transactionId);
					return string.Empty;
				}
				
				var partnerId = paymentProvider.GetSetting("PartnerId");
				var testMode = paymentProvider.TestMode;

				var idealCheck = new IdealCheck(partnerId, testMode, transactionId);

				if (idealCheck.Error)
				{
					if (idealCheck.Message != null)
					{
						Log.Instance.LogError(string.Format("Mollie idealCheck.Error Error! idealCheck.Message: {0}", idealCheck.Message));
					}
					if (idealCheck.ErrorMessage != null)
					{
						Log.Instance.LogError(string.Format("Mollie idealCheck.Error Error! idealCheck.ErrorMessage: {0}", idealCheck.ErrorMessage));
					}

					if (orderInfo.Status == OrderStatus.ReadyForDispatch)
					{
						return string.Empty;
					}
					orderInfo.Paid = false;
					orderInfo.Status = OrderStatus.PaymentFailed;

					if (idealCheck.ErrorMessage != null)
					{
						orderInfo.PaymentInfo.ErrorMessage = idealCheck.ErrorMessage;
					}
				}

				if (idealCheck.Payed)
				{
					orderInfo.Paid = true;
					orderInfo.Status = OrderStatus.ReadyForDispatch;
				}
				else
				{
					if (idealCheck.Message != null)
					{
						Log.Instance.LogError(string.Format("Mollie idealCheck.Payed Error! idealCheck.Message: {0}", idealCheck.Message));
					}
					if (idealCheck.ErrorMessage != null)
					{
						Log.Instance.LogError(string.Format("Mollie idealCheck.Payed Error! idealCheck.ErrorMessage: {0}", idealCheck.ErrorMessage));
					}

					orderInfo.Paid = false;
					orderInfo.Status = OrderStatus.PaymentFailed;
					orderInfo.PaymentInfo.ErrorMessage = idealCheck.ErrorMessage;
				}

				orderInfo.Save();
			}
			catch (Exception ex)
			{
				Log.Instance.LogError("MolliePaymentResponseHandler.HandlePaymentResponse: " + ex);
			}

			return null;
		}
	}
}