using Shared.Code.Models;
using Store.Domain.Models.Orders;

namespace Store.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _context;

        public IUnitOfWork UnitOfWork => _context;
    }
}
