using Ecommerce.Integration.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Integration.Tests
{
    public partial class Start
    {
        public Start CreateDependencyInjection()
        {

            var service = NativeInjectorBootStrapper.RegisterAll();
            service.AddSingleton(new HttpServiceClientOrder(this._serverOrder));
            service.AddSingleton(new HttpServiceClientPayment(this._serverPayment));
            service.AddSingleton(new HttpServiceClientCatalog(this._serverCatalog));

            return this;
        }   
    }
}
