using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HxLocal;

public class LanguageChangedEventArgs : EventArgs
{
    public string PrevLanguage { get; }
    public string CurrLanguage { get; }

    public LanguageChangedEventArgs(string prevLanguage, string currLanguage)
    {
        PrevLanguage = prevLanguage;
        CurrLanguage = currLanguage;
    }
}

public static class Localisation
{
    private static string[] _data;
    public static string CurrentLanguage { get; private set; }
    public static string RootDirectory = "Lingos";
    public static string CurrentFile { get; private set; }
    public static int Count => _data.Length;
    
    public static event EventHandler<LanguageChangedEventArgs> OnLanguageChange;

    public static void SetLanguage(string langCode)
    {
        if (langCode == CurrentLanguage)
        {
            return;
        }
        var prevLangCode = CurrentLanguage;
        CurrentLanguage = langCode;
        OnLanguageChange?.Invoke(null, new LanguageChangedEventArgs(prevLangCode, langCode));
    }

    public static void Reload()
    {
        LoadData(CurrentFile);
    }
    
    public static void LoadData(string filePath)
    {
        var languageFolder = Path.Combine(RootDirectory, CurrentLanguage);
        
        var combinedPath = Path.Combine(languageFolder, filePath);
        
        if (!File.Exists(combinedPath))
        {
            throw new FileNotFoundException(combinedPath);
        }
        
        try
        {
            var data = File.ReadAllLines(combinedPath);
            _data = data;
            CurrentFile = filePath;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public static string GetLine(int line)
    {
        if (_data == null)
        {
            //ToDo: error message
            throw new Exception("");
        }
        if (_data.Length <= line || line < 0)
        {
            //ToDo: error message
            throw new Exception("");
        }
        return _data[line];
    }
   
    /*
    public static bool Reload()
    {
        var filePath = Path.Combine(FilePath, $"{CurrentLanguage}.json");

        if (!File.Exists(filePath))
        {
            throw new LanguageNotFoundException(CurrentLanguage);
        }

        try
        {
            var jsonData = File.ReadAllText(filePath);
            _data = JObject.Parse(jsonData);
        }
        catch (JsonReaderException ex)
        {
            throw new JsonParseException(ex.Message);
        }
        catch (IOException ex)
        {
            throw new IOOperationException(ex.Message);
        }
        return true;
    }
    */

    /*
    public static string GetString(string key)
    {
        if (_data == null)
        {
            throw new LocalisationException("Language not loaded.");
        }

        string[] keys = key.Split('.');
        JObject temp = _data;
        foreach (var k in keys)
        {
            if (temp.ContainsKey(k))
            {
                JToken value;
                if (temp.TryGetValue(k, out value))
                {
                    if (value.Type == JTokenType.Object)
                    {
                        temp = (JObject)value;
                    }
                    else
                    {
                        return value.ToString();
                    }
                }
            }
            else
            {
                return key;
            }
        }
        return key;
        //if (_translations != null && _translations.ContainsKey(key))
        //{
        //    return _translations[key];
        //}
        //return key;  // Wenn nicht gefunden, gibt den Schlüssel zurück.
    }

    public static string GetString(string key, params object[] args)
    {
        string localizedString = GetString(key);

        if (args != null && args.Length > 0)
        {
            return string.Format(localizedString, args);
        }

        return localizedString;
    }

    public static string[] ListAvailableLanguages()
    {
        // Listen Sie die Namen der JSON-Dateien im Verzeichnis auf.
        var availableLanguages = Directory.GetFiles(FilePath + "/", "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();
        return availableLanguages!;
    }
*/
}