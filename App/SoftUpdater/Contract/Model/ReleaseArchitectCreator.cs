using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class ReleaseArchitectCreator
    {
        public Guid ReleaseId { get; set; }
        [Display(Name = "Релиз")]
        public string Release { get; set; }
        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckName", "ReleaseArchitect", ErrorMessage = "Имя уже используется", AdditionalFields ="ReleaseId")]
        public string Name { get; set; }
        [Display(Name = "Путь")]
        [Required(ErrorMessage = "Поле должно быть установлено")]
        [Remote("CheckPath", "ReleaseArchitect", ErrorMessage = "Путь уже используется", AdditionalFields = "ReleaseId")]
        public string Path { get; set; }

        [Display(Name = "Файл")]
        [Required(ErrorMessage = "Поле должно быть установлено")]        
        public IFormFile File { get; set; }
    }
}
