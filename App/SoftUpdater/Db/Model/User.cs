//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using SoftUpdater.Db.Attributes;

namespace SoftUpdater.Db.Model
{
    [TableName("user")]
    public class User : Entity, IIdentity
    {
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }
        [ColumnName("login")]
        public string Login { get; set; }
        [ColumnName("password")]
        public byte[] Password { get; set; }
    }
}