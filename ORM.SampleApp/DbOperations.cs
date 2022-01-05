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
   
        public static void InsertMentor()
        {

            Console.WriteLine("Inserting Mentor into DB.");

            Mentor m1 = new Mentor();

            m1.ID = "m1";
            m1.FirstName = "Larry";
            m1.LastName = "Bird";
            m1.Sex = (int)Person.Gender.MALE;
            m1.BirthDate = new DateTime(1969, 9, 10);
            m1.HireDate = new DateTime(1999, 1, 2);     
            m1.Salary = 4500;

            ORMapper.SaveToDb(m1);


            Console.WriteLine("\n");
        }

        public static void UpdateMentorSalary()
        {
            Console.WriteLine("Getting Mentor from DB.");
            Mentor m2 = ORMapper.GetByID<Mentor>("m1"); // get Mentor with id m1

            Console.WriteLine($"\nSalary for {m2.FirstName} {m2.LastName} is { m2.Salary.ToString()} Dollars.");

            Console.WriteLine("\nMentors salary raises for 250$.");
            m2.Salary += 250;

            Console.WriteLine($"\nSalary for {m2.FirstName} {m2.LastName} is { m2.Salary.ToString()} Dollars.");
            ORMapper.SaveToDb(m2);

        }

        // Foreign Key 1:n 
        public static void GetDepartmentsMentor()
        {
            Console.WriteLine("\nGet Department with his Mentor");

            Mentor m3 = ORMapper.GetByID<Mentor>("m1");

            Department dep = new Department();
            dep.ID = "d1";
            dep.Name = "DevOps";
            dep.Mentor = m3;

            ORMapper.SaveToDb(dep);

            dep = ORMapper.GetByID<Department>("d1");
            Console.WriteLine("\n" + dep.Name + " Mentor: " + dep.Mentor.FirstName + " " + dep.Mentor.LastName);

        }
        // Foreign key n:1
        public static void GetAllMentorsDepartments()
        {
            Console.WriteLine("\nGet Mentor with all departments he mentors.");

            Mentor m4 = ORMapper.GetByID<Mentor>("m1");
            Console.WriteLine($"\nMentor {m4.FirstName} {m4.LastName} is mentoring in following Departments: ");

            foreach (Department dep in m4.Departments)
            {
                Console.WriteLine(dep.Name);
            }

        }


    }
}
