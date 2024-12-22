using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class RewardedAdsExample : IUnityAdsLoadListener, IUnityAdsShowListener
{
    readonly string _adUnitId;

    Action _onAdFailedToLoad;
    Action _onAdLoaded;

    Action _onAdCompleted;
    Action _onAdShowFailed;

    public RewardedAdsExample(string adUnitId)
    {
        _adUnitId = adUnitId;
    }

    public void LoadAd(Action OnAdsLoaded, Action onAdFailedToLoad)
    {
        _onAdFailedToLoad = onAdFailedToLoad;
        _onAdLoaded = OnAdsLoaded;

        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd(Action onAdCompleted, Action onAdShowFailed)
    {
        _onAdCompleted = onAdCompleted;
        _onAdShowFailed = onAdShowFailed;

        Advertisement.Show(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        _onAdLoaded?.Invoke();
        _onAdLoaded = null;
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        _onAdFailedToLoad?.Invoke();
        _onAdFailedToLoad = null;

        Debug.Log($"Error loading Ad Unit {adUnitId}: {error} - {message}");
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            _onAdCompleted?.Invoke();
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        _onAdShowFailed?.Invoke();
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
}
