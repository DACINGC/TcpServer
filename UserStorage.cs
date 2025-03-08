using System;
using System.Data.SQLite;

namespace TCPServerMax
{
    public class User
    {
        public int UID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserStorage
    {
        private static readonly string DatabaseFileName = "users.db";
        private static readonly string DatabaseFilePath = @"F:\Visualstudio\VsProj\csharp\TcpServer\TCPServerMax\SqliteData\" + DatabaseFileName;
        private SQLiteConnection _connection;

        public UserStorage()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // 如果数据库文件不存在，则创建
            if (!File.Exists(DatabaseFilePath))
            {
                SQLiteConnection.CreateFile(DatabaseFilePath);
                Console.WriteLine($"数据库文件不存在，已创建: {DatabaseFilePath}");
            }

            // 连接到数据库
            _connection = new SQLiteConnection($"Data Source={DatabaseFilePath};Version=3;");
            _connection.Open();

            // 创建用户表（如果不存在）
            using (var command = new SQLiteCommand(
                "CREATE TABLE IF NOT EXISTS Users (" +
                "UID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "Username TEXT UNIQUE, " +
                "Password TEXT)", _connection))
            {
                command.ExecuteNonQuery();
            }

            // 插入默认数据（如果表为空）
            using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Users", _connection))
            {
                var count = Convert.ToInt32(command.ExecuteScalar());
                if (count == 0)
                {
                    InsertDefaultUsers();
                    Console.WriteLine("已插入默认用户数据。");
                }
            }
        }

        private void InsertDefaultUsers()
        {
            var defaultUsers = new List<User>
            {
                new User { Username = "admin", Password = "admin123" },
                new User { Username = "guest", Password = "guest123" }
            };

            foreach (var user in defaultUsers)
            {
                AddUser(user.Username, user.Password);
            }
        }

        /// <summary>
        /// 添加新用户
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool AddUser(string username, string password)
        {
            if (UsernameExists(username))
            {
                Console.WriteLine($"用户 {username} 已存在，添加失败。");
                return false;
            }

            using (var command = new SQLiteCommand(
                "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)", _connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);
                command.ExecuteNonQuery();
            }

            Console.WriteLine($"用户 {username} 已添加。");
            return true;
        }

        /// <summary>
        /// 验证用户名和密码是否匹配
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateUser(string username, string password)
        {
            using (var command = new SQLiteCommand(
                "SELECT Password FROM Users WHERE Username = @Username", _connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var storedPassword = reader["Password"].ToString();
                        return storedPassword == password;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 是否存在用户
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool UsernameExists(string username)
        {
            using (var command = new SQLiteCommand(
                "SELECT COUNT(*) FROM Users WHERE Username = @Username", _connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
        public int GetNextUID()
        {
            using (var command = new SQLiteCommand("SELECT MAX(UID) FROM Users", _connection))
            {
                var result = command.ExecuteScalar();
                if (result == DBNull.Value)
                {
                    return 0; // 如果表为空，从 0 开始
                }
                return Convert.ToInt32(result) + 1;
            }
        }
    }
}