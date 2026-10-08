namespace Orders.Infrastructure
{
    [ExcludeFromCodeCoverage]
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

            // Tabelas do outbox do MassTransit: o PaymentRequested é gravado aqui, na mesma
            // transação do pedido, e entregue ao RabbitMQ em segundo plano.
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
