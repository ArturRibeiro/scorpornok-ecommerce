using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Store.Web.Api.App;
using Store.Web.Api.Extensions;

namespace Store.Web.Api
{
    public class StartupStore
    {
        public StartupStore(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddConfigurationSwagger();
            services.AddConfigurationMvc(this.Configuration);
            services.AddNewtonsoftJsonOptions();
            services.AddConfigurationMediatR();
            services.AddHttpContextAccessor();

            NativeDependencyInjection.RegisterServices(services);

            services.AddInfraDbContext(this.Configuration);
            services.AddMediatR(typeof(StartupStore).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Store.Web.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}