using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Locking
{
    // Interface for database locking
    public interface ILocking
    {
        // method that takes an object and locks it
        void Lock(object obj);

        // method that takes an object and releases it
        void Release(object obj);
    }
}
