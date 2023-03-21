using System;

namespace IntelliMed.Core.Helpers
{
    [AttributeUsage(AttributeTargets.All)]
    public class ExcelColumnAttribute : Attribute
    {
        public int ColumnIndex { get; set; }


        public ExcelColumnAttribute(int column)
        {
            ColumnIndex = column;
        }
    }
}
