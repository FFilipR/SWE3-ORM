using ORM_FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Loading
{
    public class LazyLoadingObject<T> : ILazyLoading
    {
        protected object pKey;
        protected T value;
        protected bool isInitialized = false;
        protected string connectionString;
        public LazyLoadingObject (string connectionString = null, object pKey = null)
        {
            this.connectionString = connectionString;
            this.pKey = pKey;

        }

        public T Value
        {
            set
            {
                this.value = value;
                isInitialized = true;
            }
            get
            {
                if (!isInitialized)
                {
                    this.value = ORMapper.GetByID<T>(pKey, connectionString);
                    isInitialized = true;
                }
                return value;
            }
        }

        public static implicit operator T(LazyLoadingObject<T> lazyObj)
        {
            return lazyObj.value;
        }

        public static implicit operator LazyLoadingObject<T> (T obj)
        {
            LazyLoadingObject<T> val = new LazyLoadingObject<T>();
            val.Value = obj;
            return val;
        }
    }
}
