using Microsoft.Extensions.Configuration;
using Moq;
using Npgsql;
using NUnit.Framework;
using ORM.SampleApp.Firma;
using ORM_FrameWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.Tests
{
   // Class which tests the ORMapper functionalities
    public class ORMFrameworkTests
    {

        // static member which represetns the configuration of the application
        static IConfiguration Config;
        Mock<IDataReader> mockedReader = new Mock<IDataReader>();
        Mock<IDbDataParameter> mockedParameter = new Mock<IDbDataParameter>();
        Mock<IDbCommand> mockedCommand = new Mock<IDbCommand>();
        Mock<IDbConnection> mockedConnection = new Mock<IDbConnection>();
        Mentor mentor = new Mentor();

       
        // Mock setup
        [SetUp]
        public void Setup()
        {
            // getting connection data from json file
            Config = new ConfigurationBuilder().AddJsonFile("D:\\Private\\FH\\semester 5\\SWE3 - Software Engineering 3\\ORM.App\\ORM.SampleApp\\bin\\Debug\\net5.0\\dbSettings.json", false, true).Build();

            mockedCommand.Setup(cmd => cmd.ExecuteReader()).Returns(mockedReader.Object);
            mockedCommand.Setup(cmd => cmd.CreateParameter()).Returns(mockedParameter.Object);
            mockedCommand.Setup(cmd => cmd.Parameters.Add(mockedCommand.Object.CreateParameter()));

            mockedConnection.Setup(conn => conn.Open());
            mockedConnection.Setup(conn => conn.CreateCommand()).Returns(mockedCommand.Object);
            mockedConnection.Setup(conn => conn.ConnectionString).Returns($"Host={Config["host"]};Username={Config["username"]};Password={Config["password"]};Database={Config["database1"]};");

            mockedConnection.Object.Open();

            mentor.ID = "m0";
            mentor.FirstName = "Tyler";
            mentor.LastName = "Rowland";
            mentor.Sex = Person.Gender.MALE;
            mentor.BirthDate = new DateTime(1980, 3, 22);
            mentor.HireDate = new DateTime(2009, 9, 04);
            mentor.Salary = 9000;
        }


        [TearDown]
        public void TearDown()
        {
            mockedReader.Object.Close();
            mockedCommand.Object.Dispose();
            mockedConnection.Object.Close();
        }

        //Tests if if the returned object has the same type as the original object and the correct values
        [Test]
        public void CreateObjectTestGetsCorrectly()
        {
            
            ORMapper.SaveToDb(mentor, mockedConnection.Object.ConnectionString);

            var actualMentor = (Mentor)ORMapper.Create(typeof(Mentor), mentor.ID, null, mockedConnection.Object.ConnectionString);
            Assert.AreEqual(mentor.ID, actualMentor.ID);
            Assert.AreEqual(mentor.GetType(), actualMentor.GetType());

            ORMapper.DeleteFromDb(mentor, mockedConnection.Object.ConnectionString);
        }

        // Tests if the returned object has the expected values that fit the type
        [Test]
        public void GetEntityTest()
        {
            var expectedName = "Mentors";
            var ent = ORMapper.GetEntity(typeof(Mentor));

            Assert.AreEqual(expectedName, ent.TableName);
            Assert.AreEqual(typeof(Mentor), ent.Member);
        }

       //Tests if the object is sucesfully created
        [Test]
        public void CreateObjectTest()
        {
            ORMapper.Create(typeof(Mentor), mentor.ID, null, mockedConnection.Object.ConnectionString);

            Assert.IsTrue(true);
        }

        [Test]
        public void RemoveObjectTest()
        {
            ORMapper.Create(typeof(Mentor), mentor.ID, null, mockedConnection.Object.ConnectionString);
            ORMapper.DeleteFromDb(mentor, mockedConnection.Object.ConnectionString);
            
            Assert.IsTrue(true);
        }
    }
}
