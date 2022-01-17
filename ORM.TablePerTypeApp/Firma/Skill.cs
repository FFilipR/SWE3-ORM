using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.TablePerTypeApp.Firma
{
    [Entity(TableName ="Skills")]
    public class Skill
    {
        [PrimaryKey]
        public string ID { get; set; }
        public string Name { get; set; }

        [ForeignKey(ColumnName = "KMentor")]
        public Mentor Mentor { get; set; }

        [ForeignKey(AssigmentTable = "jDevs_skills", ColumnName ="KSkill", RemoteColumnName = "KjDev")]
        public List<JuniorDeveloper> JDevs { get; set; } = new List<JuniorDeveloper>();
        
    }
}
