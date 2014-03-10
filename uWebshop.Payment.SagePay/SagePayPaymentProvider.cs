using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.SagePay
{
	public class SagePayPaymentProvider : IPaymentProvider
	{
		public string GetName()
		{
			return "SagePay";
		}

		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.ServerPost;
		}

		public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
		{
			return Enumerable.Empty<PaymentProviderMethod>();
		}
	}
}