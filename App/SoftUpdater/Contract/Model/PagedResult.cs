using System.Collections.Generic;

namespace SoftUpdater.Contract.Model
{
    public class PagedResult<T>
    {
        public PagedResult(IEnumerable<T> data, int pageCount)
        {
            Data = data;
            PageCount = pageCount;
        }
        public IEnumerable<T> Data { get; }
        public int PageCount { get; }
    }
}
