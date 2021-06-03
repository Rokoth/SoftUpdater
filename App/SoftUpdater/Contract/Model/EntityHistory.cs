using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class EntityHistory : Entity
    {
        [Display(Name = "ИД")]
        public long HId { get; set; }
        [Display(Name = "Дата изменения")]
        public DateTimeOffset ChangeDate { get; set; }
        [Display(Name = "Удалённый")]
        public bool IsDeleted { get; set; }
    }
}
