using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.Attributes
{
    public class ForeignKeyAttribute : FieldAttribute 
    {
        public string RemoteColumnName = null;
        public string AssigmentTable = null;

    }
}
