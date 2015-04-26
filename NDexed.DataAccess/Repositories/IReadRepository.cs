using NDexed.Domain.Models;

namespace NDexed.DataAccess.Repositories
{
    public interface IReadRepository<TId, TItem> : IRepository<TId, TItem>, ISearchableRepository<TItem, TItem> where TItem : AuditInfo
    {
    }
}
