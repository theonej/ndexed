using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDexed.Security.Encryptors;
using NDexed.Security.Hashers;
using NDexed.Security.Providers;

namespace NDexed.Security.Tests
{
    [TestClass]
    public class HashProviderTests
    {
        [TestMethod]
        public void CreateTokenThenValidateToken()
        {
            var hasher = new PublicPrivateKeyHasher();
            string publicKey = ConfigurationManager.AppSettings["PublicKey"];
            string privateKey = ConfigurationManager.AppSettings["PrivateKey"];
            hasher.RegisterKeyPair(publicKey, privateKey);

            var encryptor = new RijndaelManagedEncryptor();

            var id = Guid.NewGuid();

            var tokenProvider = new HashAuthorizationTokenProvider(hasher, encryptor);
            var token = tokenProvider.GenerateAuthorizationToken(id);

            tokenProvider.ValidateAuthorizationToken(token);
        }
    }
}
