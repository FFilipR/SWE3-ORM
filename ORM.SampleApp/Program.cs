using Microsoft.Extensions.Configuration;
using Npgsql;
using ORM.SampleApp;
using ORM_FrameWork;
using ORM_FrameWork.Attributes;
using System;
namespace ORM_SampleApp
{
    public class Program
    {
        static IConfiguration config;
        static string connectionString;

        static void Main()
        {
            // get connection data from json file
            config = new ConfigurationBuilder().AddJsonFile("dbSettings.json", false, true).Build();
            connectionString = $"Host={config["host"]};Username={config["username"]};Password={config["password"]};Database={config["database"]}";

            ORMapper.DbConnection = new NpgsqlConnection(connectionString);
            ORMapper.DbConnection.Open();

            DbOperations.InsertObject();
            DbOperations.UpdateMentorSalary();
            DbOperations.GetMentorAndShowDepartments();

            ORMapper.DbConnection.Close();



         }
     
    
    }
}
