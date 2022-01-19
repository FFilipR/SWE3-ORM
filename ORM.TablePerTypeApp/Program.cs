using Microsoft.Extensions.Configuration;
using Npgsql;
using ORM.FrameWork.Cache;
using ORM_FrameWork;
using System;

namespace ORM.TablePerTypeApp
{
    // Class where the table per type application starts 
    public class Program
    {
        // static member which represetns the configuration of the application
        static IConfiguration Config;

        // public property which gets/sets the conneciton string to the database
        public static string ConnectionString { get; set; }

        public static void Main()
        {
            // getting connection data from json file
            Config = new ConfigurationBuilder().AddJsonFile("dbSettings.json", false, true).Build();
            
            // Database 2
            ConnectionString = $"Host={Config["host"]};Username={Config["username"]};Password={Config["password"]};Database={Config["database2"]};";

            ORMapper.DbConnection = new NpgsqlConnection(ConnectionString);
            ORMapper.DbConnection.Open();

            //TablePerType.CreateDB();

            ORMapper.Cache = new CacheTracking();

            TablePerType.Demo();

            //TablePerType.ClearDB();

            ORMapper.DbConnection.Close();
       
        }


    }
}
