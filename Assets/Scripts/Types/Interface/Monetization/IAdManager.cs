public interface IAdManager
{
    public void LoadRewardedAd(System.Action OnAdsLoaded, System.Action OnFailedToLoad);
    public void LoadInterstitialAd(System.Action OnAdsLoaded, System.Action OnFailedToLoad);
    public void LoadBannerAd(System.Action OnAdsLoaded, System.Action OnFailedToLoad);

    public void ShowRewardedAd(System.Action OnShowComplete, System.Action OnShowFailure);
    public void ShowInterstitialAd();
    public void ShowBannerAd();
    public void HideBannerAd();

    public void DisposeRewardedAd();
    public void DisposeInterstitialAd();
    public void DisposeBannerAd();
}
