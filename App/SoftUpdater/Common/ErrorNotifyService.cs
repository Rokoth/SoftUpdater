using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUpdater.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SoftUpdater.Common
{
    public class ErrorNotifyService : IDisposable, IErrorNotifyService
    {       
        private bool isConnected = false;
        private bool isAuth = false;
        private bool isDisposed = false;
        private bool _sendMessage = false;

        private ILogger<ErrorNotifyService> _logger;

        private string _server;
        private string _login;
        private string _password;
        private string _feedback;
        private string _defaultTitle;

        private object _lockObject = new object();
        private bool isLock = false;

        private string _token { get; set; }

        public ErrorNotifyService(ErrorNotifyOptions options)
        {                    
            if (options.SendError)
            {
                if (!string.IsNullOrEmpty(options.Server))
                {
                    _sendMessage = true;
                    _server = options.Server;
                    _login = options.Login;
                    _password = options.Password;
                    _feedback = options.FeedbackContact;
                    _defaultTitle = options.DefaultTitle;

                    Task.Factory.StartNew(CheckConnect, TaskCreationOptions.LongRunning);
                }
                else
                {
                    throw new Exception($"ErrorNotifyService error: Options.Server not set");
                }
            }
        }

        public ErrorNotifyService(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<CommonOptions>>().Value.ErrorNotifyOptions;
            _logger = serviceProvider.GetRequiredService<ILogger<ErrorNotifyService>>();
            if (options.SendError)
            {
                if (!string.IsNullOrEmpty(options.Server))
                {
                    _sendMessage = true;
                    _server = options.Server;
                    _login = options.Login;
                    _password = options.Password;
                    _feedback = options.FeedbackContact;
                    _defaultTitle = options.DefaultTitle;

                    Task.Factory.StartNew(CheckConnect, TaskCreationOptions.LongRunning);
                }
                else
                {
                    _logger.LogError($"ErrorNotifyService error: Options.Server not set");
                }
            }
        }

        private async Task<bool> Auth()
        {
            bool _isLocked = false;
            lock (_lockObject)
            {
                if (isLock)
                {
                    _isLocked = true;
                }
                else
                {
                    isLock = true;
                }
            }
            if (_isLocked)
            {
                for (int i = 0; i < 60; i++)
                {
                    if (!isLock)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }
                if (!isLock)
                {
                    if (isAuth) return true;
                    if (isConnected) return false;
                }
                else
                {
                    if(_logger!=null) _logger.LogError($"ErrorNotifyService: Error in Auth method: cant wait for auth with lock");
                    return false;
                }
            }

            var result = await Execute(client =>
                client.PostAsync($"{_server}/api/v1/client/auth", new ErrorNotifyClientIdentity()
                {
                    Login = _login,
                    Password = _password
                }.SerializeRequest()), "Post", s => s.ParseResponse<ErrorNotifyClientIdentityResponse>(), false);
            if (result.ResponseCode == ResponseEnum.Error)
            {
                if (isConnected)
                {
                    if (_logger != null) _logger.LogError($"ErrorNotifyService: Error in Auth method: wrong login or password");
                    _sendMessage = false;
                }
                return false;
            }
            _token = result.ResponseBody.Token;
            isAuth = true;
            lock (_lockObject)
            {
                isLock = false;
            }
            return true;
        }

        public async Task Send(string message, MessageLevelEnum level = MessageLevelEnum.Error, string title = null)
        {
            if (_sendMessage)
            {
                var result = await Execute(client =>
                {
                    var request = new HttpRequestMessage()
                    {
                        Headers = {
                            { HttpRequestHeader.Authorization.ToString(), $"Bearer {_token}" },
                            { HttpRequestHeader.ContentType.ToString(), "application/json" },
                        },
                        RequestUri = new Uri($"{_server}/api/v1/message/send"),
                        Method = HttpMethod.Post,
                        Content = new MessageCreator()
                        {
                            Description = message,
                            FeedbackContact = _feedback,
                            Level = (int)level,
                            Title = title ?? _defaultTitle
                        }.SerializeRequest()
                    };

                    return client.SendAsync(request);
                }, "Send", s => s.ParseResponse<MessageCreator>());

                if (result.ResponseCode == ResponseEnum.Error)
                {
                    if (_logger != null) _logger.LogError($"ErrorNotifyService: Error in Send method: cant send message error");
                }
            }
        }

        private async Task<Response<T>> Execute<T>(
            Func<HttpClient, Task<HttpResponseMessage>> action,
            string method,
            Func<HttpResponseMessage, Task<Response<T>>> parseMethod, bool needAuth = true) where T: class
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    if (isConnected)
                    {
                        var result = await action(client);
                        var resp =  await parseMethod(result);
                        if (resp.ResponseCode == ResponseEnum.NeedAuth)
                        {
                            if (needAuth && await Auth())
                            {
                                result = await action(client);
                                resp = await parseMethod(result);
                            }
                            else
                            {
                                return new Response<T>()
                                {
                                    ResponseCode = ResponseEnum.Error
                                };
                            }
                        }
                        return resp;
                    }
                    if (_logger != null) _logger.LogError($"Error in {method}: server not connected");
                    return new Response<T>()
                    {
                        ResponseCode = ResponseEnum.Error
                    };
                }
                catch (Exception ex)
                {
                    if (_logger != null) _logger.LogError($"Error in {method}: {ex.Message}; StackTrace: {ex.StackTrace}");
                    return new Response<T>()
                    {
                        ResponseCode = ResponseEnum.Error
                    };
                }
            }
        }

        private async Task CheckConnect()
        {
            while (!isDisposed)
            {
                isConnected = await CheckConnectOnce(_server);
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
                    if (_logger != null) _logger.LogInformation($"Ping result: server {server} {(result ? "connected" : "disconnected")}");
                    return result;
                }
                catch (Exception ex)
                {
                    if (_logger != null) _logger.LogError($"Error in CheckConnect: {ex.Message}; StackTrace: {ex.StackTrace}");
                    return false;
                }
            }
        }

        public void Dispose()
        {
            isDisposed = true;
        }
    }

    public enum MessageLevelEnum
    {
        Issue = 0,
        Warning = 1,
        Error = 10
    }

    public class ErrorNotifyMessage
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public MessageLevelEnum MessageLevel { get; set; }
    }

    public class ErrorNotifyClientIdentity
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class ErrorNotifyClientIdentityResponse
    {
        public string Token { get; set; }
        public string UserName { get; set; }
    }

    public class MessageCreator
    {
        public int Level { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FeedbackContact { get; set; }
    }

    public class ErrorNotifyLoggerConfiguration
    {
        public int EventId { get; set; }

        public ErrorNotifyOptions Options { get; set; }

        public List<LogLevel> LogLevels { get; set; } = new List<LogLevel>()
        {
            LogLevel.Error,
            LogLevel.Critical
        };
    }

    public class ErrorNotifyLogger : ILogger
    {
        private readonly string _name;
        private IErrorNotifyService _errorNotifyService;
        private readonly Func<ErrorNotifyLoggerConfiguration> _getCurrentConfig;

        public ErrorNotifyLogger(string name, IErrorNotifyService errorNotifyService,
            Func<ErrorNotifyLoggerConfiguration> getCurrentConfig)
        {
            _errorNotifyService = errorNotifyService;
            _getCurrentConfig = getCurrentConfig;
            _name = name;
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel)
        {
            return _getCurrentConfig().LogLevels.Contains(logLevel);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            ErrorNotifyLoggerConfiguration config = _getCurrentConfig();
            if (config.EventId == 0 || config.EventId == eventId.Id)
            {
                try
                {
                    if (exception != null)
                    {
                        _errorNotifyService
                            .Send($"Message: {exception.Message} StackTrace: {exception.StackTrace}")
                            .ContinueWith(s =>
                            {
                                if (s.Exception != null)
                                {
                                    Console.WriteLine($"ErrorNotify exception: {s.Exception.Message} {s.Exception.StackTrace}");
                                }
                            })
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        _errorNotifyService
                               .Send($"Message: {state}")
                               .ContinueWith(s =>
                               {
                                   if (s.Exception != null)
                                   {
                                       Console.WriteLine($"ErrorNotify exception: {s.Exception.Message} {s.Exception.StackTrace}");
                                   }
                               })
                               .ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ErrorNotify exception: {ex.Message} {ex.StackTrace}");
                }
            }
        }
    }

    public sealed class ErrorNotifyLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private ErrorNotifyLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, ErrorNotifyLogger> _loggers = new ConcurrentDictionary<string, ErrorNotifyLogger>();


        public ErrorNotifyLoggerProvider(
            IOptionsMonitor<ErrorNotifyLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName)
        {
            var errorNotifyService = new ErrorNotifyService(_currentConfig.Options);
            var logger = _loggers.GetOrAdd(categoryName, name => new ErrorNotifyLogger(name, errorNotifyService, GetCurrentConfig));
            return logger;
        }

        private ErrorNotifyLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}
