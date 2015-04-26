using System;
using NDexed.Domain.Models;
using NDexed.Domain.Models.Payment;
using NDexed.Domain.Models.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dependencies;
using NDexed.AWS.Messagers;
using NDexed.AWS.Repository;
using NDexed.DataAccess.Repositories;
using NDexed.Domain;
using NDexed.Messaging.Commands;
using NDexed.Messaging.Handlers;
using NDexed.Messaging.Messages;
using NDexed.Payments;
using NDexed.Payments.Stripe;
using NDexed.Rest.Registry;
using NDexed.Security;
using NDexed.Security.Encryptors;
using NDexed.Security.Hashers;
using NDexed.Security.Providers;
using NDexed.Rest.Filters;
using NDexed.Messaging.Commands.Queues;
using Waitless.Messaging.Commands;
using Waitless.Messaging.Handlers;

namespace NDexed.Rest
{
    public static class WebApiConfig
    {
        internal static IDependencyResolver Container;

        public static void Register(HttpConfiguration config)
        {
           var corsConfig = new EnableCorsAttribute("*", "*", "*");
           corsConfig.Methods.Add("get");
            corsConfig.Methods.Add("post");
            corsConfig.Methods.Add("put");
            corsConfig.Methods.Add("options");
            corsConfig.Methods.Add("delete");
            config.EnableCors(corsConfig);

            config.DependencyResolver = GetContainer();
            Container = config.DependencyResolver;

            config.Routes.MapHttpRoute(
                name: "ServiceProviderResources",
                routeTemplate: "api/ServiceProvider/{serviceProviderId}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "UserServicesWithAction",
                routeTemplate: "api/User/{action}/{id}",
                defaults: new { controller = "User", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "PaymentServicesWithAction",
                routeTemplate: "api/Payment/{action}/{id}",
                defaults: new { controller = "Payment", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ServiceInventoryResources",
                routeTemplate: "api/ServiceInventory/{serviceInventoryId}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DataApi",
                routeTemplate: "api/Data/{type}/{id}",
                defaults: new { controller="Data" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            JsonMediaTypeFormatter jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonFormatter.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Filters.Add(new ExceptionFilter());
        }

        #region Private Methods

        private static IDependencyResolver GetContainer()
        {
            DependencyController container = new DependencyController();

            container.Register<IRepository<UserInfo, UserInfo>, DynamoUserRepository>();
            container.Register<ISearchableRepository<UserInfo, UserInfo>, DynamoUserRepository>();
            container.Register<IRepository<Guid, Organization>, DynamoOrganizationRepository>();
            container.Register<IRepository<UserPaymentInfo, UserPaymentInfo>, DynamoUserPaymentRepository>();

            //security
            container.Register<IHashProvider, PublicPrivateKeyHasher>();
            container.Register<IEncryptor, RijndaelManagedEncryptor>();
            container.Register<IAuthorizationTokenProvider, HashAuthorizationTokenProvider>();

            //handlers
            container.Register<ICommandHandler<CreateOrganizationCommand>, OrganizationCommandHandler>();
            container.Register<ICommandHandler<CreateUserCommand>, UserCommandHandler>();
            container.Register<ICommandHandler<ResetPasswordCommand>, UserCommandHandler>();
            container.Register<ICommandHandler<SetPasswordCommand>, UserCommandHandler>();
            container.Register<ICommandHandler<AddQueueItemCommand>, QueueCommandHandler>();
            container.Register<ICommandHandler<UpdateQueueItemCommand>, QueueCommandHandler>();
            container.Register<ICommandHandler<SavePaymentInfoCommand>, PaymentCommandHandler>();

            

            //payments
            container.Register<IPaymentProvider, StripePaymentProvider>();

            container.Register<IMessager, AwsEmailMessenger>();
            return container;
        }

        #endregion
    }
}
