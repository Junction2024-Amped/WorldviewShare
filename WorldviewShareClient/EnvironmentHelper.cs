using System;
using System.IO;
using System.Text.Json;
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
                _settings = JsonSerializer.Deserialize<EnvironmentSettings>(json);
            }
        }

        try
        {
            var backEndUserInfo = HttpClientFactory.GetClient()
                .GetStringAsync($"api/users/{_settings.Id}");

            if (backEndUserInfo.Result == null)
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
        }
        catch (Exception e)
        {
            File.Delete("environment.env");
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

        return _settings;
    }

    public static void SaveEnvironment(EnvironmentSettings environmentSettings)
    {
        using var writer = new StreamWriter("environment.env");
        writer.Write(JsonSerializer.Serialize(environmentSettings));
    }
}