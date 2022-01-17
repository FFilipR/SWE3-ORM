﻿using ORM_FrameWork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.TablePerTypeApp.Firma
{
    [Entity(TableName ="JuniorDevelopers", ChildKey = "KPerson")]
    public class JuniorDeveloper : Person
    {
        public int Salary { get; set; }

        [Field(ColumnName = "HDate")]
        public DateTime HireDate { get; set; }

        [ForeignKey(ColumnName ="KDepartment")]
        public Department Department { get; set; }
    }
}