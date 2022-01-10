﻿using ORM_FrameWork.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.MetaModels
{
    internal class Entity
    {
        public Type Member { get; private set; }
        public string TableName { get; set; }
        public Field PKey { get;  set; } // what field is primary key

        public Field[] ExtFields { get; private set; }
        public Field[] IntFields { get; private set; }
        public Field[] Fields { get; private set; }

        public string SubsetQuery
        {
            get; private set;
        }

        public Entity(Type type)
        {
            this.Member = type;

            EntityAttribute typeAttr = (EntityAttribute)type.GetCustomAttribute(typeof(EntityAttribute));

            if ((typeAttr == null) || (string.IsNullOrWhiteSpace(typeAttr.TableName)))
                TableName = type.Name.ToUpper();
            
            else
                TableName = typeAttr.TableName;

            Fields = getFields(type).ToArray();
            IntFields = Fields.Where(f => (!f.IsExternal)).ToArray();
            ExtFields = Fields.Where(f => f.IsExternal).ToArray();
        }

        public List<Field> getFields(Type type)
        {
            List<Field> fields = new List<Field>();

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if ((IgnoreAttribute)property.GetCustomAttribute(typeof(IgnoreAttribute)) != null) // getting IgnoreAttribute if it's there
                    continue;

                Field field = new Field(this);

                FieldAttribute fieldAttr = (FieldAttribute)property.GetCustomAttribute(typeof(FieldAttribute));
                if (fieldAttr != null)
                {
                    if (fieldAttr is PrimaryKeyAttribute)
                    {
                        PKey = field;
                        PKey.IsPkey = true;
                    }

                    field.ColumnName = (fieldAttr?.ColumnName ?? property.Name); // if ColumnName doesn't exist, we take Name
                    field.ColumnType = (fieldAttr?.ColumnType ?? property.PropertyType); // if ColumnType doesn't exist, we take the type of Property
                    field.IsNullable = fieldAttr.Nullable;

                    if (field.IsFkey = (fieldAttr is ForeignKeyAttribute))
                    {
                        field.IsExternal = typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
                        field.AssigmentTable = ((ForeignKeyAttribute)fieldAttr).AssigmentTable;
                        field.RemoteColumnName = ((ForeignKeyAttribute)fieldAttr).RemoteColumnName;
                        field.IsMtoM = (!string.IsNullOrWhiteSpace(field.AssigmentTable));
                    }
                                    
                }
                else
                {
                    if ((property.GetGetMethod() == null) || (!property.GetGetMethod().IsPublic))
                        continue;

                    field.ColumnName = property.Name;
                    field.ColumnType = property.PropertyType;
                }

                field.Member = property;

                fields.Add(field);
            }

            return fields;
        }

        public string GetSql(string prefix = null)
        {
            if (prefix == null)
                prefix = string.Empty;

            string str = "SELECT ";

            for(int f = 0; f < IntFields.Length; f++)
			{
                if (f > 0) 
                    str += ", ";

                str += prefix.Trim() + IntFields[f].ColumnName;
            }

            str += $" FROM {TableName}";

            return str;
        }

       public Field GetFieldForColumn(string columnName)
        {
            foreach (Field f in IntFields)
            {
                if (f.ColumnName == columnName)
                    return f;
            }

            return null;
        }

        public Field GetFieldByName (string fieldName)
        {
            foreach (Field f in Fields)
            {
                if (f.Member.Name == fieldName)
                    return f;
            }
            return null;
        }
    }
}
