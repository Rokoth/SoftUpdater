using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftUpdater.Contract.Model
{
    public class Entity : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Display(Name = "Идентификатор")]
        public Guid Id { get; set; }
    }
}
