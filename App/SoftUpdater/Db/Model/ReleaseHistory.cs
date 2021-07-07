using SoftUpdater.Db.Attributes;
using System;

namespace SoftUpdater.Db.Model
{
    [TableName("h_release")]
    public class ReleaseHistory : EntityHistory
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