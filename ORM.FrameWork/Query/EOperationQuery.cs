using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Query
{
    // Enumeration which defines operations for the query
    internal enum EOperationQuery
    {
        NOP = 0, // nothing operation
        NOT = 1, // NOT operation
        AND = 2, // AND operation
        OR = 3, // OR operation
        GRPB = 4, // Group Begin operation
        GRPE = 5, // Group End operation
        EQ = 6, // Equals operation
        LIKE = 7, // LIKE operation
        IN = 8, // IN operation
        G = 9, // Greather Then operation
        L = 10 // Less Then operation
    }
}
