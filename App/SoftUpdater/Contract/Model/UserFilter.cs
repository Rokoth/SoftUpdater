using System;
using System.Collections.Generic;

namespace SoftUpdater.Contract.Model
{
    public class UserFilter : Filter<User>
    {
        public UserFilter(int size, int page, string sort, string name) : base(size, page, sort)
        {
            Name = name;
        }
        public string Name { get; }
    }

    public class ClientFilter : Filter<Client>
    {
        public ClientFilter(int size, int page, string sort, string name, Guid userId) : base(size, page, sort)
        {
            Name = name;
            UserId = userId;
        }
        public string Name { get; }
        public Guid? UserId { get; }
    }

    public class ReleaseFilter : Filter<Release>
    {
        public ReleaseFilter(List<Guid> clients = null, int? size = null, int? page = null, string sort = null, string name = null) : base(size, page, sort)
        {
            Name = name;
            Clients = clients;           
        }        
        public List<Guid> Clients { get; }
        public string Name { get; }
    }

    public class ReleaseArchitectFilter : Filter<ReleaseArchitect>
    {
        public ReleaseArchitectFilter(Guid releaseId, int size, int page, string sort, string name) : base(size, page, sort)
        {
            Name = name;
            ReleaseId = releaseId;
        }
        public string Name { get; }
        public Guid ReleaseId { get; }
    }

    public class LoadHistoryFilter : Filter<LoadHistory>
    {
        public LoadHistoryFilter(int size, int page, string sort) : base(size, page, sort)
        {            
        }       
    }
}
