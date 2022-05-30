using System;
using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class ReleaseArchitectHistory : EntityHistory
    {
        [Display(Name = "ИД Релиза")]
        public Guid ReleaseId { get; set; }
                
        [Display(Name = "Имя")]        
        public string Name { get; set; }

        [Display(Name = "Путь")]       
        public string Path { get; set; }

        public string FileName { get; set; }
    }
}
