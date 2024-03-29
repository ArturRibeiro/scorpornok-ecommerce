﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Store.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderContext>
    {
        public OrderContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<OrderContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseNpgsql(connectionString);
            return new OrderContext(builder.Options);
        }
    }
}
