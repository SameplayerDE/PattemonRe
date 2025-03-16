using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pattemon.Engine;

public static class LanguageCore
{
    private static readonly string _baseNamespace = "Pattemon.Content.Languages";

    private static readonly Dictionary<string, Dictionary<string, object>> _jsonData = new();
    private static readonly Dictionary<string, string[]> _textData = new();
    private static readonly string _defaultLanguage = "en";
    
    public static void Load(string fileName)
    {
        string resourcePathJson = $"{_baseNamespace}.{_defaultLanguage}.{fileName}.json";
        string resourcePathTxt = $"{_baseNamespace}.{_defaultLanguage}.{fileName}.txt";

        var assembly = Assembly.GetExecutingAssembly();

        Stream? jsonStream = assembly.GetManifestResourceStream(resourcePathJson);
        if (jsonStream is not null)
        {
            using var reader = new StreamReader(jsonStream);
            string json = reader.ReadToEnd();
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            _jsonData[fileName] = ConvertToDictionary(data);
            return;
        }
        
        Stream? textStream = assembly.GetManifestResourceStream(resourcePathTxt);
        if (textStream is not null)
        {
            using var reader = new StreamReader(textStream);
            _textData[fileName] = reader.ReadToEnd().Split(Environment.NewLine);
            return;
        }
        
        throw new FileNotFoundException($"The file {fileName} could not be found.");
    }

    public static void Unload(string fileName)
    {
        _jsonData.Remove(fileName);
        _textData.Remove(fileName);
    }
    
    private static Dictionary<string, object> ConvertToDictionary(Dictionary<string, object> source)
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in source)
        {
            if (kvp.Value is JObject jObject)
            {
                result[kvp.Key] = jObject.ToObject<Dictionary<string, object>>();
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }
    
    public static string GetLine(string fileName, int lineNumber)
    {
        if (_textData.TryGetValue(fileName, out var lines) && lineNumber >= 0 && lineNumber < lines.Length)
            return lines[lineNumber];

        return $"[Line {lineNumber} not found]";
    }
    
    public static string Get(string fileName, string keyPath)
    {
        if (_jsonData.TryGetValue(fileName, out var json))
        {
            var keys = keyPath.Split('.');
            object current = json;

            foreach (var key in keys)
            {
                if (current is Dictionary<string, object> dict && dict.TryGetValue(key, out var next))
                {
                    current = next;
                }
                else
                {
                    return keyPath; // Falls der Key nicht existiert, gib den Key selbst zurück.
                }
            }

            return current.ToString();
        }

        return keyPath; // Falls die Datei nicht geladen wurde, gib den Key selbst zurück.
    }
}