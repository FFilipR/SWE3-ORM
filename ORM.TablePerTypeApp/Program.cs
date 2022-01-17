using Microsoft.Extensions.Configuration;
using Npgsql;
using ORM.FrameWork.Cache;
using ORM_FrameWork;
using System;

namespace ORM.TablePerTypeApp
{
    public class Program
    {
        static IConfiguration Config;
        public static string ConnectionString { get; set; }

        public static void Main()
        {
            // getting connection data from json file
            Config = new ConfigurationBuilder().AddJsonFile("dbSettings.json", false, true).Build();
            // Different Database from ORM.SampleApp
            ConnectionString = $"Host={Config["host"]};Username={Config["username"]};Password={Config["password"]};Database={Config["database2"]};";

            ORMapper.DbConnection = new NpgsqlConnection(ConnectionString);
            ORMapper.DbConnection.Open();

            ORMapper.Cache = new CacheTracking();

            TablePerType.Demo();

            ORMapper.DbConnection.Close();
       
        }


    }
}
