using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.Attributes
{
    public class FieldAttribute : Attribute
    {
        public string ColumnName = null;
        public Type ColumnType = null;
        public bool Nullable = false;
    }
}
