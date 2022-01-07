using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ORM.SampleApp.Firma;
using ORM_FrameWork;
using ORM_SampleApp;

namespace ORM.SampleApp
{
    public static class DbOperations 
    {
   
        public static void InsertMentor()
        {

            Console.WriteLine("->Inserting Mentor into DB.");

            Mentor m1 = new Mentor();

            m1.ID = "m1";
            m1.FirstName = "Larry";
            m1.LastName = "Bird";
            m1.Sex = (int)Person.Gender.MALE;
            m1.BirthDate = new DateTime(1969, 9, 10);
            m1.HireDate = new DateTime(1999, 1, 2);     
            m1.Salary = 4500;

            ORMapper.SaveToDb(m1);
            Console.WriteLine("_______________________________________________________________________");

        }

        public static void UpdateMentorSalary()
        {
            Console.WriteLine("\n->Getting Mentor from DB.");

            Mentor m2 = ORMapper.GetByID<Mentor>("m1", Program.connectionString); // get Mentor with id m1

            Console.WriteLine($"Salary for {m2.FirstName} {m2.LastName} is { m2.Salary} Dollars.");

            Console.WriteLine("Mentors salary raises for 250$.");
            m2.Salary += 250;

            Console.WriteLine($"Salary for {m2.FirstName} {m2.LastName} is { m2.Salary} Dollars.");
            ORMapper.SaveToDb(m2);
            Console.WriteLine("_______________________________________________________________________");


        }

        // Foreign Key 1:n 
        public static void GetDepartmentsMentor()
        {
            Console.WriteLine("\n->Getting Department with his Mentor.");


            Mentor m3 = ORMapper.GetByID<Mentor>("m1", Program.connectionString);

            Department dep = new Department();
            dep.ID = "d1";
            dep.Name = "DevOps";
            dep.Mentor = m3;

            ORMapper.SaveToDb(dep);

            Department dep2 = new Department();
            dep2.ID = "d2";
            dep2.Name = "C#";
            dep2.Mentor = m3;
            ORMapper.SaveToDb(dep2);


            Department dep3 = new Department();
            dep3.ID = "d3";
            dep3.Name = "Java";
            dep3.Mentor = m3;
            ORMapper.SaveToDb(dep3);

            dep = ORMapper.GetByID<Department>("d1", Program.connectionString);
            Console.WriteLine($"{dep.Name} Mentor: {dep.Mentor.FirstName} {dep.Mentor.LastName}");
            Console.WriteLine("_______________________________________________________________________");


        }
        // Foreign key n:1
        public static void GetAllMentorsDepartments()
        {
            Console.WriteLine("\n->Get Mentor with all departments he mentors.");
            string departments = string.Empty;

            Mentor m4 = ORMapper.GetByID<Mentor>("m1", Program.connectionString);

            foreach (Department dep in m4.Departments)
            {
                departments += $"{dep.Name} ";
            }

            Console.WriteLine($"Mentor {m4.FirstName} {m4.LastName} is mentoring in following Departments: {departments}");
            Console.WriteLine("_______________________________________________________________________");

        }

    }
}
