//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using SoftUpdater.Db.Attributes;
using System;

namespace SoftUpdater.Db.Model
{
    [TableName("release_architect")]
    public class ReleaseArchitect : Entity 
    {
        [ColumnName("release_id")]
        public Guid ReleaseId { get; set; }
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("path")]
        public string Path { get; set; }
        [ColumnName("file_name")]
        public string FileName { get; set; }
    }
}