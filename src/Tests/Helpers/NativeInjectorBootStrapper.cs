using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.Helpers
{
    public class NativeInjectorBootStrapper
    {
        private static IServiceProvider _container;
        private static ServiceCollection _serviceCollection = new ServiceCollection();

        public static BaseHttpServiceClient GetInstanceHttpServiceClient<T>() where T : BaseHttpServiceClient
        {
            var instance = _container.GetServices<T>();
            return instance.ElementAt(0);
        }

        internal static void CreateDependencyInjectionServiceClientOrder(TestServer server)
            => _serviceCollection.AddSingleton(new HttpServiceClientOrder(server));

        internal static void CreateDependencyInjectionServiceClientPayment(TestServer server)
            => _serviceCollection.AddSingleton(new HttpServiceClientPayment(server));

        internal static void CreateDependencyInjectionServiceClientCatalog(TestServer server)
            => _serviceCollection.AddSingleton(new HttpServiceClientCatalog(server));

        internal static void BuildService()
            => _container = _serviceCollection.BuildServiceProvider();
    }
}
