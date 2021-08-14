using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class ClientHistory : EntityHistory
    {
        [Display(Name = "Имя")]        
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Логин")]       
        public string Login { get; set; }
        [Display(Name = "Id пользователя")]
        public Guid UserId { get; set; }
        [Display(Name = "Пользователь")]
        public string UserName { get; set; }
        [Display(Name = "Базовый каталог")]
        public string BasePath { get; set; }
    }
}
