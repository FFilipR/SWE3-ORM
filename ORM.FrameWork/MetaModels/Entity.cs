using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.MetaModels
{
    internal class Entity
    {
        public Entity(Type type)
        {
            this.Member = type;

            EntityAttribute typeAttr = (EntityAttribute)type.GetCustomAttribute(typeof(EntityAttribute));

            if ((typeAttr == null) || (string.IsNullOrWhiteSpace(typeAttr.TableName)))
                TableName = typeAttr.TableName.ToUpper();
            
            else
                TableName = typeAttr.TableName;
            
            this.Fields = getFields(type).ToArray();         
        }
        public Type Member { get; private set; }
        public string TableName { get; set; }
        public Field[] Fields { get; private set; }
        public Field PKey { get; private set; } // what field is primary key

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
                        PKey.IsPrimaryKey = true;
                    }

                    field.ColumnName = (fieldAttr?.ColumnName ?? property.Name); // if ColumnName doesn't exist, we take Name
                    field.ColumnType = (fieldAttr?.ColumnType ?? property.PropertyType); // if ColumnType doesn't exist, we take the type of Property
                    field.IsNullable = fieldAttr.Nullable;
                    field.IsForeignKey = (fieldAttr is ForeignKeyAttribute);
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
    }
}
