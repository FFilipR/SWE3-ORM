using ORM_FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Cache
{
    public class CacheImpl : ICache
    {
  
        protected Dictionary<Type, Dictionary<object, object>> Caches = new Dictionary<Type, Dictionary<object, object>>();

        protected virtual Dictionary<object, object> GetCache(Type type)
        {
            if (Caches.ContainsKey(type)) 
                return Caches[type]; 

            Dictionary<object, object> objDict = new Dictionary<object, object>();
            Caches.Add(type, objDict);

            return objDict;
        }
        public virtual object Get(Type type, object pKey)
        {
            Dictionary<object, object> objDict = GetCache(type);

            if (objDict.ContainsKey(pKey)) 
                return objDict[pKey];

            return null;
        }

        public virtual void Remove(object obj)
        {
            var type = obj.GetType();
            var entity = ORMapper.GetEntity(obj);

            GetCache(type).Remove(entity.PKey.GetValue(obj));
        }
        public virtual bool Contains(Type type, object pKey)
        {
            return GetCache(type).ContainsKey(pKey);
        }


        public virtual void Put(object obj)
        {
            var type = obj.GetType();
            var entity = ORMapper.GetEntity(obj);

            if (obj != null) 
                GetCache(type)[entity.PKey.GetValue(obj)] = obj; 
        }

        
        public virtual bool HasChanged(object obj)
        {
            return true;
        }

        public virtual bool Contains(object obj)
        {
            var type = obj.GetType();
            var entity = ORMapper.GetEntity(obj);

            return GetCache(type).ContainsKey(entity.PKey.GetValue(obj));
        }

    }
}
