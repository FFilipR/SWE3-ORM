using Npgsql;
using ORM_FrameWork;
using ORM_FrameWork.MetaModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.FrameWork.Locking
{
    public class LockingDB : ILocking
    {
        public string SessionKey { get; private set; }
        public string ConnectionString { get; set; }
        public int TimeOut { get; set; } = 240;

        public LockingDB(string connectionString)
        {
            this.SessionKey = Guid.NewGuid().ToString();
            this.ConnectionString = connectionString;
            try
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();
                NpgsqlCommand command = connection.CreateCommand();

                command.CommandText = "CREATE TABLE IF NOT EXISTS Locking (LClass varchar(50) NOT NULL, LObject varchar(50) NOT NULL, LTime timestamptz NOT NULL, LOwner varchar(50))";
                command.ExecuteNonQuery();
                command.Dispose();

                command = connection.CreateCommand();
                command.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS Uq_Locking ON Locking(LClass, LObject)";
                command.ExecuteNonQuery();
                command.Dispose();
                connection.Close();

            }
            catch (Exception)
            {
            }
        }

        private (string ClassKey, string ObjectKey) GetKeys(object obj)
        {
            return (ORMapper.GetEntity(obj).TableName.ToLower(), ORMapper.GetEntity(obj).PKey.ToColumnType(ORMapper.GetEntity(obj).PKey.GetValue(obj)).ToString());
        }

        private string GetLock(object obj)
        {
            (string ClassKey, string ObjectKey) keys = GetKeys(obj);
            string val = null;

            NpgsqlConnection connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = "SELECT LOwner from Locking WHERE LClass = @c AND LObject = @obj";

            command.Parameters.Add(new NpgsqlParameter("@c", keys.ClassKey));
            command.Parameters.Add(new NpgsqlParameter("@obj", keys.ObjectKey));

            using (NpgsqlDataReader dataReader = command.ExecuteReader())
            {
                if (dataReader.Read())
                {
                    val = dataReader.GetString(0);
                }
            }

            command.Parameters.Clear();
            command.Dispose();
            connection.Close();

            return val;
        }

        private void CreateLock(object obj)
        {
            (string ClassKey, string ObjectKey) keys = GetKeys(obj);
            NpgsqlConnection connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = "INSERT INTO Locking (LClass,LObject,LTime,LOwner) VALUES (@c,@obj,Current_Timestamp,@ow)";

            command.Parameters.Add(new NpgsqlParameter("@c", keys.ClassKey));
            command.Parameters.Add(new NpgsqlParameter("@obj", keys.ObjectKey));
            command.Parameters.Add(new NpgsqlParameter("@ow", SessionKey));

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception) { }

            command.Parameters.Clear();
            command.Dispose();
            connection.Close();
        }
        public virtual void Lock(object obj)
        {
            string owner = GetLock(obj);

            if (owner == SessionKey)
                return;
            if (owner == null)
            {
                CreateLock(obj);
                owner = GetLock(obj);
            }
            if (owner != SessionKey)
                throw new LockingException();

        }

        public virtual void Release(object obj)
        {
            (string ClassKey, string ObjectKey) keys = GetKeys(obj);
            NpgsqlConnection connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Locking  WHERE LClass = @c AND LObject = @obj AND LOwner = @ow";

            command.Parameters.Add(new NpgsqlParameter("@c", keys.ClassKey));
            command.Parameters.Add(new NpgsqlParameter("@obj", keys.ObjectKey));
            command.Parameters.Add(new NpgsqlParameter("@ow", SessionKey));

            command.ExecuteNonQuery();
            command.Parameters.Clear();
            command.Dispose();
            connection.Close();
        }

        public virtual void Purge()
        {
            NpgsqlConnection connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            NpgsqlCommand command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Locking  WHERE ((JulianDay(Current_Timestamp) - JulianDay(LTime) * 86400) > @time";
            command.Parameters.Add(new NpgsqlParameter("@c", TimeOut));

            command.ExecuteNonQuery();
            command.Parameters.Clear();
            command.Dispose();
            connection.Close();

        }
    }
}
