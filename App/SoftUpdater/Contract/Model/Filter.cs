namespace SoftUpdater.Contract.Model
{
    public abstract class Filter<T> : IFilter<T> where T : Entity
    {
        public Filter(int? size = null, int? page = null, string sort = null)
        {
            Size = size;
            Page = page;
            Sort = sort;
        }
        public int? Size { get; }
        public int? Page { get; }
        public string Sort { get; }
    }
}
