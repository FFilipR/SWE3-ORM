using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Cache
{
    public interface ICache
    { 
        object Get(Type t, object pk);
        void Put(object obj);

        void Remove(object obj);

        bool Contains(Type t, object pk);

        bool Contains(object obj);

        bool HasChanged(object obj);
    }
}
