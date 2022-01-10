using Npgsql;
using ORM.FrameWork.Loading;
using System;
using System.Collections;
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
        public static NpgsqlConnection DbConnection { get; set; }
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
        public bool IsPkey { get; internal set; } = false;
        public bool IsFkey { get; internal set; } = false;
        public bool IsExternal { get; internal set; } = false;

        public string AssigmentTable { get; internal set; }
        public string RemoteColumnName { get; internal set; }
        public bool IsMtoM { get; internal set; }

        internal string SqlFkey 
        {
            get
            {
                if (IsMtoM)              
                    return $"{ORMapper.GetEntity(Type.GenericTypeArguments[0]).GetSql()} WHERE ID IN (SELECT {RemoteColumnName} FROM {AssigmentTable} WHERE {ColumnName} = @fKey)";
                
                else
                    return $"{ORMapper.GetEntity(Type.GenericTypeArguments[0]).GetSql()} WHERE {ColumnName} = @fKey";
                
            }
        }
        public object GetValue (object obj)
        {
            if (Member is PropertyInfo)
            {
                object val = ((PropertyInfo)Member).GetValue(obj);

                if (val is ILazyLoading)
                {
                    if (!(val is IEnumerable)) 
                    { 
                        return val.GetType().GetProperty("Value").GetValue(val); 
                    }
                }

                return val;
            }

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
            if(IsFkey)
            {
                if (val == null)
                    return null;

                Type type = (typeof(ILazyLoading).IsAssignableFrom(Type) ? Type.GenericTypeArguments[0] : Type);
                object obj = ORMapper.GetEntity(type).PKey.GetValue(val);
                return ORMapper.GetEntity(type).PKey.ToColumnType(obj); // for example type is jDeveloper datatype // Type.GetEntity() == ORMapper.GetEntity(Type)
            }


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

        public object ToFieldType (object val, ICollection<object> cache, string connectionString)
        {
            if (IsFkey)
            {
                if (typeof(ILazyLoading).IsAssignableFrom(Type))   
                    return Activator.CreateInstance(Type, connectionString, val);
                
                return ORMapper.Create(Type, val, cache, connectionString);
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

        public object FillList(object listObj, object obj, ICollection<object> cache, string connectionString)
        {
              
            ORMapper.ListFiller(Type.GenericTypeArguments[0], listObj, SqlFkey, new Tuple<string, object>[] { new Tuple<string, object>("@fKey", Entity.PKey.GetValue(obj)) }, connectionString, cache);
            return listObj;
        }

        public void UpdateRelations(object obj, string connectionString)
        {

            Field.DbConnection = new NpgsqlConnection(connectionString);
            Field.DbConnection.Open();

            if (!IsExternal)
                return;

            Entity innerEntity = ORMapper.GetEntity(Type.GenericTypeArguments[0]); // example: if table skill, innerEntity is the jDev of the skill
            object pKey = Entity.PKey.ToColumnType(Entity.PKey.GetValue(obj));

            if (IsMtoM)
            {
                NpgsqlCommand command = Field.DbConnection.CreateCommand();
                command.CommandText = $"DELETE FROM {AssigmentTable} WHERE {ColumnName} = @pKey";

                NpgsqlParameter parameter = command.CreateParameter();
                parameter.ParameterName = "@pKey";
                parameter.Value = pKey;
                command.Parameters.Add(parameter);

                command.ExecuteNonQuery();
                command.Dispose();

                if(GetValue(obj) != null)
                {
                  
                    foreach (object o in(IEnumerable)GetValue(obj)) // example: jDevs in Skills table
                    {

                        command = Field.DbConnection.CreateCommand();
                        command.CommandText = $"INSERT INTO {AssigmentTable} ({ColumnName}, {RemoteColumnName}) VALUES (@pKey, @fKey)";
                        
                        parameter = command.CreateParameter();
                        parameter.ParameterName = "@pKey";
                        parameter.Value = pKey;
                        command.Parameters.Add(parameter);

                        parameter = command.CreateParameter();
                        parameter.ParameterName = "@fKey";
                        parameter.Value = innerEntity.PKey.ToColumnType(innerEntity.PKey.GetValue(o));
                        command.Parameters.Add(parameter);

                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                }          
            }
            else
            {
                Field field = innerEntity.GetFieldForColumn(ColumnName);

                if (field.IsNullable)
                {                
                        NpgsqlCommand command = Field.DbConnection.CreateCommand();
                        command.CommandText = $"UPDATE {innerEntity.TableName} SET {ColumnName} = NULL WHERE {ColumnName} = @pKey";
                        
                        NpgsqlParameter parameter = command.CreateParameter();
                        parameter.ParameterName = "@pKey";
                        parameter.Value = pKey;
                        command.Parameters.Add(parameter);

                        command.ExecuteNonQuery();
                        command.Dispose();                   
                }
                else if (GetValue(obj) != null)
                {
                    foreach (object o in (IEnumerable)GetValue(obj))
                    {
                        field.SetValue(o, obj);

                        NpgsqlCommand command = Field.DbConnection.CreateCommand();
                        command.CommandText = $"UPDATE {innerEntity.TableName} SET {ColumnName} = @fKey WHERE {innerEntity.PKey.ColumnName} = @pKey";

                        NpgsqlParameter parameter = command.CreateParameter();
                        parameter.ParameterName = "@fKey";
                        parameter.Value = pKey; // fKey is now our pKey
                        command.Parameters.Add(parameter);

                        parameter = command.CreateParameter();
                        parameter.ParameterName = "@fKey";
                        parameter.Value = innerEntity.PKey.ToColumnType(innerEntity.PKey.GetValue(o));
                        command.Parameters.Add(parameter);

                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
                }
            }

            Field.DbConnection.Close();
        }
    } 
}
