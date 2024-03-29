﻿using Frameworker.Scorponok.AspNet.Mvc;
using Gateway.Payment.Web.Api.App;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gateway.Payment.Web.Api.Middlewares;
using MediatR;
using Microsoft.Extensions.Hosting;


namespace Gateway.Payment.Web.Api
{
    public class StartupPayment
    {
        public StartupPayment(IWebHostEnvironment hostingEnvironment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.AddNotificationAsyncResultFilter<NotificationAsyncResultFilter>(Configuration);
            });
            services.AddNewtonsoftJsonOptions();
            services.AddMediatR(typeof(StartupPayment).Assembly);
            services.AddHttpContextAccessor();

            NativeDependencyInjection.RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}