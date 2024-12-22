public interface IAudioSettingContainer
{
    bool SoundEnabledFlag { get; }
    float MusicVolume { get; }
    float MaxVolume { get; }

    void SubscribeToChangeSoundEnableEvent(System.Action<bool> action);
    void UnsubscribeFromChangeSoundEnableEvent(System.Action<bool> action);

    void SubscribeToChangeMaxVolumeEvent(System.Action<float> action);
    void UnsubscribeFromChangeMaxVolumeEvent(System.Action<float> action);

    void SubscribeToChangeMusicVolumeEvent(System.Action<float> action);
    void UnsubscribeFromChangeMusicVolumeEvent(System.Action<float> action);
}