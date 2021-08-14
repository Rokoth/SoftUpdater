using System;
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

    public class LoadHistoryCreator
    {
    }

    public class LoadHistoryUpdater : IEntity
    {
        public Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
