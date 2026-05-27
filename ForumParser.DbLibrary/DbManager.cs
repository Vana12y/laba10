using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace ForumParser.DbLibrary
{
    public class DbManager
    {
        private readonly string _connStr;

        public DbManager(string dbPath)
        {
            _connStr = $"Data Source={dbPath}";
            InitDb();
        }

        private void InitDb()
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Message TEXT NOT NULL
            );";
            cmd.ExecuteNonQuery();
        }

        public ForumMessage GetById(long id)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Message FROM Messages WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                return new ForumMessage
                {
                    Id = rdr.GetInt64(0),
                    Name = rdr.GetString(1),
                    Message = rdr.GetString(2)
                };
            }
            return null;
        }

        public List<ForumMessage> GetByName(string name)
        {
            var res = new List<ForumMessage>();
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Message FROM Messages WHERE Name = $name";
            cmd.Parameters.AddWithValue("$name", name);

            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                res.Add(new ForumMessage
                {
                    Id = rdr.GetInt64(0),
                    Name = rdr.GetString(1),
                    Message = rdr.GetString(2)
                });
            }
            return res;
        }

        public void Add(ForumMessage msg)
        {
            if (msg.Name.Length > 256) throw new ArgumentException("Name too long");
            if (msg.Message.Length > 8096) throw new ArgumentException("Message too long");

            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Messages (Id, Name, Message) VALUES ($id, $name, $message)";
            cmd.Parameters.AddWithValue("$id", msg.Id);
            cmd.Parameters.AddWithValue("$name", msg.Name);
            cmd.Parameters.AddWithValue("$message", msg.Message);
            cmd.ExecuteNonQuery();
        }

        public void Update(long id, string newMessage)
        {
            if (newMessage.Length > 8096) throw new ArgumentException("Message too long");

            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Messages SET Message = $message WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$message", newMessage);
            cmd.ExecuteNonQuery();
        }

        public void Delete(long id)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Messages WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }
    }
}