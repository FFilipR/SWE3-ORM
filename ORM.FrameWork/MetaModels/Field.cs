using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.MetaModels
{
    internal class Field
    {
        public Field(Entity entity)
        {
            this.Entity = entity;
        }
        public Entity Entity { get; internal set; } // entity who it belongs 
        public MemberInfo Member { get; internal set; }

        public Type Type // in Object
        {
            get
            {
                if (Member is PropertyInfo)
                    return ((PropertyInfo)Member).PropertyType;

                throw new NotSupportedException();
            }
        }
        public string ColumnName { get; internal set; }
        public Type ColumnType { get; internal set; } // on DB level
        public bool IsNullable { get; internal set; } = false;
        public bool IsPrimaryKey { get; internal set; } = false;
        public bool IsForeignKey { get; internal set; } = false;
        public bool IsExternal { get; internal set; } = false;


        public object GetValue (object obj)
        {
            if (Member is PropertyInfo)
                return ((PropertyInfo)Member).GetValue(obj);

            throw new NotSupportedException("Type of the member is not supported.");
        }
        public void SetValue(object obj, object val)
        {
            if (Member is PropertyInfo)
            {
                ((PropertyInfo)Member).SetValue(obj, val);
                return;
            }

            throw new NotSupportedException("Type of the member is not supported.");
        }

        public object ToColumnType(object val) // taking an object type and converting it in the corresponding type in DB
        {
            if(IsForeignKey)
            {
                object obj = ORMapper.GetEntity(Type).PKey.GetValue(val);
                return ORMapper.GetEntity(Type).PKey.ToColumnType(obj); // for example type is jDeveloper datatype // Type.GetEntity() == ORMapper.GetEntity(Type)
            }

            //if (Type == ColumnType)
            //    return val;

            if (val is bool)
            {
                if (ColumnType == typeof(int))
                    return ((bool) val) ? 1 : 0; // if cast true ist, return 1 else 0
                if (ColumnType == typeof(short))
                    return ((bool) val) ? 1 : 0;
                if (ColumnType == typeof(long))
                    return ((bool) val) ? 1 : 0;
            }

            return val;
        }

        public object ToFieldType (object val, ICollection<object> cache, NpgsqlDataReader reader)
        {
            if (IsForeignKey)
            {
                reader.Close();
                return ORMapper.Create(Type, val, cache);
            }

            if (Type == typeof(bool))
            {
                if (val is int)
                    return ((int) val) != 0;
                if (val is long)
                    return ((int) val) != 0;
                if (val is short)
                    return ((int) val) != 0;
            }

            if (Type == typeof(short))
                return Convert.ToInt16(val);
            if (Type == typeof(int))
                return Convert.ToInt32(val);
            if (Type == typeof(long))
                return Convert.ToInt64(val);

            if (Type.IsEnum)
                return Enum.ToObject(Type, val);
       
            return val;
        }

        public object FillList(object listObj, object obj, ICollection<object> cache)
        {

            NpgsqlCommand command = ORMapper.DbConnection.CreateCommand();
            command.CommandText = ORMapper.GetEntity(Type.GenericTypeArguments[0]).GetSql() + " WHERE " + ColumnName + " = @fKey";

            NpgsqlParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@fKey";
            parameter.Value = Entity.PKey.GetValue(obj);
            command.Parameters.Add(parameter);

            NpgsqlDataReader reader = command.ExecuteReader();
            //Object[] readerValues = new Object[reader.FieldCount];

            if (reader.Read()) // or while ?
            {
                listObj.GetType().GetMethod("Add").Invoke(listObj, new object[] 
                { 
                    ORMapper.Create(Type.GenericTypeArguments[0], reader, cache)   // creating object with reader method and type and return it in the listObj
                });
            }
            reader.Close();
            reader.Dispose();
            command.Dispose();

            return listObj;
        }
    } 
}
