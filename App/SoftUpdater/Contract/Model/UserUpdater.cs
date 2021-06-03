using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class UserUpdater: IEntity
    {
        public Guid Id { get; set; }
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Логин")]
        public string Login { get; set; }
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool PasswordChanged { get; set; }
    }
}
