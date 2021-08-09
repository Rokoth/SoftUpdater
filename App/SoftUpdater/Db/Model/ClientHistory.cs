//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using SoftUpdater.Db.Attributes;
using System;

namespace SoftUpdater.Db.Model
{
    /// <summary>
    /// model for history table of "Client"
    /// </summary>
    [TableName("h_client")]
    public class ClientHistory : EntityHistory, IIdentity
    {
        /// <summary>
        /// "Client".Наименование
        /// </summary>
        [ColumnName("name")]
        public string Name { get; set; }
        /// <summary>
        /// "Client".Описание
        /// </summary>
        [ColumnName("description")]
        public string Description { get; set; }
        /// <summary>
        /// "Client".Логин
        /// </summary>
        [ColumnName("login")]
        public string Login { get; set; }
        /// <summary>
        /// "Client".Пароль
        /// </summary>
        [ColumnName("password")]
        public byte[] Password { get; set; }
        /// <summary>
        /// "Client".ИД пользователя
        /// </summary>
        [ColumnName("userid")]
        public Guid UserId { get; set; }
        /// <summary>
        /// "Client".Базовый каталог обновлений
        /// </summary>
        [ColumnName("base_path")]
        public string BasePath { get; set; }
    }
}