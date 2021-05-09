using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class ReleaseArchitect : Entity
    {
        public Guid ReleaseId { get; set; }
        [Display(Name = "Релиз")]
        public Release Release { get; set; }
        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "ReleaseArchitect", ErrorMessage = "Имя уже используется")]
        public string Name { get; set; }
        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckPath", "ReleaseArchitect", ErrorMessage = "Путь уже используется")]
        public string Path { get; set; }
    }
}
