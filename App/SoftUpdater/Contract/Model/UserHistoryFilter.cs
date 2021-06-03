using System;

namespace SoftUpdater.Contract.Model
{
    public class UserHistoryFilter : Filter<UserHistory>
    {
        public UserHistoryFilter(int size, int page, string sort, string name, Guid? id) : base(size, page, sort)
        {
            Name = name;
            Id = id;
        }
        public string Name { get; }
        public Guid? Id { get; }
    }

    public class ClientHistoryFilter : Filter<ClientHistory>
    {
        public ClientHistoryFilter(int size, int page, string sort, string name, Guid? id) : base(size, page, sort)
        {
            Name = name;
            Id = id;
        }
        public string Name { get; }
        public Guid? Id { get; }
    }

    public class ReleaseHistoryFilter : Filter<ReleaseHistory>
    {
        public ReleaseHistoryFilter(Guid clientId, int size, int page, string sort, string name, Guid? id) : base(size, page, sort)
        {
            Name = name;
            Id = id;
            ClientId = clientId;
        }
        public Guid ClientId { get; }
        public string Name { get; }
        public Guid? Id { get; }
    }
}
