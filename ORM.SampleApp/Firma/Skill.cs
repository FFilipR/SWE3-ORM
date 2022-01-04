using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.SampleApp.Firma
{
    [Entity(TableName ="Skills")]
    public class Skill
    {
        [PrimaryKey]
        public string ID { get; set; }
        public string Name { get; set; }

        [ForeignKey(ColumnName = "KMentor")]
        public Mentor Mentor { get; set; }
        
    }
}
