// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuckarooRequestParameters.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the BuckarooRequestParameters type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace uWebshop.Payment.Buckaroo
{
    using System.Globalization;

    public class BuckarooRequestParameters : BuckarooParametersBase
    {       
        public BuckarooRequestParameters(string secretKey)
            : base(secretKey)
        {
        }

        public BuckarooRequestParameters(string secretKey, Dictionary<string, string> parameters)
            : base(secretKey, parameters)
        {
        }

        
        public string Culture
        {
            get
            {
                return _params["brq_culture"];
            }
            set
            {
                _params["brq_culture"] = value;
            }
        }

        public string WebsiteKey
        {
            get
            {
                return _params["brq_websitekey"];
            }
            set
            {
                _params["brq_websitekey"] = value;
            }
        }


        public decimal Amount
        {
            set
            {
                _params["brq_amount"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public string Currency
        {
            get
            {
                return _params["brq_currency"];
            }
            set
            {
                _params["brq_currency"] = value;
            }
        }

        public string InvoiceNumber
        {
            get
            {
                return _params["brq_invoicenumber"];
            }
            set
            {
                _params["brq_invoicenumber"] = value;
            }
        }


        public string ReturnUrl
        {
            get
            {
                return _params["brq_return"];
            }
            set
            {
                _params["brq_return"] = value;
            }
        }

        public string ReturnCancelUrl
        {
            get
            {
                return _params["brq_returncancel"];
            }
            set
            {
                _params["brq_returncancel"] = value;
            }
        }

        public string ReturnErrorUrl
        {
            get
            {
                return _params["brq_returnerror"];
            }
            set
            {
                _params["brq_returnerror"] = value;
            }
        }

        public string ReturnReject
        {
            get
            {
                return _params["brq_returnreject"];
            }
            set
            {
                _params["brq_returnreject"] = value;
            }
        }

        public string PaymentMethod
        {
            get
            {
                return _params["brq_payment_method"];
            }
            set
            {
                _params["brq_payment_method"] = value;
            }
        }

        public string Signature
        {
            get
            {
                return _params["brq_signature"];
            }
             set
            {
                _params["brq_signature"] = value;
            }
        }

        public void AddCustomParameter(string paramName, string value)
        {
            _params[paramName] = value;
        }

        public string TransactionId
        {                        
           get
            {
                return _params["add_transactionReference"];
            }
             set
            {
                _params["add_transactionReference"] = value;
            }
        }

        public void Sign()
        {
            Signature = GetSignature();
        }
    }
}
