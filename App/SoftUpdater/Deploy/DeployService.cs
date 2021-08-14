//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Deployer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using System.Text.RegularExpressions;
using SoftUpdater.Common;

namespace SoftUpdater.Deploy
{
    /// <summary>
    /// Deploy lib wrapper
    /// </summary>
    public class DeployService : IDeployService
    {
        private readonly ILogger<DeployService> _logger;
        private readonly IErrorNotifyService _errorNotifyService;
        private string _connectionString;

        public DeployService(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<DeployService>>();
            _errorNotifyService = serviceProvider.GetRequiredService<IErrorNotifyService>();
            var _options = serviceProvider.GetRequiredService<IOptions<CommonOptions>>();
            _connectionString = _options.Value.ConnectionString;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString">connectionString</param>
        public DeployService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Deploy db
        /// </summary>
        /// <param name="num">number of last update</param>
        /// <returns></returns>
        public async Task Deploy(int? num = null)
        {
            var deployLog = string.Empty;
            try
            {
                CheckDbForExists(_connectionString);

                DeploySettings deploySettings = new DeploySettings()
                {
                    BeginNum = num,
                    CheckSqlPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Check"),
                    ConnectionString = _connectionString,
                    DeploySqlPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Deploy"),
                    UpdateSqlPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Update")
                };
                var deployer = new Deployer.Deployer(deploySettings);
                deployer.OnError += (sender, message) =>
                {
                    deployLog += message + "\r\n";
                    _logger?.LogError(message);
                    _errorNotifyService.Send(message);
                };
                deployer.OnDebug += (sender, message) =>
                {
                    _logger?.LogDebug(message);
                };
                deployer.OnMessage += (sender, message) =>
                {
                    _logger?.LogInformation(message);
                };
                deployer.OnWarning += (sender, message) =>
                {
                    deployLog += message + "\r\n";
                    _logger?.LogWarning(message);
                };

                if (!await deployer.Deploy())
                {                    
                    throw new DeployException($"DB was not deploy, log: {deployLog}");
                }
            }
            catch (DeployException ex)
            {
                await _errorNotifyService.Send($"DB was not deploy: {ex.Message} {ex.StackTrace}");
                throw;
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"" +
                    $"Error while Deploy DB.\r\n" +
                    $"Message: {ex.Message}\r\n" +
                    $"StackTrace: {ex.StackTrace}\r\n" +
                    $"DeployLog: {deployLog}");
                throw new DeployException($"" +
                    $"Error while Deploy DB.\r\n" +
                    $"Message: {ex.Message}\r\n" +
                    $"StackTrace: {ex.StackTrace}\r\n" +
                    $"DeployLog: {deployLog}");
            }
        }

        /// <summary>
        /// TODO: move to deploy lib
        /// </summary>
        /// <param name="connectionString"></param>
        private void CheckDbForExists(string connectionString)
        {
            try
            {
                var dbName = Regex.Match(connectionString, "Database=(.*?);").Groups[1].Value;
                var rootConnectionString = Regex.Replace(connectionString, "Database=.*?;", $"Database=postgres;");
                using NpgsqlConnection _connPg = new NpgsqlConnection(rootConnectionString);
                _connPg.Open();
                string script1 = $"select exists(SELECT 1 FROM pg_database WHERE datname = '{dbName}');";
                var cmd1 = new NpgsqlCommand(script1, _connPg);
                if (!(bool)cmd1.ExecuteScalar())
                {
                    string script2 = $"create database {dbName};";
                    var cmd2 = new NpgsqlCommand(script2, _connPg);
                    cmd2.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new DeployException($"Не удалось развернуть базу данных: " +
                    $"ошибка при проверке или создании базы: {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
