using System;
using System.IO;
using UnityEngine;

public class SaveSystem
{
    private readonly string _filePath;

    public SaveSystem()
    {
        _filePath = Application.persistentDataPath + "Save.json";
        Debug.Log(_filePath);
    }

    public void Save(SaveData data)
    {
        var json = JsonUtility.ToJson(data);

        using (var writer = new StreamWriter(_filePath))
        {
            writer.WriteLine(json);
        }
    }

    public SaveData Load()
    {
        try
        {
            string json = "";

            if (!File.Exists(_filePath))
            {
                // ���� �� ������, ���������� ��������� ��������
                Debug.LogWarning("Save file not found, returning default SaveData.");
                return new();
            }

            using (var reader = new StreamReader(_filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    json += line;
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                // ���� ���� ������, ���������� ��������� ��������
                Debug.LogWarning("Save file is empty, returning default SaveData.");
                return new();
            }

            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (FileNotFoundException ex)
        {
            // ��������� ������ ���� ���� �� ������
            Debug.LogError($"Save file not found: {_filePath}. Returning default SaveData. Exception: {ex.Message}");
            return new();
        }
        catch (Exception ex)
        {
            // ��������� ����� ������ ������
            Debug.LogError($"Error loading save file: {ex.Message}. Returning default SaveData.");
            return new();
        }
    }

}
