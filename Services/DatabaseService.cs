using Microsoft.Data.Sqlite;
using ProjectTimeEstimator.Models;
using System.Diagnostics;
using System.IO;

namespace ProjectTimeEstimator.Services;

public class DatabaseService
{
    private readonly string _dbPath;

    public DatabaseService()
    {
        // Single-file uygulamada doğru dizini bulmak için bu yöntemi kullanıyoruz
        var processModule = Process.GetCurrentProcess().MainModule;
        string outputFolder;
        
        if (processModule != null)
        {
             outputFolder = Path.GetDirectoryName(processModule.FileName) ?? AppContext.BaseDirectory;
        }
        else
        {
             outputFolder = AppContext.BaseDirectory;
        }

        _dbPath = Path.Join(outputFolder, "TimelineIQ.db");
    }

    private SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={_dbPath}");
    }

    public void Initialize()
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Projects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    EstimatedHours REAL DEFAULT 0,
                    ActualHours REAL DEFAULT 0,
                    StartDate TEXT NOT NULL,
                    EndDate TEXT,
                    Status TEXT DEFAULT 'Planned'
                );
            ";
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Database Initialize");
            throw; // Hatayı yukarı fırlat ki App.xaml.cs yakalasın
        }
    }

    public void AddProject(Project project)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Projects (Name, Description, EstimatedHours, ActualHours, StartDate, EndDate, Status)
                VALUES (@Name, @Description, @EstimatedHours, @ActualHours, @StartDate, @EndDate, @Status);
            ";

            command.Parameters.AddWithValue("@Name", project.Name);
            command.Parameters.AddWithValue("@Description", project.Description ?? string.Empty);
            command.Parameters.AddWithValue("@EstimatedHours", project.EstimatedHours);
            command.Parameters.AddWithValue("@ActualHours", project.ActualHours);
            command.Parameters.AddWithValue("@StartDate", project.StartDate.ToString("o"));
            command.Parameters.AddWithValue("@EndDate", project.EndDate.HasValue ? project.EndDate.Value.ToString("o") : DBNull.Value);
            command.Parameters.AddWithValue("@Status", project.Status);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "AddProject");
            throw new Exception("Proje eklenirken bir hata oluştu.");
        }
    }

    public void UpdateProject(Project project)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Projects 
                SET Name = @Name,
                    Description = @Description,
                    EstimatedHours = @EstimatedHours,
                    ActualHours = @ActualHours,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    Status = @Status
                WHERE Id = @Id;
            ";

            command.Parameters.AddWithValue("@Id", project.Id);
            command.Parameters.AddWithValue("@Name", project.Name);
            command.Parameters.AddWithValue("@Description", project.Description ?? string.Empty);
            command.Parameters.AddWithValue("@EstimatedHours", project.EstimatedHours);
            command.Parameters.AddWithValue("@ActualHours", project.ActualHours);
            command.Parameters.AddWithValue("@StartDate", project.StartDate.ToString("o"));
            command.Parameters.AddWithValue("@EndDate", project.EndDate.HasValue ? project.EndDate.Value.ToString("o") : DBNull.Value);
            command.Parameters.AddWithValue("@Status", project.Status);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "UpdateProject");
            throw new Exception("Proje güncellenirken bir hata oluştu.");
        }
    }

    public void DeleteProject(int id)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Projects WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "DeleteProject");
            throw new Exception("Proje silinirken bir hata oluştu.");
        }
    }

    public List<Project> GetAllProjects()
    {
        var projects = new List<Project>();
        try
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Projects ORDER BY StartDate DESC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                projects.Add(new Project
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    EstimatedHours = reader.GetDouble(3),
                    ActualHours = reader.GetDouble(4),
                    StartDate = DateTime.Parse(reader.GetString(5)),
                    EndDate = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                    Status = reader.GetString(7)
                });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "GetAllProjects");
            throw new Exception("Veriler yüklenirken bir hata oluştu.");
        }

        return projects;
    }
}
