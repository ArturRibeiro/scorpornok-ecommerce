using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Frameworker.Scorponok.AspNet.Mvc;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Store.Infrastructure;
using Store.Web.Api.App;
using Store.Web.Api.Middlewares;

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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Store.Web.Api", Version = "v1" });
            });
            
            try
            {
                services
                    .AddMvc(options => { options.AddNotificationAsyncResultFilter<NotificationAsyncResultFilter>(Configuration); });

                services.AddNewtonsoftJsonOptions();
                services.AddMediatR(typeof(StartupStore).Assembly);
                services.AddHttpContextAccessor();

                NativeDependencyInjection.RegisterServices(services);

                services.AddDbContext<OrderContext>(options
                    => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            }
            catch (Exception ex)
            {
                Log(ex);
            }
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
        
        private void Log(Exception e) => File.AppendAllText($"Log\\Log{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.txt", e.ToString());
    }
}