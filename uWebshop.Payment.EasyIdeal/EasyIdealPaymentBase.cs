using log4net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using uWebshop.Common;
using uWebshop.Domain;
using uWebshop.Domain.Interfaces;

namespace uWebshop.Payment.EasyIdeal
{
    public class EasyIdealPaymentBase
	{
        private static readonly ILog Log =
            LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType
            );

		public string GetName()
		{
            return "EasyIdeal";
		}

            //<provider title="EasyIdeal">
            //  <merchantId>merchantId#</merchantId>
            //  <merchantKey>merchantKey</merchantKey>
            //  <merchantSecret>merchantSecret</merchantSecret>
            //      <url>https://secure.ogone.com/ncol/prod/orderstandard.asp</url>
              
            //</provider> 

        public string ApiVersion()
        {
            return "1";
        }

        public const string IDEAL_GETBANKS = "IDEAL.GETBANKS";
        public const string IDEAL_EXECUTE = "IDEAL.EXECUTE";
        public const string TRANSACTIONSTATUS = "TRANSACTIONSTATUS";

        public string getXML(string action, SortedList<string, string> args, string merchantId, string merchantKey, string merchantSecret)
        {
            var document = new XDocument(
                           new XElement("Transaction",
                                        new XElement("Action",
                                                     new XElement("Name", action),
                                                     new XElement("Version", ApiVersion())
                                            ),
                                        new XElement("Merchant",
                                                     new XElement("ID", merchantId),
                                                     new XElement("Key", merchantKey),
                                                     new XElement("Checksum", getChecksum(args, merchantSecret))
                                            )
                               )
                           ) { Declaration = new XDeclaration("1.0", "utf-8", "true") };

            if (args.Count > 0)
            {
                var parameters = new XElement("Parameters");
                foreach (var arg in args)
                {
                    parameters.Add(new XElement(arg.Key, arg.Value));
                }

                if (document.Root != null) document.Root.Add(parameters);
            }

            return document.ToString();
        }

        public string postXML(string XMLData, string url)
        {
            XMLData = "data=" + XMLData;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = Encoding.UTF8.GetByteCount(XMLData);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";

            var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            streamWriter.Write(XMLData);
            streamWriter.Close();

            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            var streamReader = new StreamReader(httpWebResponse.GetResponseStream());
            var xmlDoc = XDocument.Parse(streamReader.ReadToEnd());

            return xmlDoc.ToString();
        }

        public string getChecksum(SortedList<string, string> args, string secret)
        {
            string concatValues = "";
            foreach (KeyValuePair<string, string> parameter in args)
            {
                concatValues = concatValues + parameter.Value;
            }
            concatValues = concatValues + secret;
            return SHA1HashStringForUTF8String(concatValues);
        }

        public bool checkChecksumPaymentStatus(string transactionId, string transactionCode, string paymentStatus, string salt, string checksum)
        {
            string concatValues = transactionId + transactionCode + paymentStatus + salt;
            if (SHA1HashStringForUTF8String(concatValues) == checksum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string SHA1HashStringForUTF8String(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
 
            var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(bytes);
 
            return HexStringFromBytes(hashBytes);
        }

        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

	}
}
