﻿using Microsoft.Extensions.Configuration;
using Npgsql;
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
            // get connection data from json file
            config = new ConfigurationBuilder().AddJsonFile("dbSettings.json", false, true).Build();
            connectionString = $"Host={config["host"]};Username={config["username"]};Password={config["password"]};Database={config["database"]}";

            ORMapper.DbConnection = new NpgsqlConnection(connectionString);
            ORMapper.DbConnection.Open();

            DbOperations.InsertMentor();
            DbOperations.UpdateMentorSalary();
            DbOperations.GetDepartmentsMentor();
            DbOperations.GetAllMentorsDepartments();

            ORMapper.DbConnection.Close();


        }
     
    
    }
}
