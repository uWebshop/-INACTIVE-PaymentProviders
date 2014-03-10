using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uWebshop.Payment.Buckaroo
{
    public class BuckarooParametersBase
    {
        public string[] KNOWN_PREFIXES = new string[] { "brq_", "add_", "cust_" };
        public const string SIGNATURE_PARAM_NAME = "brq_signature";

        protected Dictionary<string, string> _params;

        protected string SecretKey
        {
            get;
            set;
        }

        public BuckarooParametersBase(string secretKey)
        {
            _params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            SecretKey = secretKey;
        }

        public BuckarooParametersBase(string secretKey, Dictionary<string, string> parameters)
        {
            _params = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);
            SecretKey = secretKey;
        }

        protected Dictionary<string, string> FilterParametersToSign()
        {
            return _params
                .Where(p => string.Compare(p.Key, SIGNATURE_PARAM_NAME, true) != 0)
                .Where(p => KNOWN_PREFIXES.Where(o => p.Key.StartsWith(o, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null)
                .ToDictionary(p => p.Key, p => p.Value);
        }


        public string GetSignature()
        {
            //sort by full name
            var sorderByFullName = FilterParametersToSign()              
                .OrderBy(g => g.Key);

            //List<KeyValuePair<string, string>> sortedParams = new List<KeyValuePair<string, string>>();

            //foreach (var group in sorderByFullName)
            //{
            //    //sort by parameter name inside each group
            //    sortedParams.AddRange(group
            //        .ToList()
            //        .OrderBy(p => p.Key.Split('_')[1]));
            //}

            StringBuilder result = new StringBuilder();

            foreach (KeyValuePair<string, string> parameter in sorderByFullName)
            {
                result.Append(string.Concat(parameter.Key, "=", parameter.Value));
            }

            result.Append(SecretKey);

            return BuckarooSignTool.HashMessage(result.ToString());
        }


        public string GetParameterValue(string name)
        {
            if (_params.ContainsKey(name))
            {
                return _params[name];
            }

            return string.Empty;
        }

        public Dictionary<string, string> GetParameters()
        {
            return _params;
        }
    }
}
