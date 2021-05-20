using System;
using System.Collections.Generic;

namespace SoftUpdater.ClientHttpClient
{
    public interface IHttpClientSettings
    {
        Dictionary<Type, string> Apis { get; }
        string Server { get; }
    }
}
