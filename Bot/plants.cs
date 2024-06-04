

using Bot;
using Npgsql;

public class Database
{
    NpgsqlConnection con = new NpgsqlConnection(Constants.Connect);

    public async Task AddPlantAsync(long chatId, string plantName, DateTime dateAdded, string recommendation)
    {
        var sql = "insert into public.\"plants\"(\"ChatId\",\"PlantName\",\"DateAdded\",\"Recommendation\")"
            + "values (@ChatId, @PlantName, @DateAdded, @Recommendation)";
        NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("ChatId", chatId);
        cmd.Parameters.AddWithValue("PlantName", plantName);
        cmd.Parameters.AddWithValue("DateAdded", dateAdded);
        cmd.Parameters.AddWithValue("Recommendation", recommendation);
        await con.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
        await con.CloseAsync();
    }

    public async Task RemovePlantAsync(long chatId, string plantName)
    {
        await con.OpenAsync();
        var sql = "delete from public.\"plants\" where \"ChatId\" = @ChatId and \"PlantName\" = @PlantName";
        NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("ChatId", chatId);
        cmd.Parameters.AddWithValue("PlantName", plantName);
        await cmd.ExecuteNonQueryAsync();
        await con.CloseAsync();
    }

    public async Task<List<Plant>> SelectPlants(long chatId)
    {
        List<Plant> plants = new List<Plant>();
        await con.OpenAsync();
        var sql = "select \"ChatId\",\"PlantName\",\"DateAdded\",\"Recommendation\" from public.\"plants\" where \"ChatId\" = @ChatId";
        NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
        cmd.Parameters.AddWithValue("ChatId", chatId);
        NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            plants.Add(new Plant
            {
                ChatId = reader.GetInt64(0),
                PlantName = reader.GetString(1),
                DateAdded = reader.GetDateTime(2),
                Recommendation = reader.GetString(3)
            });
        }
        await con.CloseAsync();
        return plants;
    }
}

