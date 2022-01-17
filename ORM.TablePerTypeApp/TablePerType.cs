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
            Console.WriteLine($"\nJunior Developers with salary > 2400e: {jDevs}");

            string persons = string.Empty;

            Console.WriteLine("\nShow all persons:");
            Console.WriteLine("_______________________________________________________________________");
            foreach (Person prs in ORMapper.GetQuery<Person>(Program.ConnectionString))
            {
                    Console.WriteLine($"{prs.FirstName} {prs.LastName} -> {prs.GetType().Name}");
            }

        }
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
    }
}
