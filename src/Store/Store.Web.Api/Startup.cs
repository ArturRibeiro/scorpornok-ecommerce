using System;
using System.IO;
using Frameworker.Scorponok.AspNet.Mvc;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Infrastructure;
using Store.Web.Api.App;
using Store.Web.Api.Extensions.Service;
using Store.Web.Api.Middlewares;

namespace Store.Web.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment hostingEnvironment)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(hostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true);

                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                services
                    .AddMvc(options => { options.AddNotificationAsyncResultFilter<NotificationAsyncResultFilter>(Configuration); })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                //.Net core 3.0
                //.AddNewtonsoftJson();

                services.AddMediatR(typeof(Startup).Assembly);

                services.AddHttpContextAccessor();

                NativeDependencyInjection.RegisterServices(services);

                services.AddDbContext<OrderContext>(options
                    => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                // Register the Swagger services
                services.AddSettingSwaggerDocument();

            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            //app.UseAuthorization();
            //// Register the Swagger generator and the Swagger UI middlewares
            //app.UseSwagger();
            //app.UseSwaggerUi3();
        }

        private void Log(Exception e) => File.AppendAllText($"Log\\Log{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.txt", e.ToString());
    }
}
