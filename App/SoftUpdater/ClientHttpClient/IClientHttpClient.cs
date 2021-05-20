using SoftUpdater.Contract.Model;
using System;
using System.Threading.Tasks;

namespace SoftUpdater.ClientHttpClient
{
    public interface IClientHttpClient
    {
        bool IsConnected { get; }

        event EventHandler OnConnect;

        Task<bool> Auth(ClientIdentity identity);
        void ConnectToServer(string server, Action<bool, bool, string> onResult);
        void Dispose();
    }
}