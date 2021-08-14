namespace SoftUpdater.Contract.Model
{
    public class ClientIdentity : IIdentity
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
