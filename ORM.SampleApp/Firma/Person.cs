using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.SampleApp.Firma
{
    // Class which represents a Person of a Firma model
    public class Person
    {
        // public property which gets/sets the id of the person
        [PrimaryKey]
        public string ID { get; set; }

        // public property which gets/sets the first name of the person
        public string FirstName { get; set; }

        // public property which gets/sets the last name of the person
        public string LastName { get; set; }

        // public property which gets/sets the birthday of the person
        [Field(ColumnName ="BDate")]
        public DateTime BirthDate { get; set; }

        // public property which gets/sets the gender of the person
        public Gender Sex { get; set; }

        // protected member which represents a instance counter
        protected static int InstanceCounter = 1;

        // protected property which gets/sets the number of instance
        [Ignore]
        public int NumberOfInstance { get; protected set; } = InstanceCounter++;
        
        // public enumeration of person gender
        public enum Gender 
        {
            MALE,
            FEMALE
        }
    }
}
