using System.Collections.Generic;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Omnikassa
{
	public class OmnikassaPaymentProvider : OmnikassaPaymentBase, IPaymentProvider
	{
		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.QueryString;
		}

		public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id = 0)
		{
			var paymentProviderMethodList = new List<PaymentProviderMethod>
			                                {
				                                new PaymentProviderMethod
				                                {
					                                Id = "Omnikassa", 
													ProviderName = GetName(), 
													Title = "Omnikassa", 
													Description = "Rabobank Omnikassa" 
				                                }
			                                };

			return paymentProviderMethodList;
		}
	}
}