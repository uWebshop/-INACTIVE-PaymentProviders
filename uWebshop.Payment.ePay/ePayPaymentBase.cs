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

        private static string ByteArrayToString(byte[] array)
        {
            var sb = new StringBuilder(array.Length * 2);
            for (var i = 0; i < array.Length; i++)
            {
                sb.Append(array[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}