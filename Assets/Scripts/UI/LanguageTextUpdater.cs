using TMPro;
using UnityEngine;
using Zenject;

public class LanguageTextUpdater : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _textMeshPro; 
    [SerializeField] TextContainer _textContainer; 

    EnumLanguage _currentLanguage; 

    [Inject]
    public void Construct(EnumLanguage language)
    {
        _currentLanguage = language; 
    }

    private void Start()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        string newText = _textContainer.GetText(_currentLanguage);

        if (_textMeshPro != null)
        {
            _textMeshPro.text = newText;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI is not assigned.");
        }
    }
}
