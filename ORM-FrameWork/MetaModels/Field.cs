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

        public Type Type
        {
            get
            {
                if (Member is PropertyInfo)
                    return ((PropertyInfo)Member).PropertyType;

                throw new NotSupportedException();
            }
        }
        public string ColumnName { get; internal set; }
        public Type ColumnType { get; internal set; }
        public bool IsNullable { get; internal set; } 
        public bool IsPrimaryKey { get; internal set; } 
        public bool IsForeignKey { get; internal set; }


    }
}
