//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref1

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
        public string ConnectionString { get; set; }
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
        public string InstallSchedule { get; set; }
        public string ApplicationDirectory { get; set; }
        public string BackupDirectory { get; set; }
        
    }
}