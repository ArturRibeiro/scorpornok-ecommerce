using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Code.Models
{
    public interface IRepository<T> where T : IAggregateRoot
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
