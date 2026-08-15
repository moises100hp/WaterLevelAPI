using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Xunit;

namespace WaterLevelAPI.Tests;

public class UserSchemaTests
{
    [Fact]
    public void UsersTable_ShouldContainRoleColumn()
    {
        var dbPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "WaterLevelAPI", "WaterLevel.db"));

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(Users)";

        using var reader = command.ExecuteReader();
        var columns = new List<string>();

        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        Assert.Contains("Role", columns);
    }
}
