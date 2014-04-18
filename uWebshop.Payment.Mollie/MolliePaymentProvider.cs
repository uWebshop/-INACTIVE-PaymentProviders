using System;
using System.Collections.Generic;
using System.Linq;
using Mollie.iDEAL;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.Mollie
{
    public class MolliePaymentProvider : MolliePaymentBase, IPaymentProvider
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
                Log.Instance.LogError(
                    "Mollie PaymentProvider 'GetAllPaymentMethods' paymentProvider == null");

                return paymentMethods;
            }

            var testMode = paymentProvider.TestMode;

            var partnerId = paymentProvider.GetSetting("PartnerId");

            if (partnerId == null)
            {
                Log.Instance.LogError(
                    "Mollie PaymentProvider 'partnerId' not found in the PaymentProvider config file. Is the config correct?");
            }
            else
            {
                var banks = new IdealBanks(partnerId, testMode);

                foreach (var bank in banks.Banks)
                {
                    int paymentImageId;

                    int.TryParse(umbraco.library.GetDictionaryItem(bank.Name + "LogoId"), out paymentImageId);

                    paymentMethods.Add(new PaymentProviderMethod
                    {
                        Id = bank.Id,
                        Description = string.Format("iDEAL via {0}", bank.Name),
                        Title = bank.Name,
                        Name = bank.Name,
                        ProviderName = GetName()
                    });
                }
            }

            return paymentMethods;
        }
    }

}
