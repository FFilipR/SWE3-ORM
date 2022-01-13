using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ORM.FrameWork.Cache;
using ORM.SampleApp.Firma;
using ORM_FrameWork;
using ORM_SampleApp;

namespace ORM.SampleApp
{
    public static class Operations 
    {
   
        public static void InsertMentor()
        {

            Console.WriteLine("->Inserting Mentor into DB.");

            Mentor m1 = new Mentor();

            m1.ID = "m1";
            m1.FirstName = "Larry";
            m1.LastName = "Bird";
            m1.BirthDate = new DateTime(1969, 9, 10);
            m1.HireDate = new DateTime(1999, 1, 2);     
            m1.Sex = (int)Person.Gender.MALE;
            m1.Salary = 4500;

            ORMapper.SaveToDb(m1, Program.connectionString);
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
            ORMapper.SaveToDb(m2, Program.connectionString);
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


        

            ORMapper.SaveToDb(dep, Program.connectionString);

            Department dep2 = new Department();
            dep2.ID = "d2";
            dep2.Name = "WebDev";
            dep2.Mentor = m3;
            ORMapper.SaveToDb(dep2, Program.connectionString);


            Department dep3 = new Department();
            dep3.ID = "d3";
            dep3.Name = "SWE";
            dep3.Mentor = m3;
            ORMapper.SaveToDb(dep3, Program.connectionString);


            dep = ORMapper.GetByID<Department>("d1", Program.connectionString);

            Console.WriteLine($"{dep.Name} Mentor: {dep.Mentor.FirstName} {dep.Mentor.LastName}");
            Console.WriteLine("_______________________________________________________________________");


        }
        // Foreign key n:1
        public static void GetAllMentorsDepartments()
        {
            Console.WriteLine("\n->Get Mentor with all departments he mentors.");

            Mentor m4 = ORMapper.GetByID<Mentor>("m1", Program.connectionString);

            string departments = string.Empty;

            if (m4.Departments != null)
            {
                foreach (Department dep in m4.Departments)
                {
                    departments += $"{dep.Name}; ";
                }

            }

            Console.WriteLine($"Mentor {m4.FirstName} {m4.LastName} is mentoring in following Departments: {departments}");
            Console.WriteLine("_______________________________________________________________________");

        }

        // m:n
        public static void MtoNRelation()
        {
            Console.WriteLine("\n-> M to N Relation");

            Skill skill = new Skill();
            skill.ID = "s1";
            skill.Name = "C#";
            skill.Mentor = ORMapper.GetByID<Mentor>("m1", Program.connectionString);

            JuniorDeveloper jDev1 = new JuniorDeveloper();
            jDev1.ID = "jd1";
            jDev1.FirstName = "Eva";
            jDev1.LastName = "Atkinson";
            jDev1.BirthDate = new DateTime(1998, 3, 12);
            jDev1.HireDate = new DateTime(2021, 2, 25);
            jDev1.Sex = (int)Person.Gender.FEMALE;
            jDev1.Salary = 2100;   
            ORMapper.SaveToDb(jDev1, Program.connectionString);

            JuniorDeveloper jDev2 = new JuniorDeveloper();
            jDev2.ID = "jd2";
            jDev2.FirstName = "Joe";
            jDev2.LastName = "Rhodes";
            jDev2.BirthDate = new DateTime(1999, 6, 12);
            jDev2.HireDate = new DateTime(2021, 2, 25);
            jDev2.Sex = (int)Person.Gender.MALE;
            jDev2.Salary = 2500;
            ORMapper.SaveToDb(jDev2, Program.connectionString);

            skill.JDevs.Add(jDev1);
            skill.JDevs.Add(jDev2);
            ORMapper.SaveToDb(skill, Program.connectionString);

            skill = ORMapper.GetByID<Skill>("s1", Program.connectionString);


            string devs = string.Empty;
            foreach (JuniorDeveloper jd in skill.JDevs)
            {
                devs += $"{jd.FirstName} {jd.LastName}; ";
            }

            Console.WriteLine($"Junior Developers that are attending  {skill.Name} are: {devs}");
            Console.WriteLine("_______________________________________________________________________");

        }

        public static void LazyList()
        {
            Console.WriteLine("\n->Lazy Loading for Junior Developer list.");

            Department dep = ORMapper.GetByID<Department>("d1", Program.connectionString);
            dep.JDevs.Add(ORMapper.GetByID<JuniorDeveloper>("jd1", Program.connectionString));
            dep.JDevs.Add(ORMapper.GetByID<JuniorDeveloper>("jd2", Program.connectionString));

            ORMapper.SaveToDb(dep, Program.connectionString);

            dep = ORMapper.GetByID<Department>("d1", Program.connectionString);

            string jdevs = string.Empty;
            foreach (JuniorDeveloper jd in dep.JDevs)
            {
                jdevs += $"{jd.FirstName} {jd.LastName}; ";
            }

            Console.WriteLine($"Junior developers in department {jdevs}");
            Console.WriteLine("_______________________________________________________________________");

        }

        public static void CacheDemo()
        {
            Console.WriteLine("\n->Demonstaration of Cache.");

            Console.WriteLine("\nNo Cache:");
            for (int f=0; f<5; f++)
            {
                Mentor m1 = ORMapper.GetByID<Mentor>("m1", Program.connectionString);
                Console.WriteLine($"Mentor Object with ID: ({m1.ID}) -> instance number: {m1.NumberOfInstance}");
            }

            Console.WriteLine("\nWith Cache:");
            ORMapper.Cache = new Cache();
            for (int f = 0; f < 5; f++)
            {
                Mentor m1 = ORMapper.GetByID<Mentor>("m1", Program.connectionString);
                Console.WriteLine($"Mentor Object with ID: ({m1.ID}) -> instance number: {m1.NumberOfInstance}");
            }

            Console.WriteLine("_______________________________________________________________________");

        }

        public static void QueryDemo()
        {
            Console.WriteLine("\n->Demonstaration of Query.");

            string jDevs = string.Empty;
            foreach (JuniorDeveloper jDev in ORMapper.From<JuniorDeveloper>(Program.connectionString).Greater("Salary",2200))
            {
               jDevs += $"{jDev.FirstName} {jDev.LastName}; ";

            }
            Console.WriteLine($"\nJunior Devlopers with salary > 2200e: {jDevs}");

            jDevs = string.Empty;
            foreach (JuniorDeveloper jDev in ORMapper.From<JuniorDeveloper>(Program.connectionString).Less("Salary", 2300).Or().Like("LastName", "rh%", true))
            {
               jDevs += $"{jDev.FirstName} {jDev.LastName}; ";
            }
            Console.WriteLine($"\nJunior Devlopers with salary < 2300e or lastname name start with 'rh': {jDevs}");

     
            Console.WriteLine("_______________________________________________________________________");
        }

    }
}
