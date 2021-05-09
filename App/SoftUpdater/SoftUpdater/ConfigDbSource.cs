///Copyright 2021 Dmitriy Rokoth
///Licensed under the Apache License, Version 2.0
///
///ref 1

using Microsoft.Extensions.Configuration;
using System;
using Microsoft.EntityFrameworkCore;
using SoftUpdater.Deploy;

namespace SoftUpdater.SoftUpdaterHost
{
    public class ConfigDbSource : IConfigurationSource
    {
        private readonly Action<DbContextOptionsBuilder> _optionsAction;
        private string _connectionString;

        public ConfigDbSource(Action<DbContextOptionsBuilder> optionsAction, string connectionString)
        {
            _optionsAction = optionsAction;
            _connectionString = connectionString;
        }

        public Microsoft.Extensions.Configuration.IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            IDeployService deployService = new DeployService(_connectionString);
            return new ConfigDbProvider(_optionsAction, deployService);
        }
    }
}
