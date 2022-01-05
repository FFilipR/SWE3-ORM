using Npgsql;
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
        internal static Entity GetEntity(object obj) 
        {
            Type type = ((obj is Type) ? (Type) obj : obj.GetType()); // if obj then GetType , otherwise type

            if (!Entities.ContainsKey(type)) 
                Entities.Add(type, new Entity(type));
              
            return Entities[type];   // return value from dictionary by key
        }


        public static void SaveToDb(object obj)
        {
            Entity entity = GetEntity(obj);
                   
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = DbConnection;

            command.CommandText = "INSERT INTO " + entity.TableName + "(";

            string conflict = "ON CONFLICT (" + entity.PKey.ColumnName + ") DO UPDATE SET ";
            string insert = string.Empty;

            NpgsqlParameter parameter;

            bool fr = true;

            for (int f = 0; f < entity.IntFields.Length; f++)
            {
              
                if (f > 0) 
                { 
                    command.CommandText += ", "; 
                    insert += ", "; 
                }
                command.CommandText += entity.IntFields[f].ColumnName;             
               
                insert += "@insert" + f.ToString();

                parameter = command.CreateParameter();
                parameter.ParameterName = ("@insert" + f.ToString());
                parameter.Value = entity.IntFields[f].ToColumnType(entity.IntFields[f].GetValue(obj));
                command.Parameters.Add(parameter);

                if (!entity.IntFields[f].IsPrimaryKey)
                {

                    if (fr)
                        fr = false;
                    else
                        conflict += ", ";
  
                    conflict += (entity.IntFields[f].ColumnName + " = " + ("@conflict" + f.ToString() ));

                    parameter = command.CreateParameter();
                    parameter.ParameterName = ("@conflict" + f.ToString());
                    parameter.Value = entity.IntFields[f].ToColumnType(entity.IntFields[f].GetValue(obj));
                    command.Parameters.Add(parameter);
                }
            }
            command.CommandText += (") VALUES (" + insert + ") " + conflict);

            command.ExecuteNonQuery();
            command.Dispose();

        }

        internal static object Create(Type type, NpgsqlDataReader reader, ICollection<object> cache)
        {


            Entity entity = GetEntity(type);

            object pKey = entity.PKey.ToFieldType(reader.GetValue(reader.GetOrdinal(entity.PKey.ColumnName)), cache, reader);
            object obj = CacheSearch(type, pKey, cache);

            if(obj == null)
            {
                if (cache == null)
                    cache = new List<object>();

                cache.Add(obj = Activator.CreateInstance(type));
            }

            foreach(Field f in entity.IntFields)
            {
             
                f.SetValue(obj, f.ToFieldType(reader.GetValue(reader.GetOrdinal(f.ColumnName)), cache, reader));
            }

            reader.Close();

            

            foreach (Field f in entity.ExtFields)
            {
                f.SetValue(obj, f.FillList(Activator.CreateInstance(f.Type), obj, cache ));
            }


            return obj;
        }

        internal static object Create(Type type, object pKey, ICollection<object> cache)
        {
            object obj = CacheSearch(type, pKey, cache);

            if(obj == null)
            { 
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = DbConnection;

                command.CommandText = GetEntity(type).GetSql() + " WHERE " + GetEntity(type).PKey.ColumnName + "= @pKey";

                NpgsqlParameter parameter = command.CreateParameter();
                parameter.ParameterName = "@pKey";
                parameter.Value = pKey;
                command.Parameters.Add(parameter);

                NpgsqlDataReader reader = command.ExecuteReader();

                if(reader.Read())
                {

                    //Object[] readerValues = new Object[reader.FieldCount]; // Saving reader values into object[]
                    //int fieldCount = reader.GetValues(readerValues);

                    //for (int i = 0; i < fieldCount; i++)
                    //    Console.WriteLine(readerValues[i]);

                    //reader.Close();

                    obj = Create(type, reader, cache);
                }
                    
                
                reader.Close();
                command.Dispose();
            }

            if (obj.Equals(null))
                throw new DataException("No data has been found.");

            return obj;
        }

        public static T GetByID<T>(object pKey)
        {
            return (T) Create(typeof(T), pKey, null);
        }


        internal static object CacheSearch(Type type, object pKey, ICollection<object> cache)
        {
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
    }
}
