using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAdExample : IUnityAdsLoadListener, IUnityAdsShowListener
{
    readonly string _adUnitId;

    System.Action _onAdFailedToLoad;
    System.Action _onAdLoaded;

    public InterstitialAdExample(string adUnitId)
    {
        _adUnitId = adUnitId;
    }

    public void LoadAd(System.Action onAdLoaded, System.Action OnFailedToLoad)
    {
        _onAdFailedToLoad = OnFailedToLoad;
        _onAdLoaded = onAdLoaded;

        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd()
    {
        Advertisement.Show(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        _onAdLoaded?.Invoke();
        _onAdLoaded = null;
    }

    public void OnUnityAdsFailedToLoad(string _adUnitId, UnityAdsLoadError error, string message)
    {
        _onAdFailedToLoad?.Invoke();

        Debug.Log($"Error loading Ad Unit: {_adUnitId} - {error} - {message}");
    }

    public void OnUnityAdsShowFailure(string _adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {_adUnitId}: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string _adUnitId) { }
    public void OnUnityAdsShowClick(string _adUnitId) { }
    public void OnUnityAdsShowComplete(string _adUnitId, UnityAdsShowCompletionState showCompletionState) { }
}
