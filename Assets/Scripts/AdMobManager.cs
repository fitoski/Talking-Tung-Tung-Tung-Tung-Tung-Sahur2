using UnityEngine;
using GoogleMobileAds.Api;
using System.Collections.Generic;

public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance { get; private set; }
    public bool AdsDisabled { get; private set; }

    [Header("App IDs")]
    public string androidAppId = "ca-app-pub-5692960194342108~5036898749";
    public string iosAppId = "ca-app-pub-5692960194342108~4292106369";

#if UNITY_IOS
    const string BannerId     = "ca-app-pub-5692960194342108/8122414432";
    const string RewardedId   = "ca-app-pub-5692960194342108/2695953660";
    const string TransitionId = "ca-app-pub-5692960194342108/5915053493";
#else   // Android
    const string BannerId = "ca-app-pub-5692960194342108/5863713491";
    const string RewardedId = "ca-app-pub-5692960194342108/9125738440";
    const string TransitionId = "ca-app-pub-5692960194342108/8372442867";
#endif

    BannerView bannerView;
    RewardedAd rewardedAd;
    RewardedAd transitionAd;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);

        AdsDisabled = PlayerPrefs.GetInt("IAP_UnlimitedFeeds", 0) == 1;

        var cfg = new RequestConfiguration
        {
            TagForChildDirectedTreatment =
                AgeGateManager.IsChild ? TagForChildDirectedTreatment.True : TagForChildDirectedTreatment.False,
            TagForUnderAgeOfConsent =
                AgeGateManager.Underage15 ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False
        };
        MobileAds.SetRequestConfiguration(cfg);
    }

    void Start()
    {
        MobileAds.Initialize(_ =>
        {
            Debug.Log("▶️ AdMob SDK initialized");
            if (!AdsDisabled)
            {
                RequestBanner();
                RequestRewarded();
                RequestTransition();
            }
        });
    }

    public void ConfigureForAge(bool isChild, bool under15)
    {
        var testDevices = new List<string>
        {
            AdRequest.TestDeviceSimulator
        };

        var cfg = new RequestConfiguration
        {
            //TestDeviceIds = testDevices,
            TagForChildDirectedTreatment = isChild ? TagForChildDirectedTreatment.True : TagForChildDirectedTreatment.False,
            TagForUnderAgeOfConsent = under15 ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False
        };
        MobileAds.SetRequestConfiguration(cfg);
        Debug.Log($"▶️ COPPA flags updated: child={isChild}, under15={under15}");
    }

    public void DisableAds()
    {
        AdsDisabled = true;
        bannerView?.Destroy();
        rewardedAd = null;
        transitionAd = null;
        Debug.Log("🚫 Ads disabled via IAP");
    }

    void RequestBanner()
    {
        bannerView?.Destroy();
        bannerView = new BannerView(BannerId, AdSize.Banner, AdPosition.Top);
        bannerView.OnBannerAdLoaded += () => Debug.Log("✅ Banner loaded");
        bannerView.OnBannerAdLoadFailed += e => Debug.LogError($"❌ Banner failed: {e}");
        bannerView.LoadAd(new AdRequest());
    }

    void RequestRewarded()
    {
        RewardedAd.Load(
            RewardedId,
            new AdRequest(),
            (ad, err) =>
            {
                if (err != null || ad == null)
                {
                    Debug.LogError($"⚠️ Rewarded load failed: {err}");
                    return;
                }
                rewardedAd = ad;
                rewardedAd.OnAdFullScreenContentClosed += () => RequestRewarded();
                Debug.Log("✅ Rewarded ad loaded");
            });
    }

    void RequestTransition()
    {
        RewardedAd.Load(
            TransitionId,
            new AdRequest(),
            (ad, err) =>
            {
                if (err != null || ad == null)
                {
                    Debug.LogError($"⚠️ Transition load failed: {err}");
                    return;
                }
                transitionAd = ad;
                transitionAd.OnAdFullScreenContentClosed += () => RequestTransition();
                Debug.Log("✅ Transition ad loaded");
            });
    }

    public void ShowRewardedAd(System.Action onEarned = null)
    {
        if (AdsDisabled) { onEarned?.Invoke(); return; }

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(_ =>
            {
                Debug.Log("🎁 Reward granted");
                onEarned?.Invoke();
            });
        }
        else Debug.LogWarning("⚠️ Rewarded ad not ready");
    }

    public void ShowTransitionAd()
    {
        if (AdsDisabled) return;
        if (transitionAd != null && transitionAd.CanShowAd())
            transitionAd.Show(null);
        else
            Debug.LogWarning("⚠️ Transition ad not ready");
    }

    void OnDestroy() => bannerView?.Destroy();
}