using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.Attributes
{
    // Atribute that represent a class as a field.
    public class FieldAttribute : Attribute
    {
        //public member that represents the name of the coulmnt of the database
        public string ColumnName = null;

        //public member that represents the type of the coulmnt of the database
        public Type ColumnType = null;

        //public member that represents if the table is nullable
        public bool Nullable = false;
    }
}
