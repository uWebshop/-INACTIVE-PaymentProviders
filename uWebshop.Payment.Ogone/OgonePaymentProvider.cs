using System.Collections.Generic;
using System.Linq;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Helpers;
using uWebshop.Domain.Interfaces;
using umbraco;

namespace uWebshop.Payment.Ogone
{
    public class OgonePaymentProvider : OgonePaymentBase, IPaymentProvider
    {
        #region IPaymentProvider Members

        PaymentTransactionMethod IPaymentProvider.GetParameterRenderMethod()
        {
            return PaymentTransactionMethod.QueryString;
        }

        public IEnumerable<PaymentProviderMethod> GetAllPaymentMethods(int id)
        {

            var paymentProvider = PaymentProvider.GetPaymentProvider(id);

            var providerSettingsXML = paymentProvider.GetSettingsXML();

            var enabledServices = providerSettingsXML.Descendants("Services").FirstOrDefault();

            var paymentMethods = new List<PaymentProviderMethod>();

            foreach (var service in enabledServices.Descendants("Service"))
            {
                var serviceName = service.Attribute("name").Value;
                var serviceTitle = serviceName;

                if (service.Attribute("title") != null)
                {
                    serviceTitle = service.Attribute("title").Value;
                }

                var paymentImageId = 0;

                if (service.Descendants().Any())
                {
                    foreach (var issuer in service.Descendants("Issuer"))
                    {
                        var name = issuer.Element("Name").Value;
                        var code = issuer.Element("Code").Value;

                        var method = string.Format("{0}-{1}", serviceName, code);

                        var nameForLogoDictionaryItem = string.Format("{0}LogoId", code.Replace(" ", string.Empty));

                        var logoDictionaryItem = library.GetDictionaryItem(nameForLogoDictionaryItem);

                        if (string.IsNullOrEmpty(logoDictionaryItem))
                        {
                            int.TryParse(library.GetDictionaryItem(nameForLogoDictionaryItem), out paymentImageId);
                        }

                        paymentMethods.Add(new PaymentProviderMethod
                        {
                            Id = method,
                            Description = name,
                            Title = name,
                            Name = serviceTitle,
                            ProviderName = GetName(),
                            ImageId = paymentImageId
                        });
                    }
                }
                else
                {
                    var nameForLogoDictionaryItem = string.Format("{0}LogoId", serviceName.Replace(" ", string.Empty));

                    var logoDictionaryItem = library.GetDictionaryItem(nameForLogoDictionaryItem);

                    if (string.IsNullOrEmpty(logoDictionaryItem))
                    {
                        int.TryParse(library.GetDictionaryItem(nameForLogoDictionaryItem), out paymentImageId);
                    }

                    paymentMethods.Add(new PaymentProviderMethod
                    {
                        Id = serviceName,
                        Description = serviceName,
                        Title = serviceTitle,
                        Name = serviceName,
                        ProviderName = GetName(),
                        ImageId = paymentImageId
                    });
                }
            }




            return paymentMethods;

        }

        #endregion
    }
}