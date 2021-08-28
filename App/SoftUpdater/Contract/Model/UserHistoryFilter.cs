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
        public ReleaseHistoryFilter(Guid? clientId, int size, int page, string sort, string version, Guid? id) : base(size, page, sort)
        {
            Version = version;
            Id = id;
            ClientId = clientId;
        }
        public Guid? ClientId { get; }
        public string Version { get; }
        public Guid? Id { get; }
    }

    public class ReleaseArchitectHistoryFilter : Filter<ReleaseArchitectHistory>
    {
        public ReleaseArchitectHistoryFilter(int size, int page, string sort, Guid? id) : base(size, page, sort)
        {            
            Id = id;            
        }
       
        public Guid? Id { get; }
    }
}
