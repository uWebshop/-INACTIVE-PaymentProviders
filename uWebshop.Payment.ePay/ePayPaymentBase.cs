using System.Text;

namespace uWebshop.Payment.ePay
{
    public class ePayPaymentBase
    {
        public string GetName()
        {
            return "ePay";
        }

        public static string MD5(string phrase)
        {
            var hasher = System.Security.Cryptography.MD5.Create();
            var hashedDataBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
        }

        private static string ByteArrayToString(byte[] inputArray)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < inputArray.Length; i++)
            {
                sb.Append(inputArray[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}