namespace SoftUpdater.Contract.Model
{
    public interface IFilter<T> where T : Entity
    {
        int Page { get; }
        int Size { get; }
        string Sort { get; }
    }
}
