using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SoftUpdater.Contract.Model;

using System.Net;
using System.IO;
using SoftUpdater.Common;

namespace SoftUpdater.ClientHttpClient
{

    public class ClientHttpClient : IClientHttpClient
    {
        private readonly ILogger<ClientHttpClient> _logger;
        private bool isConnected = false;
        private bool isDisposed = false;
        private bool isCheckRun = false;
        private readonly Dictionary<Type, string> _apis;
        private string _server;

        private string _token { get; set; }

        public event EventHandler OnConnect;

        public bool IsConnected => isConnected;

        public ClientHttpClient(IHttpClientSettings settings, IServiceProvider serviceProvider)
        {
            _apis = settings.Apis;
            _logger = serviceProvider.GetRequiredService<ILogger<ClientHttpClient>>();
            if (!string.IsNullOrEmpty(settings.Server))
            {
                _server = settings.Server;
                Task.Factory.StartNew(CheckConnect, TaskCreationOptions.LongRunning);
                isCheckRun = true;
            }
        }

        public void ConnectToServer(string server, Action<bool, bool, string> onResult)
        {
            CheckConnectOnce(server).ContinueWith(s =>
            {
                if (s.IsFaulted)
                {
                    var message = "";
                    if (s.Exception is AggregateException aex)
                    {

                        var stack = "";
                        foreach (var ex in aex.InnerExceptions)
                        {
                            message += ex.Message + "\r\n";
                            stack += ex.StackTrace + "\r\n";
                        }

                        _logger.LogError($"Error in ConnectToServer: {message}; StackTrace: {stack}");
                    }
                    else
                    {
                        message = s.Exception.Message;
                        _logger.LogError($"Error in ConnectToServer: {s.Exception.Message}; StackTrace: {s.Exception.StackTrace}");
                    }
                    onResult?.Invoke(false, true, $"Error in ConnectToServer: {message};");
                }
                else
                {
                    var result = s.Result;
                    onResult?.Invoke(result, false, null);
                    if (result)
                    {
                        _server = server;
                        isConnected = true;
                        if (!isCheckRun)
                        {
                            Task.Factory.StartNew(CheckConnect, TaskCreationOptions.LongRunning);
                            isCheckRun = true;
                        }
                    }
                }
            });
        }

        public async Task<bool> Auth(ClientIdentity identity)
        {
            var result = await Execute(client =>
                client.PostAsync($"{GetApi<ClientIdentity>()}", identity.SerializeRequest()), "Post", s => s.ParseResponse<ClientIdentityResponse>(), false);
            if (result.ResponseCode == ResponseEnum.Error) return false;
            _token = result.ResponseBody.Token;
            return true;
        }

        public async Task<bool> SendErrorMessage(string message)
        {
            var result = (await Execute(client =>
            {
                var request = new HttpRequestMessage()
                {
                    Headers = {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {_token}" },
                            { HttpRequestHeader.ContentType.ToString(), "application/json" },
                        },
                    RequestUri = new Uri($"{_server}/api/v1/common/send_error"),
                    Method = HttpMethod.Post,
                    Content = new ErrorNotifyMessage()
                    { 
                       Message = message,
                       MessageLevel = MessageLevelEnum.Error,
                       Title = "Ошибка в SoftUpdater client"
                    }.SerializeRequest()
                };

                return client.SendAsync(request);
            }, "SendErrorMessage", async s => {
                await Task.CompletedTask;
                if (s.IsSuccessStatusCode)
                {                    
                    return new Response<object> { 
                      ResponseCode = ResponseEnum.OK
                    };
                }
                if (s.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new Response<object>
                    {
                        ResponseCode = ResponseEnum.NeedAuth
                    };
                }
                return new Response<object>
                {
                    ResponseCode = ResponseEnum.Error
                };
            }));


            if (result.ResponseCode != ResponseEnum.Error)
            {
                return true;
            }
            return false;
        }

        private async Task<T> Execute<T>(
            Func<HttpClient, Task<HttpResponseMessage>> action,
            string method,
            Func<HttpResponseMessage, Task<T>> parseMethod, bool needAuth = true)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var result = await action(client);
                    return await parseMethod(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in {method}: {ex.Message}; StackTrace: {ex.StackTrace}");
                    return default;
                }
            }
        }

        private string GetApi<T>(Type apiType = null) where T : class
        {
            return $"{_server}/{_apis[apiType ?? typeof(T)]}";
        }

        private async Task CheckConnect()
        {
            while (!isDisposed)
            {
                var curConnect = isConnected;
                isConnected = await CheckConnectOnce(_server);
                if (isConnected && !curConnect)
                {
                    OnConnect?.Invoke(this, new EventArgs());
                }
                await Task.Delay(1000);
            }
        }

        private async Task<bool> CheckConnectOnce(string server)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var check = await client.GetAsync($"{server}/api/v1/common/ping");
                    var result = check != null && check.IsSuccessStatusCode;
                    _logger.LogInformation($"Ping result: server {server} {(result ? "connected" : "disconnected")}");
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in CheckConnect: {ex.Message}; StackTrace: {ex.StackTrace}");
                    return false;
                }
            }
        }

        public void Dispose()
        {
            isDisposed = true;
        }

        public async Task<ReleaseClient> GetLastRelease(string currentVersion)
        {
            var result = (await Execute(client =>
            {
                var request = new HttpRequestMessage()
                {
                    Headers = {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {_token}" },
                            { HttpRequestHeader.ContentType.ToString(), "application/json" },
                        },
                    RequestUri = new Uri($"{GetApi<ReleaseClient>()}"),
                    Method = HttpMethod.Get
                };

                return client.SendAsync(request);
            }, "GetReleases", s => s.ParseResponseArray<ReleaseClient>()));


            if (result.ResponseCode != ResponseEnum.Error)
            {
                var resp = result.ResponseBody.ToList();
                var lastVersion = resp.First();
                for (int i = 1; i < resp.Count(); i++)
                {
                    if (VersionCompare(resp[i].Version, lastVersion.Version))
                    {
                        lastVersion = resp[i];
                    }
                }
                return lastVersion;
            }
            return null;
        }

        private bool VersionCompare(string downloadedVersion, string installedVersion)
        {
            try
            {
                if (string.IsNullOrEmpty(downloadedVersion)) return false;
                if (string.IsNullOrEmpty(installedVersion)) return true;
                if (downloadedVersion == installedVersion) return false;
                var downLoaded = downloadedVersion.Split('.').Select(s => int.Parse(s)).ToArray();
                var installed = installedVersion.Split('.').Select(s => int.Parse(s)).ToArray();

                for (int i = 0; i < downLoaded.Length; i++)
                {
                    if (installed.Length < i) return true;
                    if (downLoaded[i] > installed[i]) return true;
                    if (downLoaded[i] < installed[i]) return false;
                }
                return false;
            }
            catch
            {
                return downloadedVersion != installedVersion;
            }
        }

        public async Task<Stream> DownloadRelease(Guid id)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var request = new HttpRequestMessage()
                    {
                        Headers = {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {_token}" },
                            { HttpRequestHeader.ContentType.ToString(), "application/json" },
                        },
                        RequestUri = new Uri($"{GetApi<ReleaseArchitect>()}?id={id}"),
                        Method = HttpMethod.Get                        
                    };

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        var contentStream = await content.ReadAsStreamAsync(); 
                        return contentStream;
                    }
                    else
                    {
                        throw new FileNotFoundException();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in  DownloadRelease: {ex.Message}; StackTrace: {ex.StackTrace}");
                    return null;
                }
            }            
        }
    }











    
}
