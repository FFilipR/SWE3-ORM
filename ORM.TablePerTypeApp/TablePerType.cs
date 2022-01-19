using ORM.TablePerTypeApp.Firma;
using ORM_FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.TablePerTypeApp
{
    public static class TablePerType
    {
        // Class which represents a demonstration of Table Per Type Inheritance
        public static void Demo()
        {
            Console.WriteLine("\n->Demonstration of Table Per Type.");

            InsertMentor();
            InsertJdevs();

            Mentor m = ORMapper.GetByID<Mentor>("m1", Program.ConnectionString);
            Console.WriteLine($"\nMentor 'm1' -> {m.FirstName} {m.LastName}");

            Person p = new Person();
            p.ID = "p1";
            p.FirstName = "Adam";
            p.LastName = "Black";
            p.BirthDate = new DateTime(1972, 3, 28);
            p.Sex = (int)Person.Gender.MALE;

            ORMapper.SaveToDb(p, Program.ConnectionString);

            string jDevs = string.Empty;
            foreach(JuniorDeveloper jDev in ORMapper.GetQuery<JuniorDeveloper>(Program.ConnectionString).Greater("Salary",2400))
            {
                jDevs += $"{jDev.FirstName} {jDev.LastName}; ";
            }
            
            Console.WriteLine($"Junior Developers with salary > 2400e: {jDevs}");

            string persons = string.Empty;
            Console.WriteLine("_______________________________________________________________________");
            Console.WriteLine("\nShow all persons:\n");

            foreach (Person prs in ORMapper.GetQuery<Person>(Program.ConnectionString))
            {
                    Console.WriteLine($"{prs.FirstName} {prs.LastName} -> {prs.GetType().Name}");
            }

        }

        // public method which creates a new mentor and saves it into the database
        public static void InsertMentor()
        {
            Mentor m1 = new Mentor();

            m1.ID = "m1";
            m1.FirstName = "John";
            m1.LastName = "Philips";
            m1.BirthDate = new DateTime(1974, 5, 12);
            m1.HireDate = new DateTime(2004, 5, 6);
            m1.Sex = (int)Person.Gender.MALE;
            m1.Salary = 4800;

            ORMapper.SaveToDb(m1, Program.ConnectionString);
        }

        // public method which creates two junior developers and saves them into the database
        public static void InsertJdevs()
        {
            JuniorDeveloper jDev1 = new JuniorDeveloper();
            jDev1.ID = "jd1";
            jDev1.FirstName = "Tom";
            jDev1.LastName = "Ford";
            jDev1.BirthDate = new DateTime(1998, 6, 4);
            jDev1.HireDate = new DateTime(2020, 2, 24);
            jDev1.Sex = (int)Person.Gender.MALE;
            jDev1.Salary = 2700;
            ORMapper.SaveToDb(jDev1, Program.ConnectionString);

            JuniorDeveloper jDev2 = new JuniorDeveloper();
            jDev2.ID = "jd2";
            jDev2.FirstName = "Rebeca";
            jDev2.LastName = "McGarden";
            jDev2.BirthDate = new DateTime(2000, 1, 9);
            jDev2.HireDate = new DateTime(2021, 2, 25);
            jDev2.Sex = (int)Person.Gender.FEMALE;
            jDev2.Salary = 1900;
            ORMapper.SaveToDb(jDev2, Program.ConnectionString);
        }

        // public method which creates the database 
        public static void CreateDB()
        {
            Console.WriteLine("->Createing DB tables.");

            ORMapper.CreateDbTables2(Program.ConnectionString);
            Console.WriteLine("_______________________________________________________________________");
        }

        // public method which clears the database
        public static void ClearDB()
        {
            Console.WriteLine("->Droping DB tables.");

            ORMapper.DropDbTables2(Program.ConnectionString);
            Console.WriteLine("_______________________________________________________________________");
        }
    }
}
