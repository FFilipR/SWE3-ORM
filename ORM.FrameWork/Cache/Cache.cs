using ORM_FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Cache
{
    //Class that represents a implementation of a default cache 
    public class Cache : ICache
    {
        // protected dictionary of caches
        protected Dictionary<Type, Dictionary<object, object>> Caches = new Dictionary<Type, Dictionary<object, object>>();

        // protected dictionary that takes a type and returns a type cache.
        protected virtual Dictionary<object, object> GetCache(Type type)
        {
            if (Caches.ContainsKey(type))
                return Caches[type];

            Dictionary<object, object> objDict = new Dictionary<object, object>();
            Caches.Add(type, objDict);

            return objDict;
        }

        //public method that takes an type and primary key and returns a object from cache
        public virtual object Get(Type type, object pKey)
        {
            Dictionary<object, object> objDict = GetCache(type);

            if (objDict.ContainsKey(pKey))
                return objDict[pKey];

            return null;
        }

        // public method that takes an object and removes it from cache
        public virtual void Remove(object obj)
        {
            var type = obj.GetType();
            var entity = ORMapper.GetEntity(obj);

            GetCache(type).Remove(entity.PKey.GetValue(obj));
        }

        // public method that takes a type and primary key and returns boolean (true/false) whether the object is in the cache
        public virtual bool Contains(Type type, object pKey)
        {
            return GetCache(type).ContainsKey(pKey);
        }

        // public method that takes an object nad puts it into the cache
        public virtual void Put(object obj)
        {
            var type = obj.GetType();
            var entity = ORMapper.GetEntity(obj);

            if (obj != null)
                GetCache(type)[entity.PKey.GetValue(obj)] = obj;
        }

        // public method that takes an object and notifies (true/false)  whether it has changed or not
        public virtual bool HasChanged(object obj)
        {
            return true;

        }

        // public method that takes a type and returns boolean (true/false) whether the object is in the cache
        public virtual bool Contains(object obj)
        {
            var type = obj.GetType();
            var entity = ORMapper.GetEntity(obj);

            return GetCache(type).ContainsKey(entity.PKey.GetValue(obj));
        }


    }
}
