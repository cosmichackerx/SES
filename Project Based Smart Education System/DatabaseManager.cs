using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MySqlConnector;

namespace SmartEducationSystem
{
    public class User
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Student";
    }

    public class AttendanceRecord
    {
        public string StudentID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = "Present";
        public int ClassIndex { get; set; }
    }

    public class DbConfig
    {
        public string Mode { get; set; } = "JSON"; // "MySQL" or "JSON"
        public string Server { get; set; } = "localhost";
        public string Database { get; set; } = "smartedu";
        public string User { get; set; } = "root";
        public string Password { get; set; } = "";
    }

    public static class DatabaseManager
    {
        private static string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public static DbConfig CurrentConfig { get; private set; } = new DbConfig();

        public static string ConnectionString => $"Server={CurrentConfig.Server};Database={CurrentConfig.Database};User={CurrentConfig.User};Password={CurrentConfig.Password};Connection Timeout=1;";
        
        private static string JsonFallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");
        private static string JsonAttendancePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attendance.json");

        static DatabaseManager()
        {
            if (File.Exists(ConfigPath))
            {
                try
                {
                    string json = File.ReadAllText(ConfigPath);
                    CurrentConfig = JsonSerializer.Deserialize<DbConfig>(json) ?? new DbConfig();
                }
                catch { }
            }
        }

        public static void SaveConfig()
        {
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(CurrentConfig, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static string CurrentUserRole { get; private set; } = "";
        public static string CurrentUserEmail { get; private set; } = "";

        private static List<User> LoadJsonUsers()
        {
            if (!File.Exists(JsonFallbackPath))
            {
                File.WriteAllText(JsonFallbackPath, "[]");
            }
            string json = File.ReadAllText(JsonFallbackPath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private static void SaveJsonUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(JsonFallbackPath, json);
        }

        public static async Task<bool> RegisterUserAsync(string email, string password, string role = "Student")
        {
            if (CurrentConfig.Mode == "JSON") return RegisterUserJson(email, password, role);

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    // Create table if not exists
                    using (var cmdCreate = new MySqlCommand("CREATE TABLE IF NOT EXISTS Users (Email VARCHAR(255) PRIMARY KEY, Password VARCHAR(255), Role VARCHAR(50) DEFAULT 'Student')", conn))
                    {
                        await cmdCreate.ExecuteNonQueryAsync();
                    }

                    // Attempt to Alter Table in case Role column doesn't exist from older versions
                    try
                    {
                        using (var cmdAlter = new MySqlCommand("ALTER TABLE Users ADD COLUMN Role VARCHAR(50) DEFAULT 'Student'", conn))
                        {
                            await cmdAlter.ExecuteNonQueryAsync();
                        }
                    } catch { /* Column already exists */ }

                    using (var cmd = new MySqlCommand("INSERT INTO Users (Email, Password, Role) VALUES (@email, @password, @role)", conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@role", role);
                        await cmd.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch
            {
                return RegisterUserJson(email, password, role);
            }
        }

        private static bool RegisterUserJson(string email, string password, string role)
        {
            var users = LoadJsonUsers();
            if (users.Exists(u => u.Email == email)) return false; // Already exists
            users.Add(new User { Email = email, Password = password, Role = role });
            SaveJsonUsers(users);
            return true;
        }

        public static async Task<bool> LoginUserAsync(string email, string password)
        {
            CurrentUserRole = "";
            CurrentUserEmail = "";
            if (CurrentConfig.Mode == "JSON") return LoginUserJson(email, password);

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    // Create table if not exists just in case
                    using (var cmdCreate = new MySqlCommand("CREATE TABLE IF NOT EXISTS Users (Email VARCHAR(255) PRIMARY KEY, Password VARCHAR(255), Role VARCHAR(50) DEFAULT 'Student')", conn))
                    {
                        await cmdCreate.ExecuteNonQueryAsync();
                    }

                    using (var cmd = new MySqlCommand("SELECT Role FROM Users WHERE Email = @email AND Password = @password", conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", password);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                CurrentUserRole = reader.GetString(0);
                                CurrentUserEmail = email;
                                return true;
                            }
                            return false;
                        }
                    }
                }
            }
            catch
            {
                return LoginUserJson(email, password);
            }
        }

        private static bool LoginUserJson(string email, string password)
        {
            var users = LoadJsonUsers();
            var user = users.Find(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                CurrentUserRole = user.Role;
                CurrentUserEmail = email;
                return true;
            }
            return false;
        }

        public static async Task<List<User>> GetAllUsersAsync()
        {
            if (CurrentConfig.Mode == "JSON") return LoadJsonUsers();

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    var list = new List<User>();
                    using (var cmd = new MySqlCommand("SELECT Email, Password, Role FROM Users", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new User { Email = reader.GetString(0), Password = reader.GetString(1), Role = reader.GetString(2) });
                        }
                    }
                    return list;
                }
            }
            catch
            {
                return LoadJsonUsers();
            }
        }

        public static async Task<bool> DeleteUserAsync(string email)
        {
            if (CurrentConfig.Mode == "JSON") return DeleteUserJson(email);

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand("DELETE FROM Users WHERE Email=@e", conn))
                    {
                        cmd.Parameters.AddWithValue("@e", email);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch
            {
                return DeleteUserJson(email);
            }
        }

        private static bool DeleteUserJson(string email)
        {
            var users = LoadJsonUsers();
            users.RemoveAll(u => u.Email == email);
            SaveJsonUsers(users);
            return true;
        }

        public static async Task<List<AttendanceRecord>> GetAttendanceAsync(int classIndex, string date)
        {
            if (CurrentConfig.Mode == "JSON") return GetAttendanceJson(classIndex, date);

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmdCreate = new MySqlCommand("CREATE TABLE IF NOT EXISTS Attendance (StudentID VARCHAR(50), Name VARCHAR(255), Date VARCHAR(50), Status VARCHAR(50), ClassIndex INT, PRIMARY KEY(StudentID, Date, ClassIndex))", conn))
                    {
                        await cmdCreate.ExecuteNonQueryAsync();
                    }

                    var list = new List<AttendanceRecord>();
                    using (var cmd = new MySqlCommand("SELECT StudentID, Name, Status FROM Attendance WHERE ClassIndex = @c AND Date = @d", conn))
                    {
                        cmd.Parameters.AddWithValue("@c", classIndex);
                        cmd.Parameters.AddWithValue("@d", date);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                list.Add(new AttendanceRecord
                                {
                                    StudentID = reader.GetString(0),
                                    Name = reader.GetString(1),
                                    Status = reader.GetString(2),
                                    Date = date,
                                    ClassIndex = classIndex
                                });
                            }
                        }
                    }
                    return list;
                }
            }
            catch
            {
                return GetAttendanceJson(classIndex, date);
            }
        }

        private static List<AttendanceRecord> GetAttendanceJson(int classIndex, string date)
        {
            if (File.Exists(JsonAttendancePath))
            {
                string json = File.ReadAllText(JsonAttendancePath);
                var all = JsonSerializer.Deserialize<List<AttendanceRecord>>(json) ?? new List<AttendanceRecord>();
                return all.FindAll(a => a.ClassIndex == classIndex && a.Date == date);
            }
            return new List<AttendanceRecord>();
        }

        public static async Task UpdateAttendanceAsync(int classIndex, string date, string studentId, string name, string status)
        {
            if (CurrentConfig.Mode == "JSON")
            {
                UpdateAttendanceJson(classIndex, date, studentId, name, status);
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(@"
                        INSERT INTO Attendance (StudentID, Name, Date, Status, ClassIndex) 
                        VALUES (@s, @n, @d, @st, @c)
                        ON DUPLICATE KEY UPDATE Status = @st", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", studentId);
                        cmd.Parameters.AddWithValue("@n", name);
                        cmd.Parameters.AddWithValue("@d", date);
                        cmd.Parameters.AddWithValue("@st", status);
                        cmd.Parameters.AddWithValue("@c", classIndex);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    return;
                }
            }
            catch
            {
                UpdateAttendanceJson(classIndex, date, studentId, name, status);
            }
        }

        private static void UpdateAttendanceJson(int classIndex, string date, string studentId, string name, string status)
        {
            List<AttendanceRecord> all = new List<AttendanceRecord>();
            if (File.Exists(JsonAttendancePath))
            {
                string json = File.ReadAllText(JsonAttendancePath);
                all = JsonSerializer.Deserialize<List<AttendanceRecord>>(json) ?? new List<AttendanceRecord>();
            }
            var existing = all.Find(a => a.ClassIndex == classIndex && a.Date == date && a.StudentID == studentId);
            if (existing != null) existing.Status = status;
            else all.Add(new AttendanceRecord { ClassIndex = classIndex, Date = date, StudentID = studentId, Name = name, Status = status });
            
            File.WriteAllText(JsonAttendancePath, JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
