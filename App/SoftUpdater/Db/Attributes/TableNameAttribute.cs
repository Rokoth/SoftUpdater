//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using System;

namespace SoftUpdater.Db.Attributes
{
    /// <summary>
    /// Атрибут Имя таблицы (используется в контексте БД в методе создания моделей)
    /// </summary>
    public class TableNameAttribute : Attribute
    {
        /// <summary>
        /// Наименование таблицы
        /// </summary>
        public string Name { get; }

        public TableNameAttribute(string name)
        {
            Name = name;
        }
    }
}
