using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.SampleApp.Firma
{
    [Entity(TableName = "Mentors")]
    public class Mentor : Person
    {
        public int Salary { get; set; }

        [Field(ColumnName ="HDate")]
        public DateTime HireDate { get; set; }

        [ForeignKey(ColumnName = "KMentor")]
        public List<Department> Departments { get; private set; }

        [ForeignKey(ColumnName = "KMentor")]
        public List<Skill> skills { get; private set; } = new List<Skill>();

    }
}
