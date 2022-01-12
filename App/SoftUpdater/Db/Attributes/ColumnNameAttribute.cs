//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using System;

namespace SoftUpdater.Db.Attributes
{
    /// <summary>
    /// Атрибут Наименование колонки
    /// </summary>
    public class ColumnNameAttribute : Attribute
    {
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name"></param>
        public ColumnNameAttribute(string name)
        {
            Name = name;
        }
    }
}
