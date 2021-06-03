using System;
using System.Collections.Generic;

namespace SoftUpdater.Contract.Model
{
    public class Release : Entity
    {
        public string Path { get; set; }
        public string Version { get; set; }
        public int Number { get; set; }
        public Client Client { get; set; }
        public Guid ClientId { get; set; }
    }

    public class ReleaseClient
    {
        public Guid Id { get; set; }
        public string Version { get; set; }
        public List<ReleaseArchitectClient> Architects { get; set; }
    }

    public class ReleaseArchitectClient
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ReleaseHistory : EntityHistory
    {
        public Client Client { get; set; }
        public Guid ClientId { get; set; }
    }

    public class ReleaseUpdater : IEntity
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public int Number { get; set; }        
    }

    public class ReleaseCreator
    {
    }

    public class ReleaseArchitectUpdater : IEntity
    {
        public Guid Id { get; set; }
    }

    public class ReleaseArchitectCreator
    {
    }

    public class LoadHistory : Entity
    {
    }

    public class LoadHistoryCreator
    {
    }
}
