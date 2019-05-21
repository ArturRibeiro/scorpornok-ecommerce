using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Frameworker.Scorponok.Tests.WebHost.Extensions
{
    public static class IWebHostExtension
    {
        public static IWebHost ErasureDatabase<TContext>(this IWebHost webHost) 
            where TContext : DbContext
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                try
                {
                    var context = services.GetService<TContext>();
                    context.Database.EnsureDeleted();
                    logger.LogInformation($"O banco associado a esse contexto foi excluido com sucesso {typeof(TContext).Name}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Não foi possivel excluir o banco associado com esse context {typeof(TContext).Name}");
                }                
            }
            
            return webHost;
        }

        public static IWebHost CreateDataBase<TContext>(this IWebHost @this, Action<TContext, IServiceProvider> seeder) 
            where TContext : DbContext
        {
            using (var scope = @this.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();

                try
                {
                    var context = services.GetService<TContext>();
                    context.Database.Migrate();
                    logger.LogInformation($"Migrated database associated with context {typeof(TContext).Name}");
                    seeder(context, services);
                    logger.LogInformation($"Migrated database associated with context {typeof(TContext).Name}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Ocorreu um erro ao migrar o banco de dados usado esse contexto {typeof(TContext).Name}");
                }
            }

            return @this;
        }
    }
}