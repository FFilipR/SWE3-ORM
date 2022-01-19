using ORM.FrameWork.Attributes;
using ORM_FrameWork.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORM_FrameWork.MetaModels
{
    // Class which represents a entity 
    internal class Entity
    {
        //  public property which gets and sets a member of the entity
        public Type Member { get; private set; }

        // public property which gets and sets  the name of the table
        public string TableName { get; set; }

        //public property which gets and sets  the private key of the entity
        public Field PKey { get; set; }

        // private property of local internal fields of the entity
        private Field[] localIntFields = null;

        //public property which gets and sets external fields of the entity
        public Field[] ExtFields { get; private set; }

        //public property which gets and sets internal fields of the entity
        public Field[] IntFields { get; private set; }

        //public property which gets and sets  fields of the entity
        public Field[] Fields { get; private set; }

        //public property which gets and sets whether the entity is material or not
        public bool IsMaterial { get; private set; } = false;

        //public property which gets and sets the subset query of the entity
        public string SubsetQuery { get; private set; }

        //public property which gets and sets the key of the child  of the entity
        public string ChildKey { get; private set; }

        // public property which gets internal fields which are in te local entity table
        public Field[] LocalIntFields
        {
            get
            {
                if (localIntFields == null)
                {
                    Entity baseEntity = ORMapper.GetEntity(Member.BaseType);
                    if (!baseEntity.IsMaterial)
                        return IntFields;

                    List<Field> fieldList = new List<Field>();
                    foreach (Field f in IntFields)
                    {
                        if (baseEntity.IntFields.Where(@if => @if.ColumnName == f.ColumnName).Count() == 0)
                            fieldList.Add(f);
                    }

                    localIntFields = fieldList.ToArray();
                }

                return localIntFields;
            }
        }

        // construcotor which takes a type end creates new instances of the class
        public Entity(Type type)
        {
            this.Member = type;

            EntityAttribute typeAttr = (EntityAttribute)type.GetCustomAttribute(typeof(EntityAttribute));

            if (typeAttr == null)
            {
                MaterialAttribute materialAttr = (MaterialAttribute)type.GetCustomAttribute(typeof(MaterialAttribute));
                if (materialAttr != null)
                {
                    TableName = materialAttr.TableName;
                    SubsetQuery = materialAttr.SubsetQuery;
                    IsMaterial = true;
                }
            }

            else
            {
                TableName = typeAttr.TableName;
                SubsetQuery = typeAttr.SubsetQuery;
                ChildKey = typeAttr.ChildKey;
            }

            if (string.IsNullOrWhiteSpace(TableName))
                TableName = type.Name;

            Fields = getFields(type).ToArray();
            IntFields = Fields.Where(f => (!f.IsExternal)).ToArray();
            ExtFields = Fields.Where(f => f.IsExternal).ToArray();
        }

        // public method which takes a type and gets list of fields of this type
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
                        field.IsMtoN = (!string.IsNullOrWhiteSpace(field.AssigmentTable));
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

        // public method which generates a sql statement of this entity
        public string GetSql()
        {

            Entity baseEntity = ORMapper.GetEntity(Member.BaseType);

            string str = "SELECT ";

            for (int f = 0; f < IntFields.Length; f++)
            {
                if (f > 0)
                    str += ", ";

                str += IntFields[f].ColumnName;
            }

            str += $" FROM {TableName}";


            if (baseEntity.IsMaterial)
                str += $" INNER JOIN {baseEntity.TableName} ON {PKey.ColumnName} = {ChildKey}";

            if (!string.IsNullOrWhiteSpace(SubsetQuery))
                str += $" WHERE ({SubsetQuery})";

            return str;
        }

        // public method which takes a name of the column and gets its field
        public Field GetFieldForColumn(string columnName)
        {
            foreach (Field f in IntFields)
            {
                if (f.ColumnName == columnName)
                    return f;
            }

            return null;
        }

        // public method which takes a name of the field and gets its field
        public Field GetFieldByName(string fieldName)
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
