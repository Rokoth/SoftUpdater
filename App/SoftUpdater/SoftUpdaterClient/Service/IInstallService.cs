namespace SoftUpdaterClient.Service
{
    public interface IInstallService
    {
        bool Install(InstallSettings settings);
    }
}