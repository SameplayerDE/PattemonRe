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
    
#if CLIENT_LANGUAGE_de
    private const string _currentLanguage = "de";
#elif CLIENT_LANGUAGE_fr
    private const string _currentLanguage = "fr";
#endif
    
    public static bool Load(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var loaded = LoadJsonOrTextFile(assembly, _currentLanguage, fileName) &&
                     LoadJsonOrTextFile(assembly, _defaultLanguage, fileName);
        return loaded;
    }

    private static bool LoadJsonOrTextFile(Assembly assembly, string language, string fileName)
    {
        string resourcePathJson = $"{_baseNamespace}.{language}.{fileName}.json";
        string resourcePathTxt = $"{_baseNamespace}.{language}.{fileName}.txt";
        
        var key = $"{language}.{fileName}";
        
        Stream? jsonStream = assembly.GetManifestResourceStream(resourcePathJson);
        if (jsonStream is not null)
        {
            using var reader = new StreamReader(jsonStream);
            string json = reader.ReadToEnd();
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            _jsonData[key] = ConvertToDictionary(data);
            return true;
        }

        // Text laden
        Stream? textStream = assembly.GetManifestResourceStream(resourcePathTxt);
        if (textStream is not null)
        {
            using var reader = new StreamReader(textStream);
            _textData[key] = reader.ReadToEnd().Split(Environment.NewLine);
            return true;
        }

        return false;
    }

    public static void Unload(string fileName)
    {
        _jsonData.Remove($"{_currentLanguage}.{fileName}");
        _jsonData.Remove($"{_defaultLanguage}.{fileName}");
        _textData.Remove($"{_currentLanguage}.{fileName}");
        _textData.Remove($"{_defaultLanguage}.{fileName}");
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
        string keyPrimary = $"{_currentLanguage}.{fileName}";
        string keyFallback = $"{_defaultLanguage}.{fileName}";

        if (_textData.TryGetValue(keyPrimary, out var lines) && lineNumber >= 0 && lineNumber < lines.Length)
            return lines[lineNumber];

        if (_textData.TryGetValue(keyFallback, out var fallbackLines) && lineNumber >= 0 && lineNumber < fallbackLines.Length)
            return fallbackLines[lineNumber];

        return $"[Line {lineNumber} not found]";
    }

    public static string Get(string fileName, string keyPath)
    {
        string keyPrimary = $"{_currentLanguage}.{fileName}";
        string keyFallback = $"{_defaultLanguage}.{fileName}";

        if (_jsonData.TryGetValue(keyPrimary, out var json) && TryGetNestedValue(json, keyPath, out var value))
        {
            return value;
        }
        
        if (_jsonData.TryGetValue(keyFallback, out var fallbackJson) && TryGetNestedValue(fallbackJson, keyPath, out var fallbackValue))
        {
            Console.WriteLine($"[INFO] '{keyPath}' nicht in '{_currentLanguage}' gefunden. Fallback auf '{_defaultLanguage}'.");
            return fallbackValue;
        }
        
        Console.WriteLine($"[WARNUNG] '{keyPath}' nicht in '{_currentLanguage}' oder '{_defaultLanguage}' gefunden. Rückgabe des Keys als Fallback.");
        return keyPath;
    }

    private static bool TryGetNestedValue(Dictionary<string, object> json, string keyPath, out string value)
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
                value = keyPath;
                return false;
            }
        }

        value = current.ToString();
        return true;
    }
}
