using ORM_FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Loading
{
    //Class of lazy loading objects
    public class LazyLoadingObject<T> : ILazyLoading
    {
        // protected member that represent a primary key
        protected object pKey;

        // protected member that represents a value of type [T]
        protected T value;

        // protected member that represent if the object is initialized
        protected bool isInitialized = false;
        
        //protected member that represents a connection string to the database
        protected string connectionString;

        // public constructor that takes a connection string and primary key creates a new instances 
        public LazyLoadingObject (string connectionString = null, object pKey = null)
        {
            this.connectionString = connectionString;
            this.pKey = pKey;

        }

        // public getter that gets a value of the object, and sets the given one
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

        // public operators which are used for  the lazy loading class
        public static implicit operator T(LazyLoadingObject<T> lazyObj)
        {
            return lazyObj.value;
        }
        public static implicit operator LazyLoadingObject<T> (T lazyObj)
        {
            LazyLoadingObject<T> val = new LazyLoadingObject<T>();
            val.Value = lazyObj;
            return val;
        }
    }
}
