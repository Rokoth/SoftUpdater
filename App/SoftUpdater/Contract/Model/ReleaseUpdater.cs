using System;

namespace SoftUpdater.Contract.Model
{
    public class ReleaseUpdater : IEntity
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public int Number { get; set; }
        public Guid ClientId { get; set; }
        public string Client { get; set; }
    }
}
