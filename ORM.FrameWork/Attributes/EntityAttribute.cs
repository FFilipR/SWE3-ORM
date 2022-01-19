using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.Attributes
{
    //Attribute that represent a class as an entity.
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        //public member that represents name of the table
        public string TableName ;

        //public member that represents a subset of the entity table (where clause)
        public string SubsetQuery;

        //public member that represents a foreign key of the master table
        public string ChildKey;

    }
}
