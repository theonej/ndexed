using System;
using System.Configuration;
using System.Security.Authentication;
using CuttingEdge.Conditions;
using NDexed.Domain.Resources;

namespace NDexed.Security.Providers
{
    public class HashAuthorizationTokenProvider : IAuthorizationTokenProvider
    {
        private const int EXPIRATION_MINUTES = 60 * 24;//ONE DAY

        private readonly IHashProvider m_HashProvider;
        private readonly IEncryptor m_Encryptor;

        public HashAuthorizationTokenProvider(IHashProvider hashProvider, IEncryptor encryptor)
        {
            Condition.Requires(hashProvider).IsNotNull();
            Condition.Requires(encryptor).IsNotNull();

            m_HashProvider = hashProvider;
            m_Encryptor = encryptor;
        }

        public string GenerateAuthorizationToken(Guid userId)
        {
            string expiration = DateTime.Now.AddMinutes(EXPIRATION_MINUTES).ToString();

            string publicKey = ConfigurationManager.AppSettings["PublicKey"];

            string tokenString = string.Format("{0}|{1}|{2}", userId, expiration, publicKey);

            string encryptedToken = m_Encryptor.EncryptValue(tokenString);

            var hash = m_HashProvider.GenerateHash(encryptedToken, encryptedToken, publicKey);

            var token = string.Format("{0}|{1}", encryptedToken, hash);

            return token;
        }

        public Guid ValidateAuthorizationToken(string token)
        {
            var tokenParts = token.Split('|');
            var encryptedToken = tokenParts[0];
            var tokenHash = tokenParts[1];
            
            string publicKey = ConfigurationManager.AppSettings["PublicKey"];

            var validationHash = m_HashProvider.GenerateHash(encryptedToken, encryptedToken, publicKey);
            if (validationHash != tokenHash)
            {
                throw new FormatException(ErrorMessages.MalformedAuthorizationToken);
            }
            //decrypt the token
            var decryptedToken = m_Encryptor.DecryptValue(encryptedToken);

            //extract the different peices of the token [userid:expiration:hash]
            string[] decryptedParts = decryptedToken.Split('|');
            if (decryptedParts.Length != 3)
            {
                throw new FormatException(ErrorMessages.MalformedAuthorizationToken);
            }

            //validate the expiration
            DateTime expiration = DateTime.Parse(decryptedParts[1]);
            if (DateTime.Now > expiration)
            {
                throw new AuthenticationException(ErrorMessages.ExpiredToken);
            }

            Guid userId = Guid.Parse(decryptedParts[0]);

            return userId;
        }
    }
}
