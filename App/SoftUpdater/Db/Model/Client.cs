//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using SoftUpdater.Db.Attributes;
using System;

namespace SoftUpdater.Db.Model
{
    [TableName("client")]
    public class Client : Entity, IIdentity
    {
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }
        [ColumnName("login")]
        public string Login { get; set; }
        [ColumnName("password")]
        public byte[] Password { get; set; }       
        [ColumnName("userid")]
        public Guid UserId { get; set; }
        [ColumnName("base_path")]
        public string BasePath { get; set; }
    }
}