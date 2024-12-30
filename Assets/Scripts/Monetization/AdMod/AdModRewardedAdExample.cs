using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class AdModRewardedAdExample
{
    const string UNIT_ID = "ca-app-pub-1335302148584588/1857030279";//"ca-app-pub-3940256099942544/5224354917";

    RewardedAd _rewardedAd;

    Action _onAdLoaded;
    Action _onAdFailedToLoad;

    Action _onAdFailedToShow;
    Action _onUserEarnedReward;

    public void LoadRewardedAd(Action OnAdsLoaded, Action OnFailedToLoad)
    {
        _rewardedAd?.Destroy();

        _onAdLoaded = OnAdsLoaded;
        _onAdFailedToLoad = OnFailedToLoad;

        _rewardedAd = new RewardedAd(UNIT_ID);

        _rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        _rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        _rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        _rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        _rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        _rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        AdRequest request = new AdRequest.Builder().Build();
        _rewardedAd.LoadAd(request);
    }

    public void ShowRewardedAd(Action OnShowComplete, Action OnShowFailure)
    {
        _onUserEarnedReward = OnShowComplete;
        _onAdFailedToShow = OnShowFailure;

        if (_rewardedAd != null && _rewardedAd.IsLoaded())
        {
            _rewardedAd.Show();
        }
        else
        {
            Debug.LogError("Rewarded Ad is not loaded yet.");
            OnShowFailure?.Invoke();
        }
    }

    public void DestroyRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
    }

    private void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("Rewarded Ad Loaded.");

        _onAdLoaded?.Invoke();
    }

    private void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.LogError($"Rewarded Ad Failed to Load: {args.LoadAdError.GetMessage()}");

        _onAdFailedToLoad?.Invoke();
    }

    private void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        Debug.Log("Rewarded Ad Opening.");
    }

    private void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        Debug.LogError($"Rewarded Ad Failed to Show: {args.AdError.GetMessage()}");

        _onAdFailedToShow?.Invoke();
    }

    private void HandleUserEarnedReward(object sender, Reward args)
    {
        Debug.Log($"User Earned Reward: {args.Amount} {args.Type}");

        _onUserEarnedReward?.Invoke();
    }

    private void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        Debug.Log("Rewarded Ad Closed.");
    }
}
