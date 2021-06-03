namespace SoftUpdaterClient.Service
{
    public class UpdateOptions
    {
        public string CheckUpdateSchedule { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }        
        public string Architecture { get; set; }
        public string ReleasePath { get; set; }
    }
}
