using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdModManager : IAdManager
{
    readonly AdModRewardedAdExample _rewardedAdExample;

    public AdModManager()
    {
        _rewardedAdExample = new();

        MobileAds.Initialize((initializationStatus) => { });
        SetRequestConfiguration();
    }

    private void SetRequestConfiguration()
    {
        var testDevice = new List<string>() { "ffec3bc3-62cc-4838-a7e5-6cabd2aae7cf" };

        var configuration = new RequestConfiguration.Builder().SetTestDeviceIds(testDevice).build();

        MobileAds.SetRequestConfiguration(configuration);
    }


    void IAdManager.LoadBannerAd(Action OnAdsLoaded, Action OnFailedToLoad) => Debug.Log("Banner Ad - Not done(");
    void IAdManager.LoadInterstitialAd(Action OnAdsLoaded, Action OnFailedToLoad) => Debug.Log("Interstitial Ad - Not done(");
    void IAdManager.LoadRewardedAd(Action OnAdsLoaded, Action OnFailedToLoad)
    {
        _rewardedAdExample.LoadRewardedAd(OnAdsLoaded, OnFailedToLoad);
    }

    void IAdManager.ShowBannerAd() => Debug.Log("Banner Ad - Not done(");

    void IAdManager.ShowInterstitialAd() => Debug.Log("Interstitial Ad - Not done(");

    void IAdManager.ShowRewardedAd(Action OnShowComplete, Action OnShowFailure)
    {
        _rewardedAdExample.ShowRewardedAd(OnShowComplete, OnShowFailure);
    }

    void IAdManager.HideBannerAd() => Debug.Log("Banner Ad - Not done(");

    void IAdManager.DisposeRewardedAd()
    {
        _rewardedAdExample.DestroyRewardedAd();
    }
    void IAdManager.DisposeInterstitialAd() => Debug.Log("Interstitial Ad - Not done(");
    void IAdManager.DisposeBannerAd() => Debug.Log("Banner Ad - Not done(");
}
