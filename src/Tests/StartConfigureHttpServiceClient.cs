using Ecommerce.Integration.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Integration.Tests
{
    public partial class Start
    {
        public Start ConfigureHttpServiceClient()
        {
            NativeInjectorBootStrapper.CreateDependencyInjectionServiceClientPayment(_serverPayment);
            NativeInjectorBootStrapper.CreateDependencyInjectionServiceClientOrder(_serverOrder);
            NativeInjectorBootStrapper.CreateDependencyInjectionServiceClientCatalog(_serverCatalog);
            NativeInjectorBootStrapper.BuildService();

            return this;
        }
    }
}
