using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.TablePerTypeApp.Firma
{
    // Class which represents a Mentor of a Firma model
    [Entity(TableName = "Mentors", ChildKey = "KPerson")]
    public class Mentor : Person
    {
        // public property which gets/sets the salary of the mentor
        public int Salary { get; set; }


        // public property which gets/sets the hire date of the mentor
        [Field(ColumnName ="HDate")]
        public DateTime HireDate { get; set; }

        // public property which gets/sets the list of departments where the mentor is mentoring
        [ForeignKey(ColumnName = "KMentor")]
        public List<Department> Departments { get; private set; }

        // public property which gets/sets the list of skills which the mentor is mentoring
        [ForeignKey(ColumnName = "KMentor")]
        public List<Skill> skills { get; private set; } = new List<Skill>();
        


    }
}
