using SoftUpdater.Db.Attributes;
using System;

namespace SoftUpdater.Db.Model
{
    [TableName("h_client")]
    public class ClientHistory : EntityHistory, IIdentity
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