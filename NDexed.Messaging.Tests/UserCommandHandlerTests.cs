using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDexed.Domain.Models.User;
using NDexed.Security;
using NDexed.Security.Encryptors;
using NDexed.Security.Hashers;
using NDexed.Security.Providers;
using NDexed.DataAccess.Repositories;
using NDexed.AWS.Repository;
using NDexed.AWS.Repository;
using NDexed.Domain;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Handlers;
using System.Configuration;
using NDexed.Messaging.Messages;
using NDexed.AWS.Messagers;
using Waitless.Messaging.Tests.Mocks;

namespace NDexed.Messaging.Tests
{
    [TestClass]
    public class UserCommandHandlerTests
    {
        [TestMethod]
        public void CreateNewUserByCommandThenFindThenDelete()
        {
            IHashProvider hashProvider= new PublicPrivateKeyHasher();
            string publicKey = ConfigurationManager.AppSettings["PublicKey"];
            string privateKey = ConfigurationManager.AppSettings["PrivateKey"];
            hashProvider.RegisterKeyPair(publicKey, privateKey);

            var userRepository = new MockUserRepository();

            CreateUserCommand command = new CreateUserCommand();
            command.UserName = "J.Henry";
            command.Password = "mydogalma";
            command.Id = Guid.NewGuid();
            command.EmailAddress = "technology@n-dexed.com";

            var messager = new MockMailMessager();
            IEncryptor encryptor =new RijndaelManagedEncryptor();

            IAuthorizationTokenProvider tokenProvider = new HashAuthorizationTokenProvider(hashProvider, encryptor);
            ICommandHandler<CreateUserCommand> commandHandler = new UserCommandHandler(hashProvider, userRepository, messager, tokenProvider, userRepository);
            commandHandler.Handle(command);

            UserInfo searchCriteria = new UserInfo();
            searchCriteria.EmailAddress = command.EmailAddress;

            UserInfo userData = userRepository.Search(searchCriteria).FirstOrDefault();
            Assert.IsNotNull(userData);

            string pashWordHash = hashProvider.GenerateHash(command.Password, userData.Id.ToString(), publicKey);
            Assert.AreEqual(userData.PasswordHash, pashWordHash);
            Assert.AreEqual(command.EmailAddress, userData.EmailAddress);
            Assert.AreEqual(command.Id, userData.Id);

            userRepository.Remove(userData);
        }

        [TestMethod]
        public void CreateUserThenResetPasswordThenDelete()
        {
            IHashProvider hashProvider = new PublicPrivateKeyHasher();
            string publicKey = ConfigurationManager.AppSettings["PublicKey"];
            string privateKey = ConfigurationManager.AppSettings["PrivateKey"];
            hashProvider.RegisterKeyPair(publicKey, privateKey);

            var userRepository = new MockUserRepository();

            CreateUserCommand command = new CreateUserCommand();
            command.UserName = "J.Henry";
            command.Password = "mydogalma";
            command.Id = Guid.NewGuid();
            command.EmailAddress = "technology@n-dexed.com";

            try
            {
                var messager = new MockMailMessager();
                IEncryptor encryptor = new RijndaelManagedEncryptor();

                IAuthorizationTokenProvider tokenProvider = new HashAuthorizationTokenProvider(hashProvider, encryptor);
                ICommandHandler<CreateUserCommand> commandHandler = new UserCommandHandler(hashProvider, userRepository, messager, tokenProvider, userRepository);
                commandHandler.Handle(command);

                ICommandHandler<ResetPasswordCommand> passwordhandler = (ICommandHandler<ResetPasswordCommand>)commandHandler;

                ResetPasswordCommand passwordCommand = new ResetPasswordCommand();
                passwordCommand.EmailAddress = command.EmailAddress;

                passwordhandler.Handle(passwordCommand);
            }
            finally
            {
                UserInfo searchCriteria = new UserInfo();
                searchCriteria.EmailAddress = command.EmailAddress;

                UserInfo userData = userRepository.Search(searchCriteria).FirstOrDefault();
                Assert.IsNotNull(userData);
                userRepository.Remove(userData);
            }
            
        }
    }
}
