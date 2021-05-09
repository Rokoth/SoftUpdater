using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class UserHistory : EntityHistory
    {
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Логин")]
        public string Login { get; set; }
    }
}
