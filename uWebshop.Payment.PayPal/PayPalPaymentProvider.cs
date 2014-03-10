using System.Collections.Generic;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.PayPal
{
	/// <summary>
	/// DLL naming must be: uWebshop.Payment.PaymentProviderName
	/// </summary>
	public class PayPalPaymentProvider : PayPalPaymentBase, IPaymentProvider
	{
		
		public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id = 0)
		{
			var paymentProviderMethodList = new List<PaymentProviderMethod>
			                                {
				                                new PaymentProviderMethod
				                                {
					                                Id = "PayPal", 
													ProviderName = GetName(), 
													Title = "PayPal", 
													Description = "PayPal"
				                                }
			                                };

			return paymentProviderMethodList;
		}
		
		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.QueryString;
		}
	}
}