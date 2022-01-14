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
        static IConfiguration Config;
        public static string ConnectionString { get; set; }

        public static void Main()
        {
            // getting connection data from json file
            Config = new ConfigurationBuilder().AddJsonFile("dbSettings.json", false, true).Build();
            ConnectionString = $"Host={Config["host"]};Username={Config["username"]};Password={Config["password"]};Database={Config["database"]};MaxPoolSize={Config["maxPsize"]};Timeout={Config["timeout"]};Pooling=true;CommandTimeout={Config["50"]};";

            ORMapper.DbConnection = new NpgsqlConnection(ConnectionString);
            ORMapper.DbConnection.Open();

            //ORMapper.Cache = new CacheTracking();

            //Operations.CreateDB();
            
            Operations.InsertObjDemo();
            Operations.UpdateObjDemo();
            Operations.OneToNDemo();
            Operations.NtoOneDemo();
            Operations.MtoNDemo();
            Operations.LazyListDemo();
            Operations.CacheDemo();
            Operations.QueryDemo();
            Operations.LockingDemo();

           //Operations.ClearDB();

            ORMapper.DbConnection.Close();

        }
     
    
    }
}
