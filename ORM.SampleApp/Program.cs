using Microsoft.Extensions.Configuration;
using Npgsql;
using ORM.FrameWork.Cache;
using ORM.SampleApp;
using ORM_FrameWork;
using ORM_FrameWork.MetaModels;
using System;
namespace ORM_SampleApp
{
    public class Program
    {
        static IConfiguration config;
        public static string connectionString;

        public static void Main()
        {
            // getting connection data from json file
            config = new ConfigurationBuilder().AddJsonFile("dbSettings.json", false, true).Build();
            connectionString = $"Host={config["host"]};Username={config["username"]};Password={config["password"]};Database={config["database"]};MaxPoolSize={config["maxPsize"]};Timeout={config["timeout"]};Pooling=true;CommandTimeout={config["50"]};";

            ORMapper.DbConnection = new NpgsqlConnection(connectionString);
            ORMapper.DbConnection.Open();

            ORMapper.Cache = new CacheTracking();

            Operations.InsertMentor();
            Operations.UpdateMentorSalary();
            Operations.GetDepartmentsMentor();
            Operations.GetAllMentorsDepartments();
            Operations.MtoNRelation();
            Operations.LazyList();
            Operations.CacheDemo();

            ORMapper.DbConnection.Close();

        }
     
    
    }
}
