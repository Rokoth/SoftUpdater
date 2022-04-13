//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using System;

namespace SoftUpdater.Db.Model
{
    public interface IEntity
    {
        Guid Id { get; set; }
        bool IsDeleted { get; set; }
        DateTimeOffset VersionDate { get; set; }
    }
}