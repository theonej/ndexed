using System;
using System.Collections.Generic;
using NDexed.Domain.Models;

namespace NDexed.DataAccess.Repositories
{
    public interface IRepository<TId, TItem> where TItem:AuditInfo
    {
        TItem Get(TId id);
        TId Add(TItem item);
        void Remove(TItem item);
        IEnumerable<TItem> GetAll();
    }
}
