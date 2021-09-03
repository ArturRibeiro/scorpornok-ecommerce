using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Infrastructure;

namespace Store.Web.Api.Extensions
{
    public static class DbContextExtension
    {
        public static IServiceCollection AddInfraDbContext(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<OrderContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            return services;
        }
    }
}