namespace SoftUpdaterClient.Service
{
    public interface IInstallService
    {
        bool Install(InstallSettings settings);
    }

    public interface IInstallSelfService
    {
        bool Install(InstallSettings settings);
    }
}