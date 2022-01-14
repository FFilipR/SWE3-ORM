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
    [Entity(TableName ="Departments")]
    public class Department
    {
        public Department()
        {
            JDevs = new LazyLoadingList<JuniorDeveloper>(this, "JDevs", Program.ConnectionString);
        }
        [PrimaryKey]
        public string ID { get; set; }
        public string Name { get; set; }
   
        [ForeignKey(ColumnName = "KMentor")]
        private LazyLoadingObject<Mentor> LazyMentor { get; set; } = new LazyLoadingObject<Mentor>(Program.ConnectionString);

        [Ignore]
        public Mentor Mentor { get { return LazyMentor.Value; } set { LazyMentor.Value = value; } }

        [ForeignKey(ColumnName ="KDepartment")]
        public LazyLoadingList<JuniorDeveloper> JDevs { get; set; }
        

    }
}
