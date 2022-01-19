using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Attributes
{
    //Attribute that represent a class as a material base entity
    public class MaterialAttribute : Attribute
    {
        //public member that represents a name of a table
        public string TableName;

        //public member that represents a subset of the entity table (where clause)
        public string SubsetQuery;
    }
}
