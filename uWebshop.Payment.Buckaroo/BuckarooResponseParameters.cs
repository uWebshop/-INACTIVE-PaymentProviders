// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuckarooResponseParameters.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the BuckarooResponseParameters type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace uWebshop.Payment.Buckaroo
{
    using System.Globalization;

    public class BuckarooResponseParameters : BuckarooParametersBase
    {
        public BuckarooResponseParameters(string secretKey)
            : base(secretKey)
        {
        }

        public BuckarooResponseParameters(string secretKey, Dictionary<string, string> parameters)
            : base(secretKey, parameters)
        {
        }
        
        public string WebsiteKey
        {
            get
            {
                return GetParameterValue("brq_websitekey");
            }
        }


        public decimal Amount
        {
            get
            {
                decimal amount = 0;
                decimal.TryParse(GetParameterValue("brq_amount"), NumberStyles.Currency, CultureInfo.InvariantCulture, out amount);

                return amount;
            }
        }

        public string Currency
        {
            get
            {
                return GetParameterValue("brq_currency");
            }
        }

        public string Invoicenumber
        {
            get
            {
                return GetParameterValue("brq_invoicenumber");
            }
        }


        public string Payment
        {
            get
            {
                return GetParameterValue("brq_payment");
            }
        }

        public string PaymentMethod
        {
            get
            {
                return GetParameterValue("brq_payment_method");
            }

        }

        public string StatusCode
        {
            get
            {
                return GetParameterValue("brq_statuscode");
            }
        }

        public string StatusMessage
        {
            get
            {
                return GetParameterValue("brq_statusmessage");
            }
        }

        public string TimeStamp
        {
            get
            {
                return GetParameterValue("brq_timestamp");
            }
        }

        public string Transactions
        {
            get
            {
                return GetParameterValue("brq_transactions");
            }
        }

        public string IssuingCountry
        {
            get
            {
                return GetParameterValue("brq_issuing_country");
            }
        }

        public string TransactionId
        {
            get
            {
                return GetParameterValue("add_transactionReference");
            }           
        }

        public string Signature
        {
            get
            {
                return GetParameterValue("brq_signature");
            }
        }

        public bool IsValid()
        {
            if (_params.ContainsKey("brq_signature") &&
                _params.ContainsKey("brq_statuscode") &&
                _params.ContainsKey("brq_invoicenumber") &&
                _params.ContainsKey("brq_currency") &&
                _params.ContainsKey("add_orderid") &&
                _params.ContainsKey("brq_websitekey"))
            {
                return true;
            }

            return false;
        }
    }
}
