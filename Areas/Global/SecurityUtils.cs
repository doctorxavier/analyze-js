using System.Web;

using IDB.Architecture.Security;

namespace IDB.Presentation.MVC4.Areas.Global
{
    internal class SecurityUtils
    {
        internal static string TryDecodeToken(
            string token, HttpSessionStateBase session, out string[] decodedToken)
        {
            decodedToken = new string[] { };

            if (session == null)
            {
                return "No session started";
            }

            string decryptedToken = CryptoManager.Decode(token);

            if (string.IsNullOrEmpty(decryptedToken))
            {
                return "Invalid token";
            }

            if (!decryptedToken.Contains("|"))
            {
                return "Unexpected decryption token result";
            }

            decodedToken = decryptedToken.Split('|');
            if (decodedToken.Length == 0)
            {
                return "Invalied username after token decryption";
            }

            return string.Empty;
        }

        internal static string GetDecodedUser(string[] tokenContent)
        {
            string[] decodedUserDomain = tokenContent[0].Split('\\');

            // example: IDB\everis1-idb
            return (decodedUserDomain.Length == 1) ? decodedUserDomain[0] : decodedUserDomain[1];
        }
    }
}