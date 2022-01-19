using ORM_FrameWork;
using ORM_FrameWork.MetaModels;
using System;
using System.Collections.Generic;
using System.Collections;


namespace ORM.FrameWork.Loading
{
    // Class of lazy loading lists 
    public class LazyLoadingList<T> : ILazyLoading, IList<T>
    {
        // protected list member of internal values
        protected List<T> InternalItems = null;

        // protected member that represents an sql 
        protected string Sql;

        // protected member tuple that represents parameters
        protected ICollection<Tuple<string, object>> Parameters;

        // protected member which represents a conneciton to database
        public string connectionString;

        // protected constructor that takes a sql , tuple of string-objects and connection string nad creates new instances 
        internal protected LazyLoadingList(string sql, ICollection<Tuple<string, object>> parameters, string connectionString)
        {
            this.Sql = sql;
            this.Parameters = parameters;
            this.connectionString = connectionString;
        }

        // public constructor that takes a object , name of the field and connection string nad creates new instances 
        public LazyLoadingList(object obj, string fieldName, string connectionString)
        {
            Field field = ORMapper.GetEntity(obj).GetFieldByName(fieldName);

            this.Sql = field.SqlFkey;
            this.Parameters = new Tuple<string, object>[] { new Tuple<string, object>("@fKey", field.Entity.PKey.GetValue(obj)) };
            this.connectionString = connectionString;

        }

        // protected  property of list of items that gets the values from the list
        protected List<T> Items
        {
            get
            {
                if (InternalItems == null)
                {
                    InternalItems = new List<T>();
                    ORMapper.ListFiller(typeof(T), InternalItems, Sql, Parameters, connectionString, null);
                }

                return InternalItems;
            }
        }

        // IList methods implemenations

        // public property that takes an index and gets or sets a item by it
        public T this[int index]
        {
          get 
            { 
               return Items[index]; 
            }
          set 
            { 
                Items[index] = value; 
            }
        }

        // public property that gets a number of items in a list
        public int Count
        {
           get 
            { 
                return Items.Count; 
            }
        }

        // public property that gets a list if it's read only
        bool ICollection<T>.IsReadOnly
        {
            get 
            { 
                return ((IList<T>)Items).IsReadOnly; 
            }
        }

        // public method that adss a method to the list
        public void Add(T item)
        {
            Items.Add(item);
        }

        // public method that clears the whole list
        public void Clear()
        {
            Items.Clear();
        }


        // public method that returns a boolean (true/false) if the list contains a given item
        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        // public method that takes a array and index of the array and copies the list to the array
        public void CopyTo(T[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        // public method that retuns retunts a IEnumerator for the list
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }


        // public method that returns a index of the given item in the list
        public int IndexOf(T item)
        {
            return Items.IndexOf(item);
        }

        // public method that inserts a item at the given index
        public void Insert(int index, T item)
        {
            Items.Insert(index, item);
        }

        // public boolean method that removes the given item from the list and retuns (true/false) whether it was successful or not.
        public bool Remove(T item)
        {
            return Items.Remove(item);
        }


        // public method that removas an item with on given index in the list
        public void RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        // public method that returns a enumerator of the lsit
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
