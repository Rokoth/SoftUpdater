using System;
using System.Collections.Generic;

namespace SoftUpdater.Contract.Model
{
    public class ReleaseClient
    {
        public Guid Id { get; set; }
        public string Version { get; set; }
        public List<ReleaseArchitectClient> Architects { get; set; }
    }
}
