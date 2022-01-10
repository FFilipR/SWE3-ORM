using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Cache
{
    public interface ICache
    {
        bool Contains(Type type, object pKey);
        object Get(Type type, object pKey);
        bool Contains(object obj);
        void Put(object obj);
        bool HasChanged(object obj);
        void Remove(object obj);
   
    }
}
