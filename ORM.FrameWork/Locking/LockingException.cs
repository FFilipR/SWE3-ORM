using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Locking
{
    public class LockingException : Exception
    {
        public LockingException() : base("Another session locked the object.") {}
    }
}
