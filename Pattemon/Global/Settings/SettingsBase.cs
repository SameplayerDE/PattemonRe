using System;
using System.IO;
using System.Text.Json;

namespace Pattemon.Global.Settings;

public class SettingsBase<T> where T : class, new()
{
    private readonly string _filePath;

    public SettingsBase(string filePath)
    {
        _filePath = filePath;
        EnsureDirectoryExists();
    }

    // Prüft und erstellt das Verzeichnis, falls nötig
    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    // Daten laden
    public T Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<T>(json);
                if (data != null)
                {
                    return data;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Datei {_filePath}: {ex.Message}");
        }
        return new T(); // Standardwert bei Fehler
    }

    // Daten speichern
    public void Save(T data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern der Datei {_filePath}: {ex.Message}");
        }
    }
}