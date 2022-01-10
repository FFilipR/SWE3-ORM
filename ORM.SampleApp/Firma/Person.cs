using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.SampleApp.Firma
{
    public class Person
    {
        [PrimaryKey]
        public string ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Field(ColumnName ="BDate")]
        public DateTime BirthDate { get; set; }
        public int Sex { get; set; }

        protected static int InstanceCounter = 1;

        [Ignore]
        public int NumberOfInstance { get; protected set; } = InstanceCounter++;
        public enum Gender : int
        {
            MALE = 0,
            FEMALE = 1
        }
    }
}
