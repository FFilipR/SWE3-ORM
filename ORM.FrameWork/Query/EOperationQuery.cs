using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Query
{
    internal enum EOperationQuery
    {
        NOP = 0,
        NOT = 1,
        AND = 2,
        OR = 3,
        GRPB = 4,
        GRPE = 5,
        EQ = 6,
        LIKE = 7,
        IN = 8,
        G = 9,
        L = 10
    }
}
