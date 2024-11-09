using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WorldviewShareClient.Models;

namespace WorldviewShareClient;

public static class EnvironmentHelper
{
    private static EnvironmentSettings? _settings;

    public static EnvironmentSettings GetEnvironment()
    {
        if (_settings == null)
        {
            if (!File.Exists("environment.env"))
            {
                File.Copy("environment.env.default", "environment.env");

                _settings = new EnvironmentSettings
                {
                    Id = Guid.NewGuid(),
                    Name = string.Empty
                };

                using var writer = new StreamWriter("environment.env");
                var json = JsonSerializer.Serialize(_settings);
                writer.Write(json);
            }
            else
            {
                using var reader = new StreamReader("environment.env");
                var json = reader.ReadToEnd();
                _settings = JsonSerializer.Deserialize<EnvironmentSettings>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                reader.Close();

                var result = Task.Run(() => CheckIfUserExists(_settings.Id.ToString())).Result;
                if (!result)
                {
                    File.Delete("environment.env");
                    File.Copy("environment.env.default", "environment.env");

                    _settings = new EnvironmentSettings
                    {
                        Id = Guid.NewGuid(),
                        Name = string.Empty
                    };

                    using var writer = new StreamWriter("environment.env");
                    writer.Write(JsonSerializer.Serialize(_settings));
                }
            }
        }

        return _settings;
    }

    private static async Task<bool> CheckIfUserExists(string userId)
    {
        try
        {
            var client = HttpClientFactory.GetClient();
            var backEndUserInfo = await client.GetStringAsync($"api/users/{userId}");

            if (backEndUserInfo == null) return false;

            return true;
        }

        catch (Exception e)
        {
            return false;
        }
    }

    public static void SaveEnvironment(EnvironmentSettings environmentSettings)
    {
        using var writer = new StreamWriter("environment.env");
        writer.Write(JsonSerializer.Serialize(environmentSettings));
    }
}