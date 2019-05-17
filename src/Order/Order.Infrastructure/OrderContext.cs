using Microsoft.EntityFrameworkCore;
using Order.Domain.SeedWork;
using Order.Infrastructure.EntityConfigurations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Order.Infrastructure
{
    public class OrderContext : DbContext, IUnitOfWork
    {
        public DbSet<Domain.Models.Orders.Order> Orders { get; set; }
        public DbSet<Domain.Models.Orders.OrderItem> OrderItems { get; set; }
        public DbSet<Domain.Models.Orders.ShippingAddress> Address { get; set; }

        public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Ignore(typeof(Domain.Models.Orders.OrderItem));
            modelBuilder.Ignore(typeof(Domain.Models.Orders.ShippingAddress));
            modelBuilder.ApplyConfiguration(new OrderConfigurations());
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    //base.OnConfiguring(optionsBuilder);


        //}
    }
}
