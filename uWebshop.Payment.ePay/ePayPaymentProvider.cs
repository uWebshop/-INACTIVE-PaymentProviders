using System.Collections.Generic;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.ePay
{
	public class ePayPaymentProvider : ePayPaymentBase, IPaymentProvider
	{
		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.QueryString;
		}

		public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
		{
			var paymentProviderMethodList = new List<PaymentProviderMethod>
			    {
				    new PaymentProviderMethod
				    {
					    Id = "ePay", 
						ProviderName = GetName(), 
						Title = "ePay", 
						Description = "ePay Payment Provider"
				    }
			    };
			return paymentProviderMethodList;
		}
	}
}