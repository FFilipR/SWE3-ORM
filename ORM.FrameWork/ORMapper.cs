using Npgsql;
using ORM.FrameWork.Cache;
using ORM.FrameWork.Loading;
using ORM.FrameWork.Locking;
using ORM.FrameWork.Query;
using ORM_FrameWork.MetaModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork
{
    public class ORMapper
    {
        private static Dictionary<Type, Entity> Entities = new Dictionary<Type, Entity>();
        public static NpgsqlConnection DbConnection { get; set; }

        public static ICache Cache { get; set; }

        public static ILocking Locking { get; set; } 
        internal static Entity GetEntity(object obj)
        {
            Type type = ((obj is Type) ? (Type)obj : obj.GetType()); // if obj then GetType , otherwise type

            if (!Entities.ContainsKey(type))
                Entities.Add(type, new Entity(type));

            return Entities[type];   // return value from dictionary by key
        }


        public static void SaveToDb(object obj, string connectionString)
        {
            if (Cache != null)
            {
                if (!Cache.HasChanged(obj))
                    return;
            }

            Entity entity = GetEntity(obj);

            DbConnection = new NpgsqlConnection(connectionString);
            // DbConnection.ConnectionString = connectionString;
            var command = new NpgsqlCommand();

            command.Connection = DbConnection;

            DbConnection.Open();
            command = DbConnection.CreateCommand();

            command.CommandText = $"INSERT INTO {entity.TableName} (";

            string conflict = $"ON CONFLICT ({entity.PKey.ColumnName }) DO UPDATE SET ";
            string insert = string.Empty;

            bool fr = true;

            for (int f = 0; f < entity.IntFields.Length; f++)
            {

                if (f > 0)
                {
                    command.CommandText += ", ";
                    insert += ", ";
                }
                command.CommandText += entity.IntFields[f].ColumnName;

                insert += $"@insert{f}";

                object field = entity.IntFields[f].ToColumnType(entity.IntFields[f].GetValue(obj));

                if (field == null)
                    field = DBNull.Value;

                command.Parameters.Add(new NpgsqlParameter($"@insert{ f }", field));



                if (!entity.IntFields[f].IsPkey)
                {

                    if (fr)
                        fr = false;
                    else
                        conflict += ", ";

                    conflict += $"{entity.IntFields[f].ColumnName} = @conflict{f}";

                    field = entity.IntFields[f].ToColumnType(entity.IntFields[f].GetValue(obj));

                    if (field == null)
                        field = DBNull.Value;

                    command.Parameters.Add(new NpgsqlParameter("@conflict" + f.ToString(), field));
                }
            }
            command.CommandText += $") VALUES ({insert}) {conflict}";

            command.ExecuteNonQuery();
            command.Parameters.Clear();

            command.Dispose();
            DbConnection.Close();



            foreach (Field f in entity.ExtFields)
                f.UpdateRelations(obj, connectionString);

            if (Cache != null)
                Cache.Put(obj);

        }

        public static void DeleteFromDb(object obj, string connectionString)
        {
            Entity entity = GetEntity(obj);
            DbConnection = new NpgsqlConnection(connectionString);
            // DbConnection.ConnectionString = connectionString;

            var command = new NpgsqlCommand();
            command.Connection = DbConnection;
            DbConnection.Open();
            command = DbConnection.CreateCommand();

            command.CommandText = $"DELETE FROM {entity.TableName} WHERE {entity.PKey.ColumnName} = @pKey";
  
            command.Parameters.Add(new NpgsqlParameter("@pKey", entity.PKey.GetValue(obj)));

            command.ExecuteNonQuery();
            command.Parameters.Clear();
            command.Dispose();
            DbConnection.Close();
        }

        internal static object Create(Type type, NpgsqlDataReader reader, ICollection<object> cache, string connectionString)
        {

            Entity entity = GetEntity(type);
            object pKey = entity.PKey.ToFieldType(reader.GetValue(reader.GetOrdinal(entity.PKey.ColumnName)), cache, connectionString);
            object obj = CacheSearch(type, reader, cache);

            if (obj == null)
            {
                if (cache == null)
                    cache = new List<object>();

                cache.Add(obj = Activator.CreateInstance(type));
            }

            foreach (Field f in entity.IntFields)
            {
                object readerVal = reader.GetValue(reader.GetOrdinal(f.ColumnName));
                object fieldVal = f.ToFieldType(readerVal, cache, connectionString);
                f.SetValue(obj, fieldVal);
            }

            foreach (Field f in entity.ExtFields)
            {
                if (typeof(ILazyLoading).IsAssignableFrom(f.Type))
                {
                    object list = Activator.CreateInstance(f.Type, obj, f.Member.Name, connectionString);
                    f.SetValue(obj, list);
                }
                else
                {
                    object list = f.FillList(Activator.CreateInstance(f.Type), obj, cache, connectionString);
                    f.SetValue(obj, list);
                }
            }

            //if (Cache != null)
            //    Cache.Put(obj);

            return obj;
        }

        internal static object Create(Type type, object pKey, ICollection<object> cache, string connectionString)

        {

            object obj = CacheSearch(type, pKey, cache);

            //object obj = null;
            //var count = ((cache != null) ? cache.Count : 0);
            if (obj == null)
            {
                DbConnection = new NpgsqlConnection(connectionString);
                //DbConnection.ConnectionString = connectionString;
                var command = new NpgsqlCommand();
                command.Connection = DbConnection;

                DbConnection.Open();
                Entity entity = GetEntity(type);
                command = DbConnection.CreateCommand();

                command.CommandText = entity.GetSql() + (string.IsNullOrWhiteSpace(entity.SubsetQuery) ? " WHERE " : " AND ") + GetEntity(type).PKey.ColumnName + " = @pKey";
         
                command.Parameters.Add(new NpgsqlParameter("@pKey", pKey));

                using (var dataReader = command.ExecuteReader()) // while using is active, data reader is open. It isn't required to reader.dispose and reader.dispose
                {
                    while (dataReader.Read())
                    {
                        obj = Create(type, dataReader, cache, connectionString);
                    }
                }
                command.Parameters.Clear();
                command.Dispose();
                DbConnection.Close();

            }

            //if (cache != null)
            //    if ((cache != null) && (cache.Count > count))
            //        Cache.Put(obj);

            return obj;

        }

        public static T GetByID<T>(object pKey, string connectionString)
        {
            return (T)Create(typeof(T), pKey, null, connectionString);
        }



        internal static object CacheSearch(Type type, object pKey, ICollection<object> cache)
        {
            if ((Cache != null) && Cache.Contains(type, pKey))
                return Cache.Get(type, pKey);

            if (cache != null)
            {
                foreach (object obj in cache)
                {
                    if (obj.GetType() != type)
                        continue;

                    if (GetEntity(type).PKey.GetValue(obj).Equals(pKey))
                        return obj;
                }
            }

            return null;
        }


        internal static void ListFiller(Type type, object listObj, string sql, IEnumerable<Tuple<string, object>> sqlParameters, string connectionString, ICollection<object> cache = null)
        {
            DbConnection = new NpgsqlConnection(connectionString);
            //DbConnection.ConnectionString = connectionString;

            var command = new NpgsqlCommand();

            command.Connection = DbConnection;
            DbConnection.Open();

            command = DbConnection.CreateCommand();
            command.CommandText = sql;

            foreach (Tuple<string, object> so in sqlParameters) // fkey: string = parameter.name , object = parameter value
            {
                NpgsqlParameter parameter = command.CreateParameter();
                parameter.ParameterName = so.Item1;
                parameter.Value = so.Item2;
                if (parameter.Value == null)
                    parameter.Value = DBNull.Value;
                command.Parameters.Add(parameter);
            }

            using (var dataReader = command.ExecuteReader()) // while using is active, data reader is open. It isn't required to reader.close and reader.dispose
            {
                while (dataReader.Read())
                {
                    listObj.GetType().GetMethod("Add").Invoke(listObj, new object[]
                    {
                        Create(type, dataReader, cache, connectionString)
                    });
                }
            }

            command.Dispose();
            DbConnection.Close();

        }
        internal static Type[] GetTypeOfChild(Type type)
        {
            List<Type> typeList = new List<Type>();
            foreach (Type t in Entities.Keys)
            {
                if (type.IsAssignableFrom(t) && (!t.IsAbstract))
                    typeList.Add(t);

            }
            return typeList.ToArray();
        }
        public static Query<T> GetQuery<T>(string connectionString)
        {
            return new Query<T>(null, connectionString);
        }

        public static List<T> GetFromSql<T>(string sql, string connectionString, IEnumerable<string> keys = null, IEnumerable<object> vals = null)
        {
            List<T> listObj = new List<T>();
            List<Tuple<string, object>> parameters = new List<Tuple<string, object>>();

            if (keys != null)
            {
                List<string> _keys = new List<string>(keys);
                List<object> _vals = new List<object>(vals);

                for (int f = 0; f < _keys.Count; f++)
                    parameters.Add(new Tuple<string, object>(_keys[f], _vals[f]));

            }
            ListFiller(typeof(T), listObj, sql, parameters, connectionString, null);

            return listObj;
        }

        public static void Lock(object obj)
        {
            if (Locking != null)
                Locking.Lock(obj);
        }
        public static void Release(object obj)
        {
            if (Locking != null)
                Locking.Release(obj);
        }

        public static void CreateDbTables(string connectionString)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = "CREATE TABLE Mentors(ID varchar(50) primary key, FirstName varchar(50), LastName varchar(50), BDate timestamptz, Sex int, Salary int, HDate timestamptz)";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Skills (ID varchar(50) primary key, Name varchar(50), KMentor varchar(50), foreign key(KMentor) references Mentors(ID))";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Departments (ID varchar(50) primary key, Name varchar(50), KMentor varchar(50), foreign key(KMentor) references Mentors(ID))";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE JuniorDevelopers (ID varchar(50) primary key, FirstName varchar(50), LastName varchar(50),BDate timestamptz, Sex int, Salary int, HDate timestamptz, KSkill varchar(50), KDepartment varchar(50), foreign key(KSkill) references Skills(ID), foreign key(KDepartment) references Departments(ID))";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE jDevs_skills(KjDev varchar(50), KSkill varchar(50), foreign key(KjDev) references JuniorDevelopers(ID), foreign key(KSkill) references Skills(ID)); ";
            command.ExecuteNonQuery();
            command.Dispose();

            connection.Close();
        }
        public static void DropDbTables(string connectionString)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = "DROP TABLE jDevs_skills";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "DROP TABLE JuniorDevelopers";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "DROP TABLE Departments";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "DROP TABLE Skills";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "DROP TABLE Mentors";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "DROP TABLE Locking";
            command.ExecuteNonQuery();
            command.Dispose();

            connection.Close();
        }
    }
}
