using System;

namespace SoftUpdater.Contract.Model
{
    public class ReleaseHistory : EntityHistory
    {
        public Client Client { get; set; }
        public Guid ClientId { get; set; }
    }
}
