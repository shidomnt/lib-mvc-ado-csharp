using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DBLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed public class ColumnAttribute : Attribute
    {
        readonly string columnName;

        public ColumnAttribute (string columnName)
        {
            this.columnName = columnName;
            DataType = SqlDbType.NVarChar;

        }

        public ColumnAttribute ()
        {
            this.columnName = "";

        }

        public string ColumnName
        {
            get
            {
                return columnName;
            }
        }

        public SqlDbType DataType
        {
            get;
            set;
        }
    }
}
