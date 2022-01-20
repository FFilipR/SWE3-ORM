using NUnit.Framework;
using System;
using ORM.SampleApp.Firma;
using ORM_FrameWork.MetaModels;
using System.Linq;
using ORM_FrameWork;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Data;

namespace ORM.Tests
{
    // Class which tests meta models functionality
    public class MetaModelTests
    {
        public Entity entity;
        public Mentor mentor;
        public Department department;
         
        // static member which represetns the configuration of the application
        static IConfiguration Config;
        Mock<IDataReader> mockedReader = new Mock<IDataReader>();
        Mock<IDbDataParameter> mockedParameter = new Mock<IDbDataParameter>();
        Mock<IDbCommand> mockedCommand = new Mock<IDbCommand>();
        Mock<IDbConnection> mockedConnection = new Mock<IDbConnection>();

        // Setup of object that will be used in tests
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

            entity = new Entity(typeof(Mentor));

            mentor = new Mentor();
            mentor.FirstName = "Ben";
            mentor.LastName = "Smith";
            mentor.BirthDate = new DateTime(1971, 5, 5);
            mentor.ID = "m0";
            mentor.Sex = Person.Gender.MALE;
            mentor.HireDate = new DateTime(2011, 6, 2);
            mentor.Salary = 8000;
            

            department = new Department();
            department.Name = "Artificial Intelligence";
            department.ID = "d0";
            department.Mentor = mentor;
        }

        [TearDown]
        public void TearDown()
        {
            mockedReader.Object.Close();
            mockedCommand.Object.Dispose();
            mockedConnection.Object.Close();
        }

        // Tests if the retrieved values are correct
        [Test]
        public void GetObjectValue()
        {
            var fieldID = entity.Fields[4].GetValue(mentor);
            var fieldGender = entity.Fields[8].GetValue(mentor);

            Assert.AreEqual("m0", fieldID);
            Assert.AreEqual(mentor.Sex, fieldGender);
        }

        // Tests if the created object consists of the correct values for the type
        [Test]
        public void CreateEntityObject()
        {
            var expectedName = "Mentors";
            var ent = new Entity(typeof(Mentor));

            Assert.AreEqual(expectedName, ent.TableName);
            Assert.AreEqual(typeof(Mentor), ent.Member);
        }

        // Tests if the object has the correct value after the change
        [Test]
        public void SetObjectValue()
        {
            string expectedID = "m0";
            entity.Fields[4].SetValue(mentor, expectedID);

            int expectedSalary = 8000;
            entity.Fields[0].SetValue(mentor, expectedSalary);

            Assert.AreEqual(expectedID, entity.Fields[4].GetValue(mentor));
            Assert.AreEqual(expectedSalary, entity.Fields[0].GetValue(mentor));
        }

      
        //Tests if the transformed type and value are correct
        [Test]
        public void ToColumnTypeTestRefToPkey()
        {
            var depEntity = ORMapper.GetEntity(department);
            var actualValue = depEntity.Fields[2].ToColumnType(department.Mentor);

            Assert.AreEqual(typeof(string), actualValue.GetType());
            Assert.AreEqual(department.Mentor.ID, actualValue);
        }

        // Tests if the transformed type and value are correct
        [Test]
        public void ToColumnTypeTest()
        {
            var actualValue = entity.Fields[8].ToColumnType(entity.Fields[8].GetValue(mentor));

            Assert.AreEqual(mentor.Sex, actualValue);
        }

        // Tests if the given type has fields 
        [Test]
        public void GetFieldsTest()
        {
            var actualValue = entity.getFields(mentor.GetType());
            Assert.IsNotEmpty(actualValue);
        }

        // Tests if the transformed type and value are correct
        [Test]
        public void ToFieldTypeTestIntToEnum()
        {
            var actualValue = entity.Fields[8].ToFieldType(0,null,mockedConnection.Object.ConnectionString);

            Assert.IsTrue(actualValue.GetType().IsEnum);
            Assert.AreEqual(mentor.Sex, actualValue);
        }

    }
}