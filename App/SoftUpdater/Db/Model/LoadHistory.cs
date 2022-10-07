//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using SoftUpdater.Db.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System;

namespace SoftUpdater.Db.Model
{
    [TableName("load_history")]
    public class LoadHistory : Entity
    {
        [ColumnName("client_id")]
        public Guid ClientId { get; set; }
        [ColumnName("release_id")]
        public Guid ReleaseId { get; set; }
        [ColumnName("architect_id")]
        public Guid ArchitectId { get; set; }
        [ColumnName("load_date")]
        public DateTime LoadDate { get; set; }
        [ColumnName("success")]
        public bool Success { get; set; }     
    }
}