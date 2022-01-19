using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Locking
{
    // Class that implements a locking exception
    public class LockingException : Exception
    {
        // public exception which is thrown when the object that we are trying to lock is already locked
        public LockingException() : base("Another session locked the object.") {}
    }
}
