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

        private static ServiceCollection _serviceCollection = new ServiceCollection();
        internal static IServiceProvider _container => _serviceCollection.BuildServiceProvider();

        public static T GetInstance<T>()
        {
            var instance = (T)_container.GetService(typeof(T));
            return instance;
        }



        public static ServiceCollection RegisterAll()
        {
            //DbContext
            _serviceCollection.AddScoped<Order.Infrastructure.OrderContext>();

            
            return _serviceCollection;
        }

        //}





        //internal static void BuildService()
        //    => _container = _serviceCollection.BuildServiceProvider();
    }
}
