public interface ILanguageContainer
{
    public EnumLanguage Language { get; }

    public void SubscribeToChangeLanguageEvent(System.Action<EnumLanguage> action);
    public void UnsubscribeFromChangeLanguageEvent(System.Action<EnumLanguage> action);
}
