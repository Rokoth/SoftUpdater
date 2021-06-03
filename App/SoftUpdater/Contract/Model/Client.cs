using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class Client : Entity
    {
        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "User", ErrorMessage = "Имя уже используется")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Логин")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckLogin", "User", ErrorMessage = "Логин уже используется")]
        public string Login { get; set; }
        [Display(Name = "Id пользователя")]
        public Guid UserId { get; set; }
        [Display(Name = "Пользователь")]
        public string UserName { get; set; }
        [Display(Name = "Базовый каталог")]
        public string BasePath { get; set; }
    }

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

    public class ClientUpdater : IEntity
    {
        public Guid Id { get; set; }
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Логин")]
        public string Login { get; set; }
        [Display(Name = "Пароль")]
        public string Password { get; set; }
        public bool PasswordChanged { get; set; }
        public Guid UserId { get; set; }
        [Display(Name = "Пользователь")]
        public string UserName { get; set; }
        [Display(Name = "Базовый каталог")]
        public string BasePath { get; set; }
    }

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
