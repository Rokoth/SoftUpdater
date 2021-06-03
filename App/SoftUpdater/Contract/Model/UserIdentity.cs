using System.ComponentModel.DataAnnotations;

namespace SoftUpdater.Contract.Model
{
    public class UserIdentity : IIdentity
    {
        [Display(Name = "Логин")]
        public string Login { get; set; }
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
