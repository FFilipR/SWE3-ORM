using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.TablePerTypeApp.Firma
{
    // Class which represents a Junior Developer of a Firma model
    [Entity(TableName ="JuniorDevelopers", ChildKey = "KPerson")]
    public class JuniorDeveloper : Person
    {
        // public property which gets/sets the salary of the junior developer
        public int Salary { get; set; }

        // public property which gets/sets the hire date of the junior developer
        [Field(ColumnName = "HDate")]
        public DateTime HireDate { get; set; }

        // public property which gets/sets the department of the junior developer
        [ForeignKey(ColumnName ="KDepartment")]
        public Department Department { get; set; }
    }
}
