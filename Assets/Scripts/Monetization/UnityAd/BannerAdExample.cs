using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAdExample
{
    const int MAX_ATTEMPT_RATE = 10;

    readonly string _adUnitId = null;
    readonly BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    System.Action _onAdFailedToLoad;
    System.Action _onBannerLoaded;

    public BannerAdExample(string adUnitId)
    {
        _adUnitId = adUnitId;
        Advertisement.Banner.SetPosition(_bannerPosition);
    }

    public void LoadBanner(System.Action OnAdsLoaded, System.Action OnFailedToLoad)
    {
        _onAdFailedToLoad = OnFailedToLoad;
        _onBannerLoaded = OnAdsLoaded;

        var options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId, options);
    }

    void OnBannerLoaded()
    {
        _onBannerLoaded?.Invoke();
        _onBannerLoaded = null;
    }

    void OnBannerError(string message)
    {
        _onAdFailedToLoad?.Invoke();
        _onAdFailedToLoad = null;

        Debug.Log($"Banner Error: {message}");
    }

    public void ShowBannerAd()
    {
        var options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(_adUnitId, options);
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
    }

    void OnBannerClicked() { }
    void OnBannerShown() { }
    void OnBannerHidden() { }
}
