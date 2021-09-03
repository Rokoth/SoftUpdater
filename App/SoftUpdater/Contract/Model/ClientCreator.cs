using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class ClientCreator
    {
        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "Client", ErrorMessage = "Имя уже используется")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Логин")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckLogin", "Client", ErrorMessage = "Логин уже используется")]
        public string Login { get; set; }
        [Display(Name = "Пароль")]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Id пользователя")]
        [Required]
        public Guid UserId { get; set; }
        [Display(Name = "Пользователь")]
        public string UserName { get; set; }
        [Display(Name = "Базовый каталог")]
        public string BasePath { get; set; }
    }
}
