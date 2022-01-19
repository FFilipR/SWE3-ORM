using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.TablePerTypeApp.Firma
{
    // Class which represents a Skill of a Firma model
    [Entity(TableName ="Skills")]
    public class Skill
    {
        [PrimaryKey]
        // public property which gets/sets the id of the skill
        public string ID { get; set; }

        // public property which gets/sets the name of the skill
        public string Name { get; set; }

        // public property which gets/sets the mentor of the skill
        [ForeignKey(ColumnName = "KMentor")]
        public Mentor Mentor { get; set; }

        // public property which gets/sets the  list of junior developers with the skill
        [ForeignKey(AssigmentTable = "jDevs_skills", ColumnName ="KSkill", RemoteColumnName = "KjDev")]
        public List<JuniorDeveloper> JDevs { get; set; } = new List<JuniorDeveloper>();
        
    }
}
