//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref1

using System.Collections.Generic;

namespace SoftUpdater.Common
{
    /// <summary>
    /// Класс хранения настроек
    /// </summary>
    public class CommonOptions
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        public Dictionary<string, string> ConnectionStrings { get; set; }
        public ErrorNotifyOptions ErrorNotifyOptions { get; set; }
        public string UploadBasePath { get; set; }
    }

    public class ClientOptions: CommonOptions
    { 
        public string CheckUpdateSchedule { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }        
        public string Architecture { get; set; }
        public string ReleasePath { get; set; }
        public string DownloadedVersionField { get; set; }
        public string InstalledVersionField { get; set; }

        public string CheckUpdateScheduleSelf { get; set; }
        public string LoginSelf { get; set; }
        public string PasswordSelf { get; set; }        
        public string ArchitectureSelf { get; set; }
        public string ReleasePathSelf { get; set; }
        public string DownloadedSelfVersionField { get; set; }
        public string InstalledSelfVersionField { get; set; }
        public string NextRunDateTimeField { get; set; }
        public string NextRunDateTimeSelfField { get; set; }
        public string InstallSchedule { get; set; }
        public string InstallSelfSchedule { get; set; }
        public string ApplicationDirectory { get; set; }
        public string BackupDirectory { get; set; }

        public string ApplicationSelfDirectory { get; set; }
        public string BackupSelfDirectory { get; set; }

        public string UpdateScript { get; set; }
        public string UpdateSelfScript { get; set; }
        public string ServiceName { get; set; }

        public RunMode Mode { get; set; }
        public string SelfUpdateTempDir { get; set; }
        public string Server { get; set; }
    }

    public enum RunMode
    { 
        Normal,
        SelfUpdate
    }

    public class ErrorNotifyOptions
    {
        public bool SendError { get; set; }
        public string Server { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string FeedbackContact { get; set; }
        public string DefaultTitle { get; set; }
    }
}