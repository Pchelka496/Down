using System;
using System.Threading;
using UnityEngine;

public class UnityAdManager : IAdManager
{
    const string BANNER_AD_UNIT_ID = "Banner_Android";
    const string REWARDED_AD_UNIT_ID = "Rewarded_Android";
    const string INTERSTITIAL_AD_UNIT_ID = "Interstitial_Android";

    readonly AdsInitializer _adsInitializer = new();
    readonly BannerAdExample _bannerAdExample;
    readonly RewardedAdsExample _rewardedAdsExample;
    readonly InterstitialAdExample _interstitialAdExample;

    readonly SynchronizationContext _mainThreadContext;

    public UnityAdManager()
    {
        _mainThreadContext = SynchronizationContext.Current;
        if (_mainThreadContext == null)
        {
            Debug.LogWarning("SynchronizationContext is null. Ensure this is initialized on the main thread.");
        }

        _adsInitializer.InitializeAds();
        _bannerAdExample = new(BANNER_AD_UNIT_ID);
        _rewardedAdsExample = new(REWARDED_AD_UNIT_ID);
        _interstitialAdExample = new(INTERSTITIAL_AD_UNIT_ID);
    }

    void IAdManager.LoadBannerAd(Action OnAdsLoaded, Action OnFailedToLoad)
        => RunOnMainThread(() => _bannerAdExample.LoadBanner(OnAdsLoaded, OnFailedToLoad));

    void IAdManager.LoadInterstitialAd(Action OnAdsLoaded, Action OnFailedToLoad)
        => RunOnMainThread(() => _interstitialAdExample.LoadAd(OnAdsLoaded, OnFailedToLoad));

    void IAdManager.LoadRewardedAd(Action OnAdsLoaded, Action OnFailedToLoad)
        => RunOnMainThread(() => _rewardedAdsExample.LoadAd(OnAdsLoaded, OnFailedToLoad));

    void IAdManager.HideBannerAd()
        => RunOnMainThread(() => _bannerAdExample.HideBannerAd());

    void IAdManager.ShowBannerAd()
        => RunOnMainThread(() => _bannerAdExample.ShowBannerAd());

    void IAdManager.ShowInterstitialAd()
        => RunOnMainThread(() => _interstitialAdExample.ShowAd());

    void IAdManager.ShowRewardedAd(Action OnShowComplete, Action OnShowFailure)
        => RunOnMainThread(() =>
        {
            _rewardedAdsExample.ShowAd(onAdCompleted: OnShowComplete, onAdShowFailed: OnShowFailure);
        });

    private void RunOnMainThread(Action action)
    {
        if (_mainThreadContext == null)
        {
            Debug.LogError("SynchronizationContext is not set. RunOnMainThread cannot execute.");
            return;
        }

        _mainThreadContext.Post(_ => action(), null);
    }
}
