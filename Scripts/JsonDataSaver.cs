using System.IO;
using UnityEngine;

public class JsonDataSaver : MonoBehaviour
{
    private static string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName + ".json");
    }

    public static void Save<T>(T data, string fileName)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetFilePath(fileName), json);
        Debug.Log($"[JsonDataSaver] Сохранено: {GetFilePath(fileName)}");
    }

    public static T Load<T>(string fileName)
    {
        string path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        else
        {
            Debug.LogWarning($"[JsonDataSaver] Файл не найден: {path}");
            return default;
        }
    }

    public static bool Exists(string fileName)
    {
        return File.Exists(GetFilePath(fileName));
    }

    public static void Delete(string fileName)
    {
        string path = GetFilePath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[JsonDataSaver] Удалено: {path}");
        }
    }
}
