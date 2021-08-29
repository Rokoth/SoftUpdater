using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class LoadHistory : Entity
    {
        [Display(Name = "ИД клиента")]
        public Guid ClientId { get; set; }
        [Display(Name = "ИД релиза")]
        public Guid ReleaseId { get; set; }
        [Display(Name = "ИД архитектуры")]
        public Guid ArchitectId { get; set; }
        [Display(Name = "Дата загрузки")]
        public DateTime LoadDate { get; set; }
        [Display(Name = "Успешно")]
        public bool Success { get; set; }

        [Display(Name = "Клиент")]
        public string Client { get; set; }
        [Display(Name = "Релиз")]
        public string Release { get; set; }
        [Display(Name = "Архитектура")]
        public string Architect { get; set; }
    }

    public class LoadHistoryCreator
    {
        [Display(Name = "ИД клиента")]
        public Guid ClientId { get; set; }
        [Display(Name = "ИД релиза")]
        public Guid ReleaseId { get; set; }
        [Display(Name = "ИД архитектуры")]
        public Guid ArchitectId { get; set; }
        [Display(Name = "Дата загрузки")]
        public DateTime LoadDate { get; set; }
        [Display(Name = "Успешно")]
        public bool Success { get; set; }

        [Display(Name = "Клиент")]
        public string Client { get; set; }
        [Display(Name = "Релиз")]
        public string Release { get; set; }
        [Display(Name = "Архитектура")]
        public string Architect { get; set; }
    }
}
