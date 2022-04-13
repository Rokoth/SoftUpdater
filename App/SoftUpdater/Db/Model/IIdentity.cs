//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1

namespace SoftUpdater.Db.Model
{
    public interface IIdentity
    {
        string Login { get; set; }
        byte[] Password { get; set; }
    }
}