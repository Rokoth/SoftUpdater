using SoftUpdater.Db.Attributes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SoftUpdater.Db.Model
{
    public abstract class Entity: IEntity
    {
        [PrimaryKey]
        [ColumnName("id")]
        public Guid Id { get; set; }
        [ColumnName("version_date")]
        public DateTimeOffset VersionDate { get; set; }
        [ColumnName("is_deleted")]
        public bool IsDeleted { get; set; }
    }

    public interface IEntity
    {
        Guid Id { get; set; }
        bool IsDeleted { get; set; }
        DateTimeOffset VersionDate { get; set; }
    }

    public class Filter<T> where T : IEntity
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public string Sort { get; set; }

        public Expression<Func<T, bool>> Selector { get; set; }
    }

    public interface IIdentity
    {
        string Login { get; set; }
        byte[] Password { get; set; }
    }

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

    [TableName("h_user")]
    public class UserHistory : EntityHistory
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

    public abstract class EntityHistory : IEntity
    {
        [PrimaryKey]
        [ColumnName("h_id")]
        public long HId { get; set; }
        [ColumnName("change_date")]
        public DateTimeOffset ChangeDate { get; set; }

        [ColumnName("id")]
        public Guid Id { get; set; }
        [ColumnName("version_date")]
        public DateTimeOffset VersionDate { get; set; }
        [ColumnName("is_deleted")]
        public bool IsDeleted { get; set; }
    }

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

    [TableName("release")]
    public class Release : Entity
    {
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("path")]
        public string Path { get; set; }
        [ColumnName("version")]
        public string Version { get; set; }
        [ColumnName("number")]
        public int Number { get; set; }       
        [ColumnName("client_id")]
        public Guid ClientId { get; set; }
        [Ignore]
        public List<ReleaseArchitect> ReleaseArchitects { get; set; }
    }

    [TableName("h_release")]
    public class ReleaseHistory : EntityHistory
    {
        public string Name { get; set; }
    }

    [TableName("load_history")]
    public class LoadHistory
    {
        
    }

    [TableName("release_architect")]
    public class ReleaseArchitect : Entity 
    {
        [ColumnName("release_id")]
        public Guid ReleaseId { get; set; }
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("path")]
        public string Path { get; set; }
    }

    [TableName("h_release_architect")]
    public class ReleaseArchitectHistory : EntityHistory
    {
        [ColumnName("release_id")]
        public Guid ReleaseId { get; set; }
        [ColumnName("name")]
        public string Name { get; set; }
        [ColumnName("path")]
        public string Path { get; set; }
    }
}