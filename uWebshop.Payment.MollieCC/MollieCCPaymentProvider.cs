using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using Mollie.Api;

namespace uWebshop.Payment.MollieCC
{
    public class MollieCCPaymentProvider : MollieCCPaymentBase, IPaymentProvider
    {
        PaymentTransactionMethod IPaymentProvider.GetParameterRenderMethod()
        {
            return PaymentTransactionMethod.QueryString;
        }
        public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
        {
            var paymentMethods = new List<PaymentProviderMethod>();

            var paymentProvider = PaymentProvider.GetPaymentProvider(id);


            if (paymentProvider == null)
            {
                Log.Instance.LogError("MollieCC PaymentProvider 'GetAllPaymentMethods' paymentProvider == null");

                return paymentMethods;
            }

            var testMode = paymentProvider.TestMode;

            var partnerId = paymentProvider.GetSetting("PartnerId");
            
            paymentMethods.Add(new PaymentProviderMethod
            {
                Id = "0",
                Description = "Creditcard",
                Title = "Creditcard",
                Name = "Creditcard",
                ProviderName = GetName()
            });

            return paymentMethods;
        }
    }
}
