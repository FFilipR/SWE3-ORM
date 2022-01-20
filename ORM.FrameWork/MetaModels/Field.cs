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
    // Class which represents a field  
    public class Field
    {

        // public property which gets and sets the entity who the field belongs
        public Entity Entity { get; set; } 

        // public property which gets and sets new connection to the database 
        public static NpgsqlConnection DbConnection { get; set; } = new NpgsqlConnection();

        // public property which gets and sets a member info of the field
        public MemberInfo Member { get; internal set; }

        // public property which gets and sets a type of the field
        public Type Type // in Object
        {
            get
            {
                if (Member is PropertyInfo)
                    return ((PropertyInfo)Member).PropertyType;

                throw new NotSupportedException();
            }
        }

        // public property which gets and sets the name of the column
        public string ColumnName { get; internal set; }


        // public property which gets and sets a db type of the column
        public Type ColumnType { get; internal set; } // on DB level

        // public property which gets and sets whether the field is null or not
        public bool IsNullable { get; internal set; } = false;

        // public property which gets and sets whether the field is primary key
        public bool IsPkey { get; internal set; } = false;

        // public property which gets and sets whether the field is foreign key 
        public bool IsFkey { get; internal set; } = false;

        // public property which gets and sets whether the field is external 
        public bool IsExternal { get; internal set; } = false;

        // public property which represent that the field is a assigment table
        public string AssigmentTable { get; internal set; }

        // public property which gets and sets a column name of the remote table
        public string RemoteColumnName { get; internal set; }

        // public property which gets and sets whether the field is in M to N relation
        public bool IsMtoN { get; internal set; }

        // internal property which gets sql foreign key
        internal string SqlFkey
        {
            get
            {
                if (IsMtoN)
                    return $"{ORMapper.GetEntity(Type.GenericTypeArguments[0]).GetSql()} WHERE ID IN (SELECT {RemoteColumnName} FROM {AssigmentTable} WHERE {ColumnName} = @fKey)";

                else
                    return $"{ORMapper.GetEntity(Type.GenericTypeArguments[0]).GetSql()} WHERE {ColumnName} = @fKey";

            }
        }
        
        // constructor which takes a entity and creates a new instance of the class
        public Field(Entity entity)
        {
            this.Entity = entity;
        }

        // public method which takes an object and gets the field value of it
        public object GetValue(object obj)
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

        // public method which takes an object and its value and sets it

        public void SetValue(object obj, object val)
        {
            if (Member is PropertyInfo)
            {
                ((PropertyInfo)Member).SetValue(obj, val);
                return;
            }

            throw new NotSupportedException("Type of the member is not supported.");
        }

        // public method which takes an object and retuns his column type of the database level
        public object ToColumnType(object val) // taking an object type and converting it in the corresponding type in DB
        {
            if (IsFkey)
            {
                if (val == null)
                    return null;

                Type type = (typeof(ILazyLoading).IsAssignableFrom(Type) ? Type.GenericTypeArguments[0] : Type);
                object obj = ORMapper.GetEntity(type).PKey.GetValue(val);
                return ORMapper.GetEntity(type).PKey.ToColumnType(obj); // for example type is jDeveloper datatype 
            }

            if (val is bool)
            {
                if (ColumnType == typeof(int))
                    return ((bool)val) ? true : false;
                if (ColumnType == typeof(short))
                    return ((bool)val) ? true : false;
                if (ColumnType == typeof(long))
                    return ((bool)val) ? true : false;
            }

            return val;
        }

        // public method which takes an object, cache and conneciton string then retuns his field type 
        public object ToFieldType(object val, ICollection<object> cache, string connectionString)
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
                    return ((int)val) != 0;
                if (val is long)
                    return ((int)val) != 0;
                if (val is short)
                    return ((int)val) != 0;
            }

            if (Type == typeof(short))
                return Convert.ToInt16(val);
            if (Type == typeof(int))
                return Convert.ToInt32(val);
            if (Type == typeof(long))
                return Convert.ToInt64(val);

            if (Type.IsEnum)
            {
                int nVal = 0;

                if (val == "male")
                    nVal = 0;
                if (val == "female")
                    nVal = 1;

                return Enum.ToObject(Type, nVal);
            }

            return val;
        }

        // public method which takes a list , object, cache and conneciton string and fills the list for the foreign key
        public object FillList(object listObj, object obj, ICollection<object> cache, string connectionString)
        {
            ORMapper.ListFiller(Type.GenericTypeArguments[0], listObj, SqlFkey, new Tuple<string, object>[] { new Tuple<string, object>("@fKey", Entity.PKey.GetValue(obj)) }, connectionString, cache);
            return listObj;
        }

        // public methodh which takes an object and connection string then updates the relations
        public void UpdateRelations(object obj, string connectionString)
        {


            var connection = new NpgsqlConnection(connectionString);
            var command = new NpgsqlCommand();

            command.Connection = connection;
            connection.Open();

            if (!IsExternal)
                return;

            Entity innerEntity = ORMapper.GetEntity(Type.GenericTypeArguments[0]); // example: if table skill, innerEntity is the jDev of the skill
            object pKey = Entity.PKey.ToColumnType(Entity.PKey.GetValue(obj));

            if (IsMtoN)
            {
                command = connection.CreateCommand();
                command.CommandText = $"DELETE FROM {AssigmentTable} WHERE {ColumnName} = @pKey";

                command.Parameters.Add(new NpgsqlParameter("@pKey", pKey));

                command.ExecuteNonQuery();
                command.Parameters.Clear();
                command.Dispose();

                if (GetValue(obj) != null)
                {

                    foreach (object o in (IEnumerable)GetValue(obj)) // example: jDevs in Skills table
                    {

                        command = connection.CreateCommand();
                        command.CommandText = $"INSERT INTO {AssigmentTable} ({ColumnName}, {RemoteColumnName}) VALUES (@pKey, @fKey)";

                        command.Parameters.Add(new NpgsqlParameter("@pKey", pKey));
                        command.Parameters.Add(new NpgsqlParameter("@fKey", innerEntity.PKey.ToColumnType(innerEntity.PKey.GetValue(o))));

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.Dispose();
                    }
                }
            }
            else
            {
                Field field = innerEntity.GetFieldForColumn(ColumnName);

                if (field.IsNullable)
                {
                    command = connection.CreateCommand();

                    command.CommandText = $"UPDATE {innerEntity.TableName} SET {ColumnName} = NULL WHERE {ColumnName} = @pKey";

                    command.Parameters.Add(new NpgsqlParameter("@pKey", pKey));

                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    command.Dispose();
                }
                else if (GetValue(obj) != null)
                {
                    foreach (object o in (IEnumerable)GetValue(obj))
                    {
                        field.SetValue(o, obj);

                        command = connection.CreateCommand();

                        command.CommandText = $"UPDATE {innerEntity.TableName} SET {ColumnName} = @fKey WHERE {innerEntity.PKey.ColumnName} = @pKey";

                        command.Parameters.Add(new NpgsqlParameter("@fKey", pKey)); // fKey is now our pKey
                        command.Parameters.Add(new NpgsqlParameter("@pKey", innerEntity.PKey.ToColumnType(innerEntity.PKey.GetValue(o))));

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.Dispose();
                    }
                }


                connection.Close();

            }
        }
    }
}
