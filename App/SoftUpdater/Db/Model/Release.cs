using SoftUpdater.Db.Attributes;
using System;

namespace SoftUpdater.Db.Model
{
    [TableName("release")]
    public class Release : Entity
    {        
        [ColumnName("path")]
        public string Path { get; set; }
        [ColumnName("version")]
        public string Version { get; set; }
        [ColumnName("number")]
        public int Number { get; set; }       
        [ColumnName("client_id")]
        public Guid ClientId { get; set; }       
    }
}