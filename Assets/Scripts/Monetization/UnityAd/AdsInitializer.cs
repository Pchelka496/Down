using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : IUnityAdsInitializationListener
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    const string IOS_GAME_ID = "5755672";
    const string ANDROID_GAME_ID = "5755673";

    readonly bool _testMode = true;

    string _gameId;

    public void InitializeAds()
    {
#if UNITY_IOS
            _gameId =IOS_GAME_ID;
#elif UNITY_ANDROID
        _gameId = ANDROID_GAME_ID;
#elif UNITY_EDITOR
            _gameId = ANDROID_GAME_ID; //Only for testing the functionality in the Editor
#endif
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(_gameId, _testMode, this);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error} - {message}");
    }
}
