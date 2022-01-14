using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Locking
{
    public interface ILocking
    {
        void Lock(object obj);
        void Release(object obj);
    }
}
