using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.Attributes
{
    //Attribute that represent a property as an foreign key field.
    public class ForeignKeyAttribute : FieldAttribute 
    {
        //public member that represents a foreign key in assigment table
        public string RemoteColumnName = null;

        //public member that represents a assigment table for m to n relations
        public string AssigmentTable = null;

    }
}
