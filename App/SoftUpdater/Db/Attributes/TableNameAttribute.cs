using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUpdater.Db.Attributes
{
    public class TableNameAttribute : Attribute
    {
        public string Name { get; }

        public TableNameAttribute(string name)
        {
            Name = name;
        }
    }

    public class IgnoreAttribute : Attribute
    {

    }

    public class PrimaryKeyAttribute : Attribute
    {

    }

    public class ColumnTypeAttribute : Attribute
    {
        public string Name { get; }

        public ColumnTypeAttribute(string name)
        {
            Name = name;
        }
    }

    public class ColumnNameAttribute : Attribute
    {
        public string Name { get; }

        public ColumnNameAttribute(string name)
        {
            Name = name;
        }
    }
}
