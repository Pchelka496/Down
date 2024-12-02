using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    [SerializeField] int _points;
    [SerializeField] List<CustomKeyValuePair<string, bool>> _skinOpenStatus = new();

    public int Points { get => _points; set => _points = value; }

    public Dictionary<string, bool> SkinOpenStatus
    {
        get
        {
            var dict = new Dictionary<string, bool>();
            foreach (var pair in _skinOpenStatus)
            {
                if (string.IsNullOrEmpty(pair.Key))
                {
                    Debug.LogWarning("Encountered a null or empty key in _skinOpenStatus!");
                    continue;
                }

                dict.Add(pair.Key, pair.Value);
            }
            return dict;
        }
        set
        {
            _skinOpenStatus.Clear();
            Debug.Log(value.Count);
            foreach (var kvp in value)
            {
                Debug.Log(kvp.Value);
                _skinOpenStatus.Add(new CustomKeyValuePair<string, bool>(kvp.Key, kvp.Value));
            }
        }
    }

}

