using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Attributes
{
    public class MaterialAttribute : Attribute
    {
        public string TableName;

        public string SubsetQuery;
    }
}
