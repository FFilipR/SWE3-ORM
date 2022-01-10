using ORM_FrameWork;
using ORM_FrameWork.MetaModels;
using System;
using System.Collections.Generic;
using System.Collections;


namespace ORM.FrameWork.Loading
{
    public class LazyLoadingList<T> : ILazyLoading, IList<T>
    {
      

        protected List<T> InternalItems = null;
        protected string Sql;
        protected ICollection<Tuple<string, object>> Parameters;
        public string connectionString;

        internal protected LazyLoadingList(string sql, ICollection<Tuple<string, object>> parameters, string connectionString)
        {
            this.Sql = sql;
            this.Parameters = parameters;
            this.connectionString = connectionString;
        }

        public LazyLoadingList(object obj, string fieldName, string connectionString)
        {
            Field field = ORMapper.GetEntity(obj).GetFieldByName(fieldName);

            this.Sql = field.SqlFkey;
            this.Parameters = new Tuple<string, object>[] { new Tuple<string, object>("@fKey", field.Entity.PKey.GetValue(obj)) };
            this.connectionString = connectionString;

        }

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

        // IList methods implemenation
        public T this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        public int Count
        {
            get { return Items.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return ((IList<T>)Items).IsReadOnly; }
        }

        public void Add(T item)
        {
            Items.Add(item);
        }

        public void Clear()
        {
            Items.Clear();
        }


        public bool Contains(T item)
        {
            return Items.Contains(item);
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }


        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }


        public int IndexOf(T item)
        {
            return Items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Items.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return Items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
