using System.Linq;
using UnityEngine;

[System.Serializable]
public class TextContainer
{
    [SerializeField] string _defaultText;
    [SerializeField] TextForLanguage[] _texts;

    public string GetText(EnumLanguage language)
    {
        var textForLanguage = _texts.FirstOrDefault(t => t.Language == language);

        return textForLanguage?.Text ?? _defaultText;
    }

    [System.Serializable]
    private record TextForLanguage
    {
        [SerializeField] EnumLanguage _language;
        [SerializeField] string _text;

        public EnumLanguage Language { get => _language; }
        public string Text { get => _text; }
    }

}
