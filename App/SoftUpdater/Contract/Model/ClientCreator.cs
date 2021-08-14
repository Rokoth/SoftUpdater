using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class ClientCreator
    {       
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Логин")]
        public string Login { get; set; }
        [Display(Name = "Пароль")]
        public string Password { get; set; }   
        [Required]
        public Guid UserId { get; set; }        
        [Display(Name = "Базовый каталог")]
        public string BasePath { get; set; }
    }
}
