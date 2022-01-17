using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ORM.FrameWork.Cache;
using ORM.FrameWork.Locking;
using ORM.SampleApp.Firma;
using ORM_FrameWork;
using ORM_SampleApp;

namespace ORM.SampleApp
{
    public static class Operations 
    {
   
        public static void CreateDB()
        {
            Console.WriteLine("->Createing DB tables.");

            ORMapper.CreateDbTables(Program.ConnectionString);
            Console.WriteLine("_______________________________________________________________________");
        }
        public static void ClearDB()
        {
            Console.WriteLine("->Droping DB tables.");

            ORMapper.DropDbTables(Program.ConnectionString);
            Console.WriteLine("_______________________________________________________________________");
        }
        public static void InsertObjDemo()
        {

            Console.WriteLine("->Demonstration of inserting a object into DB.");

            Mentor m1 = new Mentor();

            m1.ID = "m1";
            m1.FirstName = "Larry";
            m1.LastName = "Bird";
            m1.BirthDate = new DateTime(1969, 9, 10);
            m1.HireDate = new DateTime(1999, 1, 2);     
            m1.Sex = (int)Person.Gender.MALE;
            m1.Salary = 4500;

            ORMapper.SaveToDb(m1, Program.ConnectionString);
            Console.WriteLine("_______________________________________________________________________");

        }

        public static void UpdateObjDemo()
        {
            Console.WriteLine("\n->Demonstration of update of a object from DB.");

            Mentor m2 = ORMapper.GetByID<Mentor>("m1", Program.ConnectionString); // get Mentor with id m1

            Console.WriteLine($"Salary for {m2.FirstName} {m2.LastName} is { m2.Salary} Dollars.");

            Console.WriteLine("Mentors salary raises for 250$.");
            m2.Salary += 250;

            Console.WriteLine($"Salary for {m2.FirstName} {m2.LastName} is { m2.Salary} Dollars.");
            ORMapper.SaveToDb(m2, Program.ConnectionString);
            Console.WriteLine("_______________________________________________________________________");


        }

        // Foreign Key 1:n 
        public static void OneToNDemo()
        {
            Console.WriteLine("\n->Demonstration of 1:N relation.");


            Mentor m3 = ORMapper.GetByID<Mentor>("m1", Program.ConnectionString);

            Department dep = new Department();
            dep.ID = "d1";
            dep.Name = "DevOps";
            dep.Mentor = m3;       
            ORMapper.SaveToDb(dep, Program.ConnectionString);

            Department dep2 = new Department();
            dep2.ID = "d2";
            dep2.Name = "WebDev";
            dep2.Mentor = m3;
            ORMapper.SaveToDb(dep2, Program.ConnectionString);


            Department dep3 = new Department();
            dep3.ID = "d3";
            dep3.Name = "SWE";
            dep3.Mentor = m3;
            ORMapper.SaveToDb(dep3, Program.ConnectionString);


            dep = ORMapper.GetByID<Department>("d1", Program.ConnectionString);

            Console.WriteLine($"{dep.Name} Mentor: {dep.Mentor.FirstName} {dep.Mentor.LastName}");
            Console.WriteLine("_______________________________________________________________________");


        }
        // Foreign key n:1
        public static void NtoOneDemo()
        {
            Console.WriteLine("\n->Demonstration of N:1 relation.");

            Mentor m1 = ORMapper.GetByID<Mentor>("m1", Program.ConnectionString);

            string departments = string.Empty;

            if (m1.Departments != null)
            {
                foreach (Department dep in m1.Departments)
                {
                    departments += $"{dep.Name}; ";
                }

            }

            Console.WriteLine($"Mentor {m1.FirstName} {m1.LastName} is mentoring in following Departments: {departments}");
            Console.WriteLine("_______________________________________________________________________");

        }

        // m:n
        public static void MtoNDemo()
        {
            Console.WriteLine("\n-> Demonstration of M:N relation.");

            Skill skill = new Skill();
            skill.ID = "s1";
            skill.Name = "C#";
            skill.Mentor = ORMapper.GetByID<Mentor>("m1", Program.ConnectionString);

            JuniorDeveloper jDev1 = new JuniorDeveloper();
            jDev1.ID = "jd1";
            jDev1.FirstName = "Eva";
            jDev1.LastName = "Atkinson";
            jDev1.BirthDate = new DateTime(1998, 3, 12);
            jDev1.HireDate = new DateTime(2021, 2, 25);
            jDev1.Sex = (int)Person.Gender.FEMALE;
            jDev1.Salary = 2100;   
            ORMapper.SaveToDb(jDev1, Program.ConnectionString);

            JuniorDeveloper jDev2 = new JuniorDeveloper();
            jDev2.ID = "jd2";
            jDev2.FirstName = "Joe";
            jDev2.LastName = "Rhodes";
            jDev2.BirthDate = new DateTime(1999, 6, 12);
            jDev2.HireDate = new DateTime(2021, 2, 25);
            jDev2.Sex = (int)Person.Gender.MALE;
            jDev2.Salary = 2500;
            ORMapper.SaveToDb(jDev2, Program.ConnectionString);

            skill.JDevs.Add(jDev1);
            skill.JDevs.Add(jDev2);
            ORMapper.SaveToDb(skill, Program.ConnectionString);

            skill = ORMapper.GetByID<Skill>("s1", Program.ConnectionString);


            string devs = string.Empty;
            foreach (JuniorDeveloper jd in skill.JDevs)
            {
                devs += $"{jd.FirstName} {jd.LastName}; ";
            }

            Console.WriteLine($"Junior Developers that are attending  {skill.Name} are: {devs}");
            Console.WriteLine("_______________________________________________________________________");

        }

        public static void LazyListDemo()
        {
            Console.WriteLine("\n->Demonstration of LazyLoading.");

            Department dep = ORMapper.GetByID<Department>("d1", Program.ConnectionString);

            JuniorDeveloper jDev1 = ORMapper.GetByID<JuniorDeveloper>("jd1", Program.ConnectionString);
            jDev1.Department = dep;
            ORMapper.SaveToDb(jDev1, Program.ConnectionString);

            JuniorDeveloper jDev2 = ORMapper.GetByID<JuniorDeveloper>("jd2", Program.ConnectionString);
            jDev2.Department = dep;
            ORMapper.SaveToDb(jDev2, Program.ConnectionString);

            dep.JDevs.Add(jDev1);
            dep.JDevs.Add(jDev2);
            ORMapper.SaveToDb(dep, Program.ConnectionString);

            dep = ORMapper.GetByID<Department>("d1", Program.ConnectionString);

            string jdevs = string.Empty;
            foreach (JuniorDeveloper jd in dep.JDevs)
            {
                jdevs += $"{jd.FirstName} {jd.LastName}; ";
            }

            Console.WriteLine($"Junior developers in department {dep.Name}: {jdevs}");
            Console.WriteLine("_______________________________________________________________________");

        }

        public static void CacheDemo()
        {
            Console.WriteLine("\n->Demonstaration of Cache.");

            Console.WriteLine("\nNo Cache:");
            for (int f=0; f<5; f++)
            {
                Mentor m1 = ORMapper.GetByID<Mentor>("m1", Program.ConnectionString);
                Console.WriteLine($"Mentor Object with ID: ({m1.ID}) -> instance number: {m1.NumberOfInstance}");
            }

            Console.WriteLine("\nWith Cache:");
            ORMapper.Cache = new Cache();
            for (int f = 0; f < 5; f++)
            {
                Mentor m1 = ORMapper.GetByID<Mentor>("m1", Program.ConnectionString);
                Console.WriteLine($"Mentor Object with ID: ({m1.ID}) -> instance number: {m1.NumberOfInstance}");
            }

            Console.WriteLine("_______________________________________________________________________");
        }

        public static void QueryDemo()
        {
            Console.WriteLine("\n->Demonstaration of Query.");

            string jDevs = string.Empty;
            foreach (JuniorDeveloper jDev in ORMapper.GetQuery<JuniorDeveloper>(Program.ConnectionString).Greater("Salary",2200))
            {
               jDevs += $"{jDev.FirstName} {jDev.LastName}; ";

            }
            Console.WriteLine($"\nJunior Devlopers with salary > 2200e: {jDevs}");

            jDevs = string.Empty;
            foreach (JuniorDeveloper jDev in ORMapper.GetQuery<JuniorDeveloper>(Program.ConnectionString).Less("Salary", 2300).Or().Like("LastName", "rh%", true))
            {
               jDevs += $"{jDev.FirstName} {jDev.LastName}; ";
            }
            Console.WriteLine($"\nJunior Devlopers with salary < 2300e or lastname name start with 'rh': {jDevs}");    
            Console.WriteLine("_______________________________________________________________________");
        }

        public static void LockingDemo()
        {

            Console.WriteLine("\n->Demonstaration of Locking.");

            Console.WriteLine("\nLocking mentor Larry Bird");
            ORMapper.Locking = new LockingDB(Program.ConnectionString);

            Mentor m =  ORMapper.GetByID<Mentor>("m1", Program.ConnectionString);
            ORMapper.Lock(m);
            Console.WriteLine("\nLocking mentor Larry Bird from another session");
            ORMapper.Locking = new LockingDB(Program.ConnectionString);


            try
            {
                ORMapper.Lock(m);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }


            Console.WriteLine("_______________________________________________________________________");
        }

   

    }
}
