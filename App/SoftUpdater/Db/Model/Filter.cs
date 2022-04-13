//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using System;
using System.Linq.Expressions;

namespace SoftUpdater.Db.Model
{
    public class Filter<T> where T : IEntity
    {
        public int? Page { get; set; }
        public int? Size { get; set; }
        public string Sort { get; set; }

        public Expression<Func<T, bool>> Selector { get; set; }
    }
}