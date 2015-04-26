using System.Collections.Generic;

namespace NDexed.DataAccess.Repositories
{
    public interface ISearchableRepository<in TSearchCriteria, out TResult>
    {
        IEnumerable<TResult> Search(TSearchCriteria criteria);
    }
}
