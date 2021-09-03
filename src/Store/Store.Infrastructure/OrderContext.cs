using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shared.Code.Models;
using Store.Domain.Models.Orders;
using Store.Infrastructure.EntityConfigurations;

namespace Store.Infrastructure
{
    public class OrderContext : DbContext, IUnitOfWork
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderAddress> OrderAddress { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }

        public OrderContext(DbContextOptions<OrderContext> options) : base(options) { }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.SaveChangesAsync(cancellationToken) > 0;

            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("order");
            modelBuilder.ApplyConfiguration(new OrderItemConfigurations());
            modelBuilder.ApplyConfiguration(new OrderConfigurations());
        }
    }
}
