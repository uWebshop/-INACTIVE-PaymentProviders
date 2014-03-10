namespace uWebshop.Payment.Buckaroo
{
    using System.Security.Cryptography;
    using System.Text;

    public class BuckarooSignTool
    {
        public static string HashMessage(string message)
        {
            SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
            //convert input string to a byte array
            byte[] messageArray = Encoding.UTF8.GetBytes(message);
            //calculate hash over the byte array
            byte[] hash1 = sha1Provider.ComputeHash(messageArray);

            StringBuilder builder = new StringBuilder();
            //convert each byte in the hash to hexadecimal format
            foreach (byte b in hash1)
            {
                builder.Append(b.ToString("x2"));
            }
            //retrieve the result from the stringbuilder
            return builder.ToString();
        }
    }
}
