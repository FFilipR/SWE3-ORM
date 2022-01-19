using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Cache
{
    //Cache interface
    public interface ICache
    {
        // public method that takes a type and primary key and returns boolean (true/false) whether the object is in the cache
        bool Contains(Type type, object pKey);

        //public method that takes an type and primary key and returns a object from cache
        object Get(Type type, object pKey);

        // public method that takes a type and returns boolean (true/false) whether the object is in the cache
        bool Contains(object obj);

        // public method that takes an object and puts it into the cache
        void Put(object obj);

        // public method that takes an object and notifies (true/false)  whether it has changed or not
        bool HasChanged(object obj);

        // public method that takes an object and removes it from cache
        void Remove(object obj);
   
    }
}
