namespace SoftUpdater.Contract.Model
{
    public abstract class Filter<T> : IFilter<T> where T : Entity
    {
        public Filter(int size, int page, string sort)
        {
            Size = size;
            Page = page;
            Sort = sort;
        }
        public int Size { get; }
        public int Page { get; }
        public string Sort { get; }
    }
}
