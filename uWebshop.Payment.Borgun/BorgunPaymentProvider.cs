using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Borgun
{
    public class BorgunPaymentProvider : BorgunPaymentBase, IPaymentProvider
    {
        public PaymentTransactionMethod GetParameterRenderMethod()
        {
            return PaymentTransactionMethod.Inline;
        }

        public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
        {
            var paymentProviderMethodList = new List<PaymentProviderMethod>
				{
					new PaymentProviderMethod
						{
							Id = "BorgunAPI",
							ProviderName = "BorgunAPI",
							Title = "Borgun API",
							Description = "Borgun API Payment Provider"
						}
				};

            return paymentProviderMethodList;
        }
    }
}
