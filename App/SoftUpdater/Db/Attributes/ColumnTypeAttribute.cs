using System;

namespace SoftUpdater.Db.Attributes
{
    public class ColumnTypeAttribute : Attribute
    {
        public string Name { get; }

        public ColumnTypeAttribute(string name)
        {
            Name = name;
        }
    }
}
