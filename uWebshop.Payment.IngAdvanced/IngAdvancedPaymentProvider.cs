using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using uWebshop.API;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;
using uWebshop.Domain.Helpers;
using Log = uWebshop.Domain.Log;

namespace uWebshop.Payment.IngAdvanced
{
	public class IngAdvancedPaymentProvider : IngAdvancedPaymentBase, IPaymentProvider
	{
		public PaymentTransactionMethod GetParameterRenderMethod()
		{
			return PaymentTransactionMethod.ServerPost;
		}

	    public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
	    {
            var paymentProviderMethodList = new List<PaymentProviderMethod>
                                            {
                                                new PaymentProviderMethod
                                                {
                                                    Id = "iDeal",
                                                    ProviderName = GetName(),
                                                    Title = "iDeal",
                                                    Description = "ING iDeal"
                                                }
                                            };

            return paymentProviderMethodList;
        }
	}
}