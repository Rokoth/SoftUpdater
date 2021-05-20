using System;

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


    public class ReleaseHistory : EntityHistory
    {
        public Client Client { get; set; }
        public Guid ClientId { get; set; }
    }

    public class ReleaseUpdater : IEntity
    {
        public Guid Id { get; set; }
    }

    public class ReleaseCreator
    {
    }

    public class LoadHistory : Entity
    {
    }
}
