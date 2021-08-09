using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class Release : Entity
    {
        [Display(Name = "Каталог")]
        public string Path { get; set; }
        [Display(Name = "Версия")]
        public string Version { get; set; }
        public int Number { get; set; }
        [Display(Name = "Клиент")]
        public string Client { get; set; }
        [Display(Name = "ID клиента")]
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
        public Guid ClientId { get; set; }
    }

    public class ReleaseCreator
    {
        [Display(Name="Каталог")]
        public string Path { get; set; }
        [Display(Name = "Версия")]
        public string Version { get; set; }
        public int Number { get; set; }
        [Display(Name = "ID клиента")]
        public Guid ClientId { get; set; }
        [Display(Name = "Клиент")]
        public string Client { get; set; }
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

    public class LoadHistoryUpdater : IEntity
    {
        public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
