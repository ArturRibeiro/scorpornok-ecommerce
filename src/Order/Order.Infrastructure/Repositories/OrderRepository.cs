using Order.Domain.Models.Orders;
using Shared.Code.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _context;

        public IUnitOfWork UnitOfWork => _context;
    }
}
