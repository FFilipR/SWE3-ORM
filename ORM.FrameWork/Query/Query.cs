using ORM_FrameWork;
using ORM_FrameWork.MetaModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Query
{
    public sealed class Query<T> : IEnumerable<T>
    {
        private object[] Arguments = null;
        private Query<T> Previous;
        private EOperationQuery Operation = EOperationQuery.NOP;
        private List<T> IntValues = null;
        public string ConnectionString { get; set;  }
        internal Query(Query<T> previous, string connectionString)
        {
            this.Previous = previous;
            this.ConnectionString = connectionString;
        }

        private List<T> Values
        {
            get
            {
                if (IntValues == null)
                {
                    IntValues = new List<T>();

                    if (typeof(T).IsAbstract || ORMapper.GetEntity(typeof(T)).IsMaterial)
                    {
                        ICollection<object> cache = null;
                        foreach (Type t in ORMapper.GetTypeOfChild(typeof(T)))
                           setValues(t, cache);                       
                    }
                    else  
                        setValues(typeof(T), null); 
                }
                return IntValues;         
            }
        }
        private Query<T> SetOperation (EOperationQuery operation, params object[] arguments)
        {
            this.Operation = operation;
            this.Arguments = arguments;
            return new Query<T>(this, ConnectionString);
        }

        public Query<T>Not()
        {
            return SetOperation(EOperationQuery.NOT);
        }
        public Query<T> And()
        {
            return SetOperation(EOperationQuery.AND);
        }
        public Query<T> Or()
        {
            return SetOperation(EOperationQuery.OR);
        }
        public Query<T> GroupBegin()
        {
            return SetOperation(EOperationQuery.GRPB);
        }
        public Query<T> GroupEnd()
        {
            return SetOperation(EOperationQuery.GRPE);
        }
        public Query<T> Equals(string field, object val, bool caseIgnore = false)
        {
            return SetOperation(EOperationQuery.EQ, field, val, caseIgnore);
        }

        public Query<T> Like(string field, object val, bool caseIgnore = false)
        {
            return SetOperation(EOperationQuery.LIKE, field, val, caseIgnore);
        }
        public Query<T> IN(string field, params object[] val)
        {
            List<object> list = new List<object>(val);
            list.Insert(0, field);
            return SetOperation(EOperationQuery.IN, list.ToArray());
        }
        public Query<T> Greater(string field, object val)
        {
            return SetOperation(EOperationQuery.G, field, val);
        }
        public Query<T> Less(string field, object val)
        {
            return SetOperation(EOperationQuery.L, field, val);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public List<T> ToList()
        {
            return new List<T>(Values);
        }

        private void setValues(Type type, ICollection<object> cache)
        {
           
                List<Query<T>> queries = new List<Query<T>>();
                Query<T> query = this;
                while (query != null)
                {
                    queries.Insert(0, query);
                    query = query.Previous;
                }

                Entity entity = ORMapper.GetEntity(type);
                string sql = entity.GetSql();

                List<Tuple<string, object>> parameters = new List<Tuple<string, object>>();
                string merge = string.IsNullOrWhiteSpace(entity.SubsetQuery) ? " WHERE (" : " AND (";
                bool not = false;
                int n = 0;
                string opened = "";
                string closed = "";
                string operation;

                Field field;
                foreach (Query<T> q in queries)
                {
                    switch (q.Operation)
                    {
                        case EOperationQuery.OR:
                            if (!merge.EndsWith("("))
                                merge = " OR ";
                            break;
                        case EOperationQuery.NOT:
                            not = true;
                            break;
                        case EOperationQuery.GRPB:
                            opened += "(";
                            break;
                        case EOperationQuery.GRPE:
                            closed += ")";
                            break;
                        case EOperationQuery.EQ:
                        case EOperationQuery.LIKE:
                            field = entity.GetFieldByName((string)q.Arguments[0]);
                            if (q.Operation == EOperationQuery.LIKE)
                                operation = not ? " NOT LIKE " : " LIKE ";
                            else
                                operation = not ? " != " : " = ";
                            sql += closed + merge + opened;
                        sql += (((bool)q.Arguments[2] ? "Lower(" + field.ColumnName + ")" : field.ColumnName) + operation +
                                ((bool)q.Arguments[2] ? "Lower(@p" + n.ToString() + ")" : "@p" + n.ToString()));

                        if ((bool)q.Arguments[2])
                                q.Arguments[1] = ((string)q.Arguments[1]).ToLower();
                            parameters.Add(new Tuple<string, object>("@p" + n++.ToString(), field.ToColumnType(q.Arguments[1])));

                            opened = closed = "";
                            merge = " AND ";
                            not = false;
                            break;
                        case EOperationQuery.IN:
                            field = entity.GetFieldByName((string)q.Arguments[0]);
                            sql += closed + merge + opened;
                            sql += field.ColumnName + (not ? " NOT IN (" : " IN (");
                            for (int i = 1; i < q.Arguments.Length; i++)
                            {
                                if (i > 1)
                                    sql += ", ";
                                sql += "@p" + n.ToString();
                                parameters.Add(new Tuple<string, object>("@p" + n++.ToString(), field.ToColumnType(q.Arguments[i])));
                            }
                            sql += ")";
                            opened = closed = "";
                            merge = " AND ";
                            not = false;
                            break;
                        case EOperationQuery.G:
                        case EOperationQuery.L:
                            field = entity.GetFieldByName((string)q.Arguments[0]);
                            if (q.Operation == EOperationQuery.G)
                                operation = not ? " <= " : " > ";
                            else
                                operation = not ? " >= " : " < ";
                            sql += closed + merge + opened;
                            sql += field.ColumnName + operation + "@p" + n.ToString();
                            parameters.Add(new Tuple<string, object>("@p" + n++.ToString(), field.ToColumnType(q.Arguments[1])));

                            opened = closed = "";
                            merge = " AND ";
                            not = false;
                            break;
                    }                
                }
                    
                if (!merge.EndsWith("(") )  
                    sql += ")"; 

                ORMapper.ListFiller(type, IntValues, sql, parameters, ConnectionString, cache);               
        }
    }
}
