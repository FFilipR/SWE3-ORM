﻿using Npgsql;
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

            for (int f = 0; f < entity.Fields.Length; f++)
            {
                if (f > 0) 
                { 
                    command.CommandText += ", "; 
                    insert += ", "; 
                }
                command.CommandText += entity.Fields[f].ColumnName;             
               
                insert += /*"'"+*/ "@insert" + f.ToString();

                parameter = command.CreateParameter();
                parameter.ParameterName = ("@insert" + f.ToString());
                parameter.Value = entity.Fields[f].ToColumnType(entity.Fields[f].GetValue(obj));
                command.Parameters.Add(parameter);

                if (!entity.Fields[f].IsPrimaryKey)
                {

                    if (fr)
                        fr = false;
                    else
                        conflict += ", ";
  
                    conflict += (entity.Fields[f].ColumnName + " = " + ("@conflict" + f.ToString() /*+ "'"*/));

                    parameter = command.CreateParameter();
                    parameter.ParameterName = ("@conflict" + f.ToString());
                    parameter.Value = entity.Fields[f].ToColumnType(entity.Fields[f].GetValue(obj));
                    command.Parameters.Add(parameter);
                }
            }
            command.CommandText += (") VALUES (" + insert + ") " + conflict);

            command.ExecuteNonQuery();
            command.Dispose();

        }

        internal static object Create(Type type, NpgsqlDataReader reader)
        {
            object obj = Activator.CreateInstance(type);

            foreach(Field f in GetEntity(type).Fields)
            {
                f.SetValue(obj, f.ToFieldType(reader.GetValue(reader.GetOrdinal(f.ColumnName))));
            }

            return obj;
        }

        internal static object Create(Type type, object pKey)
        {
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = DbConnection;

            command.CommandText = GetEntity(type).GetSql() + " WHERE " + GetEntity(type).PKey.ColumnName + "= @pKey";

            NpgsqlParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@pKey";
            parameter.Value = pKey;
            command.Parameters.Add(parameter);

            object obj = null;
            NpgsqlDataReader reader = command.ExecuteReader();

            while(reader.Read())
            {
                obj = Create(type, reader);
            }

            reader.Close();
            reader.Dispose();
            command.Dispose();

            if (obj == null)
                throw new DataException("No data has been found.");

            return obj;
        }

        public static T GetByID<T>(object pKey)
        {
            return (T) Create(typeof(T), pKey);
        }

    }
}
