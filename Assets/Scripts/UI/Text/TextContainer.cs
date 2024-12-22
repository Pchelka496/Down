using System.Linq;
using UnityEngine;

[System.Serializable]
public class TextContainer
{
    [SerializeField, TextArea(3, 10)] string _defaultText;
    [SerializeField] TextForLanguage[] _texts;

    public string GetText(EnumLanguage language)
    {
#if UNITY_EDITOR

        CheckForDuplicateLanguages();
#endif
        var textForLanguage = _texts.FirstOrDefault(t => t.Language == language);

        return textForLanguage?.Text ?? _defaultText;
    }

#if UNITY_EDITOR
    private void CheckForDuplicateLanguages()
    {
        var duplicates = _texts
            .GroupBy(t => t.Language)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            Debug.LogWarning("Duplicate EnumLanguage values found in TextContainer! Languages: "
                + string.Join(", ", duplicates.Select(lang => lang.ToString()))
                + ". Please ensure each language appears only once in the list.");
        }
    }
#endif

    [System.Serializable]
    private record TextForLanguage
    {
        [SerializeField] EnumLanguage _language;
        [SerializeField, TextArea(5, 15)] string _text;

        public EnumLanguage Language => _language;
        public string Text => _text;
    }
}
