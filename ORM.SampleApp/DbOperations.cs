using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ORM.SampleApp.Firma;
using ORM_FrameWork;

namespace ORM.SampleApp
{
    public static class DbOperations 
    {
   
        public static void InsertObject()
        {

            Console.WriteLine("(1) Insert object");
            Console.WriteLine("-----------------");

            Mentor m = new Mentor();

            m.ID = "1";
            m.FirstName = "Larry";
            m.LastName = "Bird";
            m.Sex = Person.Gender.MALE;
            m.BirthDate = new DateTime(1969, 9, 10);
            m.HireDate = new DateTime(1999, 1, 2);     
            m.Salary = 4500;

            ORMapper.Insert(m);

            Console.WriteLine("\n");


        }

        public static void getMentor()
        {
            Mentor m2 = ORMapper.GetByPkey<Mentor>("1"); // get Mentor with id 1

        }


    }
}
