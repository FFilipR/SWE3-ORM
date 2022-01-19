using ORM.FrameWork.Loading;
using ORM_FrameWork.Attributes;
using ORM_SampleApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.SampleApp.Firma
{
    // Class which represents a Department of a Firma model
    [Entity(TableName ="Departments")]
    public class Department
    {
        // Public constructor which generates a lazyloading list of junior developers
        public Department()
        {
            JDevs = new LazyLoadingList<JuniorDeveloper>(this, "JDevs", Program.ConnectionString);
        }

        // public property which gets/sets the ID of the department
        [PrimaryKey]
        public string ID { get; set; }

        // public property which gets/sets the Name of the department
        public string Name { get; set; }

        // private property which gets/sets the  lazy loading Mentor of the department
        [ForeignKey(ColumnName = "KMentor")]
        private LazyLoadingObject<Mentor> LazyMentor { get; set; } = new LazyLoadingObject<Mentor>(Program.ConnectionString);

        // public property which gets/sets the  Mentor of the department
        [Ignore]
        public Mentor Mentor { get { return LazyMentor.Value; } set { LazyMentor.Value = value; } }

        // public property which gets/sets lazy loading list of junior developers
        [ForeignKey(ColumnName ="KDepartment")]
        public LazyLoadingList<JuniorDeveloper> JDevs { get; set; }
        

    }
}
