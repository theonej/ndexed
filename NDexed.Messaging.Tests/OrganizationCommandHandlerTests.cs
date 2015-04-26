using System;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDexed.AWS.Messagers;
using NDexed.AWS.Repository;
using NDexed.DataAccess.Repositories;
using NDexed.Domain;
using NDexed.Domain.Models.User;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Handlers;
using NDexed.Security;
using NDexed.Security.Encryptors;
using NDexed.Security.Hashers;
using NDexed.Security.Providers;
using Waitless.Messaging.Commands;
using Waitless.Messaging.Handlers;
using Waitless.Messaging.Tests.Mocks;

namespace Waitless.Messaging.Tests
{
    [TestClass]
    public class OrganizationCommandHandlerTests
    {
        [TestMethod]
        public void CreateNewOrganizationWithUserThenDelete()
        {
            var command = new CreateOrganizationCommand();
            command.Name = "Test Org";
            command.PrimaryEmailAddress = "technology@n-dexed.com";
            command.WebsiteUrl = "www.n-dexed.com";

            IHashProvider hashProvider = new PublicPrivateKeyHasher();
            string publicKey = ConfigurationManager.AppSettings["PublicKey"];
            string privateKey = ConfigurationManager.AppSettings["PrivateKey"];
            hashProvider.RegisterKeyPair(publicKey, privateKey);

            IRepository<UserInfo, UserInfo> userRepository = new MockUserRepository();
            ISearchableRepository<UserInfo, UserInfo> searchRepository = new MockUserRepository();

            var messager = new MockMailMessager();
            IEncryptor encryptor = new RijndaelManagedEncryptor();

            IAuthorizationTokenProvider tokenProvider = new HashAuthorizationTokenProvider(hashProvider, encryptor);
            ICommandHandler<CreateUserCommand> commandHandler = new UserCommandHandler(hashProvider, userRepository, messager, tokenProvider, searchRepository);

            var handler = new OrganizationCommandHandler(new MockOrganizationRepository(), searchRepository,
                commandHandler);

            try
            {

                handler.Handle(command);
            }
            finally
            {
                try
                {
                    var searchCriteria = new UserInfo()
                    {
                        EmailAddress = command.PrimaryEmailAddress
                    };
                    var user = searchRepository.Search(searchCriteria).First();
                    userRepository.Remove(user);
                }
                catch
                {
                }
            }


        }
    }
}
