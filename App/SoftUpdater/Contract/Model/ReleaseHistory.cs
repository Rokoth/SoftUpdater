using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class ReleaseHistory : EntityHistory
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
}
