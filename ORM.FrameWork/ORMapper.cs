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
    // Class which implements the functionalities of the Object Relational Mapping
    public class ORMapper
    {
        // public property which gets/sets a pl sql connection to the database
        public static NpgsqlConnection DbConnection { get; set; }

        // private member dictionary which consists type and entities
        private static Dictionary<Type, Entity> Entities = new Dictionary<Type, Entity>();

        // public property which gets/sets the db locking 
        public static ILocking Locking { get; set; }

        // public property which gets/sets the cache of the framework
        public static ICache Cache { get; set; }

        // public method which takes a object as a parameter and gets a entity for this object
        public static Entity GetEntity(object obj)
        {
            Type type = ((obj is Type) ? (Type)obj : obj.GetType()); // if obj then GetType , otherwise type

            if (!Entities.ContainsKey(type))
                Entities.Add(type, new Entity(type));

            return Entities[type];   // returns value from dictionary by key
        }

        // public method which  takes an object and conneciton string and saves it 
        public static void SaveToDb(object obj, string connectionString)
        {
       
            if (GetEntity(obj.GetType().BaseType).IsMaterial)  
                Save(obj, GetEntity(obj.GetType().BaseType), false, true, connectionString); 

            Save(obj, GetEntity(obj), GetEntity(obj.GetType().BaseType).IsMaterial, false, connectionString);
        }

        // private method which  takes an object, entity, baseMaterial boolean, base boolean and conneciton string and saves the object in the database
        private static void Save(object obj, Entity entity, bool isBaseMaterial, bool isBase, string connectionString)
        {
            string insert = string.Empty;
            string conflict = string.Empty;
            bool fr = true;

            //if (Cache != null)
            //{
            //    if (!Cache.HasChanged(obj))
            //        return;
            //}

            DbConnection = new NpgsqlConnection(connectionString);
            var command = new NpgsqlCommand();
            command.Connection = DbConnection;
            DbConnection.Open();
            command = DbConnection.CreateCommand();

            command.CommandText = $"INSERT INTO {entity.TableName} (";

            if (isBaseMaterial)
            {
                command.CommandText += $"{entity.ChildKey}, ";
                conflict = $"ON CONFLICT ({entity.ChildKey}) DO UPDATE SET ";
                insert = "@cKey, ";
                command.Parameters.Add(new NpgsqlParameter("@cKey", entity.PKey.GetValue(obj)));
            }
            else
                 conflict = $"ON CONFLICT ({entity.PKey.ColumnName}) DO UPDATE SET ";

            for (int f = 0; f < entity.LocalIntFields.Length; f++)
            {
                if (f > 0)
                {
                    command.CommandText += ", ";
                    insert += ", ";
                }

                command.CommandText += entity.LocalIntFields[f].ColumnName;

                insert += $"@insert{f}";

                object field = entity.LocalIntFields[f].ToColumnType(entity.LocalIntFields[f].GetValue(obj));

                if (field == null)
                    field = DBNull.Value;

                command.Parameters.Add(new NpgsqlParameter($"@insert{ f }", field));

                if (!entity.LocalIntFields[f].IsPkey)
                {

                    if (fr)
                        fr = false;
                    else
                        conflict += ", ";

                    conflict += $"{entity.LocalIntFields[f].ColumnName} = @conflict{f}";

                    field = entity.LocalIntFields[f].ToColumnType(entity.LocalIntFields[f].GetValue(obj));

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

            if (!isBase)
                foreach (Field f in entity.ExtFields)
                    f.UpdateRelations(obj, connectionString);

            if (Cache != null)
                Cache.Put(obj);

        }

        // public method which takes a object and connection string, and deletes the object 
        public static void DeleteFromDb(object obj, string connectionString)
        {        
            if (GetEntity(obj.GetType().BaseType).IsMaterial)
            {
                DeleteFromDb(obj, false, GetEntity(obj), connectionString );
                DeleteFromDb(obj, true, GetEntity(obj.GetType().BaseType), connectionString);
            }
            else
                DeleteFromDb(obj, true, GetEntity(obj), connectionString); 
        }


        // priavate method which takes a object, base boolean, entity and  connection string, and deletes the object from database
        private static void DeleteFromDb(object obj, bool isBase, Entity entity, string connectionString)
        {
            DbConnection = new NpgsqlConnection(connectionString);
            var command = new NpgsqlCommand();
            command.Connection = DbConnection;

            DbConnection.Open();
            command = DbConnection.CreateCommand();

            command.CommandText = $"DELETE FROM {entity.TableName} WHERE {(isBase ? entity.PKey.ColumnName : entity.ChildKey)} = @pKey";
            command.Parameters.Add(new NpgsqlParameter("@pKey", entity.PKey.GetValue(obj)));

            command.ExecuteNonQuery();
            command.Parameters.Clear();
            command.Dispose();
            DbConnection.Close();
        }

        // internal method which takes a type, dataReader, cache and conneciton string and then creates a object
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

        // public method which takes a type, primary key, cache and conneciton string and then creates a object for the database
        public static object Create(Type type, object pKey, ICollection<object> cache, string connectionString)

        {

            object obj = CacheSearch(type, pKey, cache);

            //object obj = null;
            //var count = ((cache != null) ? cache.Count : 0);
            if (obj == null)
            {
                DbConnection = new NpgsqlConnection(connectionString);
                var command = new NpgsqlCommand();
                command.Connection = DbConnection;

                DbConnection.Open();
                Entity entity = GetEntity(type);
                command = DbConnection.CreateCommand();

                command.CommandText = entity.GetSql() + (string.IsNullOrWhiteSpace(entity.SubsetQuery) ? " WHERE " : " AND ") + entity.PKey.ColumnName + " = @pKey";
         
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
        /// pulbic method which  takes a primary key and connection string and then gets [T] from the database for the provided [primaryKey]
        public static T GetByID<T>(object pKey, string connectionString)
        {
            return (T)Create(typeof(T), pKey, null, connectionString);
        }

        // internal method which takes a type, primary key and cache and then gets the object for which is searched for
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

        // internal method which takes type, object lsit, sql , tuple of sql parameters, conenciton string and cache, then it fills the list
        internal static void ListFiller(Type type, object listObj, string sql, IEnumerable<Tuple<string, object>> sqlParameters, string connectionString, ICollection<object> cache = null)
        {
            DbConnection = new NpgsqlConnection(connectionString);

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

        // internal method which takes a type and gets the type of this child
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

        // public method which takes a conneciton string and gets ghe query for a class
        public static Query<T> GetQuery<T>(string connectionString)
        {
            return new Query<T>(null, connectionString);
        }

        // public method which takes a sql, conneciton string, enumeration of keys and enumeration of values and the returns a list of objects for the given sql query
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

        // public method which takes a object and locks it
        public static void Lock(object obj)
        {
            if (Locking != null)
                Locking.Lock(obj);
        }

        // public method which takes a object and releases the lock
        public static void Release(object obj)
        {
            if (Locking != null)
                Locking.Release(obj);
        }

        // public method which takes the connection string to SampleApp and creates all tables in the database
        public static void CreateDbTables1(string connectionString)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = "CREATE TYPE Gender AS ENUM ('male', 'female')";
            command.ExecuteNonQuery();
            command.Dispose();


            command = connection.CreateCommand();
            command.CommandText =   "CREATE TABLE Mentors" +
                                    "(" +
                                        "ID varchar(50) primary key, " +
                                        "FirstName varchar(50), " +
                                        "LastName varchar(50), " +
                                        "BDate timestamptz, " +
                                        "Sex Gender, " +
                                        "Salary int, " +
                                        "HDate timestamptz" +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText =   "CREATE TABLE Skills " +
                                    "(" +
                                        "ID varchar(50) primary key, " +
                                        "Name varchar(50), " +
                                        "KMentor varchar(50), " +
                                        "foreign key(KMentor) references Mentors(ID)" +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText =   "CREATE TABLE Departments " +
                                    "(" +
                                        "ID varchar(50) primary key, " +
                                        "Name varchar(50), " +
                                        "KMentor varchar(50), " +
                                        "foreign key(KMentor) references Mentors(ID)" +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText =   "CREATE TABLE JuniorDevelopers " +
                                    "(" +
                                        "ID varchar(50) primary key, " +
                                        "FirstName varchar(50), " +
                                        "LastName varchar(50), " +
                                        "BDate timestamptz, " +
                                        "Sex Gender, " +
                                        "Salary int, " +
                                        "HDate timestamptz, " +
                                        "KDepartment varchar(50), " +
                                        "foreign key(KDepartment) references Departments(ID)" +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText =   "CREATE TABLE jDevs_skills" +
                                    "(" +
                                        "KjDev varchar(50), " +
                                        "KSkill varchar(50), " +
                                        "foreign key(KjDev) references JuniorDevelopers(ID), " +
                                        "foreign key(KSkill) references Skills(ID)" +
                                    "); ";

            command.ExecuteNonQuery();
            command.Dispose();

            connection.Close();
        }

        // public method which takes the connection string to SampleApp and drops all tables from the database
        public static void DropDbTables1(string connectionString)
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
            command.CommandText = "DROP TYPE Gender";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "DROP TABLE Locking";
            command.ExecuteNonQuery();
            command.Dispose();

            connection.Close();
        }

        // public method which takes the connection string from TablePerTypeApp and creates all tables in the database
        public static void CreateDbTables2(string connectionString)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = "CREATE TYPE Gender AS ENUM ('male', 'female')";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Persons" +
                                    "(" +
                                        "ID varchar(50) primary key, " +
                                        "FirstName varchar(50), " +
                                        "LastName varchar(50), " +
                                        "Sex Gender, " +
                                        "BDate timestamptz " +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();
   
            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Mentors " +
                                    "(" +
                                        "KPerson varchar(50) references Persons(ID) primary key, " +
                                        "HDate timestamptz, " +
                                        "Salary int " +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Skills " +
                                    "(" +
                                        "ID varchar(50) primary key, " +
                                        "Name varchar(50), " +
                                        "KMentor varchar(50), " +
                                        "foreign key(KMentor) references Mentors(KPerson)" +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Departments " +
                                    "(" +
                                        "ID varchar(50) primary key, " +
                                        "Name varchar(50), " +
                                        "KMentor varchar(50), " +
                                        "foreign key(KMentor) references Mentors(KPerson)" +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE JuniorDevelopers " +
                                    "(" +
                                        "KPerson varchar(50) references Persons(ID) primary key, " +
                                        "KDepartment varchar(50), " +
                                        "HDate timestamptz, " +
                                        "Salary int, " +
                                        "foreign key(KDepartment) references Departments(ID)" +
                                    ")";

            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE jDevs_skills" +
                                    "(" +
                                        "KjDev varchar(50), " +
                                        "KSkill varchar(50), " +
                                        "foreign key(KjDev) references JuniorDevelopers(KPerson), " +
                                        "foreign key(KSkill) references Skills(ID)" +
                                    "); ";

            command.ExecuteNonQuery();
            command.Dispose();

            connection.Close();
        }

        // public method which takes the connection string to TablePerTypeApp and drops all tables from the database
        public static void DropDbTables2(string connectionString)
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
            command.CommandText = "DROP TABLE Persons";
            command.ExecuteNonQuery();
            command.Dispose();

            command = connection.CreateCommand();
            command.CommandText = "DROP TYPE Gender";
            command.ExecuteNonQuery();
            command.Dispose();

            connection.Close();
        }
    }
}
