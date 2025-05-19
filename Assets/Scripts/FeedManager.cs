using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;

public class FeedManager : MonoBehaviour
{
    public static event Action onInteraction;
    public static FeedManager Instance { get; private set; }

    public enum FoodType { Chili, Pide, Banana, Gum, Apple, Cola }

    [Header("Food GameObjects")]
    public GameObject chiliObj;
    public GameObject pideObj;
    public GameObject bananaObj;
    public GameObject gumPacketObj;
    public GameObject appleObj;
    public GameObject colaObj;

    [Header("UI")]
    public Button feedToggleButton;
    public GameObject feedOptionsPanel;
    public Button chiliButton, pideButton, bananaButton, gumButton, appleButton, colaButton;
    public Button watchAdButton;
    public Button dailyFreeButton;
    public TMP_Text usesText;

    [Header("Feeding")]
    [Tooltip("İlk başta kaç ücretsiz kullanım?")]
    public int initialFreeUses = 5;

    [Header("Rewarded Ads")]
    public string rewardedAdUnitId = "ca-app-pub-5692960194342108/9125738440"; 
    public string transitionAdUnitId = "ca-app-pub-5692960194342108/8372442867"; 
    public string dailyAdUnitId = "ca-app-pub-5692960194342108/8372442867"; 

    private int usesRemaining;
    private int interactionCount;

    private RewardedAd rewardAd;
    private RewardedAd transitionAd;
    private RewardedAd dailyAd;

    private Animator tomAnimator;

    bool unlimited = false;

    void Awake()
    {
        usesRemaining = PlayerPrefs.GetInt("FeedUsesRemaining", initialFreeUses);
        interactionCount = PlayerPrefs.GetInt("InteractionCount", 0);
    }

    void Start()
    {
        //PlayerPrefs.DeleteAll();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        tomAnimator = FindFirstObjectByType<TomInteraction>()?.GetComponent<Animator>();

        feedToggleButton.onClick.AddListener(ToggleFeedPanel);
        chiliButton.onClick.AddListener(() => AttemptFeed(FoodType.Chili));
        pideButton.onClick.AddListener(() => AttemptFeed(FoodType.Pide));
        bananaButton.onClick.AddListener(() => AttemptFeed(FoodType.Banana));
        gumButton.onClick.AddListener(() => AttemptFeed(FoodType.Gum));
        appleButton.onClick.AddListener(() => AttemptFeed(FoodType.Apple));
        colaButton.onClick.AddListener(() => AttemptFeed(FoodType.Cola));

        watchAdButton.onClick.AddListener(() =>
        {
            AdMobManager.Instance.ShowRewardedAd(() =>
            {
                usesRemaining++;
                SaveUses();
                UpdateUsesUI();
            });
        });

        dailyFreeButton.onClick.AddListener(ShowDailyAd);

        feedOptionsPanel.SetActive(false);
        UpdateUsesUI();

        LoadRewardedAd();
        LoadTransitionAd();
        LoadDailyAd();

        dailyFreeButton.interactable = !HasClaimedToday();

        HideAllFoods();
        NotificationManager.Instance.ScheduleHungerNotification(4);
    }

    public void OnEatAnimationEnd()
    {
        HideAllFoods();
        CharacterBusy.Unlock();

        TextureFader textureFader = tomAnimator.GetComponent<TextureFader>();
        if (textureFader != null)
        {
            textureFader.EndBananaFade();
        }
        NotificationManager.Instance.ScheduleHungerNotification(4);
    }

    void HideAllFoods()
    {
        chiliObj.SetActive(false);
        pideObj.SetActive(false);
        bananaObj.SetActive(false);
        gumPacketObj.SetActive(false);
        appleObj.SetActive(false);
        colaObj.SetActive(false);
    }

    // ---------- Daily +5 Ad ----------
    void LoadDailyAd()
    {
        var req = new AdRequest();
        RewardedAd.Load(dailyAdUnitId, req, (RewardedAd ad, LoadAdError err) =>
        {
            if (err != null || ad == null) { Debug.LogError(err); return; }
            dailyAd = ad;
            dailyAd.OnAdFullScreenContentClosed += () => LoadDailyAd();
            dailyAd.OnAdFullScreenContentFailed += (AdError e) => { Debug.LogError(e); dailyAd = null; };
        });
    }

    bool HasClaimedToday()
    {
        string last = PlayerPrefs.GetString("LastDailyDate", "");
        return last == DateTime.Now.ToString("yyyyMMdd");
    }

    void ShowDailyAd()
    {
        if (dailyAd != null && !HasClaimedToday())
        {
            dailyAd.Show((Reward r) =>
            {
                usesRemaining += 10;
                SaveUses();
                UpdateUsesUI();

                PlayerPrefs.SetString("LastDailyDate", DateTime.Now.ToString("yyyyMMdd"));
                PlayerPrefs.Save();
                dailyFreeButton.interactable = false;

                Debug.Log($"🎉 Günlük ödül: +10 hak alındı");
            });
        }
        else Debug.LogWarning("🎉 Günlük reklam hazır değil veya zaten alındı.");
    }

    // ---------- +1 Hak Ad ----------
    void LoadRewardedAd()
    {
        var req = new AdRequest();
        RewardedAd.Load(rewardedAdUnitId, req, (RewardedAd ad, LoadAdError err) =>
        {
            if (err != null || ad == null) { Debug.LogError(err); return; }
            rewardAd = ad;
            rewardAd.OnAdFullScreenContentClosed += () => LoadRewardedAd();
            rewardAd.OnAdFullScreenContentFailed += (AdError e) => { Debug.LogError(e); rewardAd = null; };
        });
    }


    // ---------- Transition Ad ----------
    void LoadTransitionAd()
    {
        var req = new AdRequest();
        RewardedAd.Load(transitionAdUnitId, req, (RewardedAd ad, LoadAdError err) =>
        {
            if (err != null || ad == null) { Debug.LogError(err); return; }
            transitionAd = ad;
            transitionAd.OnAdFullScreenContentClosed += () => LoadTransitionAd();
            transitionAd.OnAdFullScreenContentFailed += (AdError e) => { Debug.LogError(e); transitionAd = null; };
        });
    }

    public void RecordInteraction()
    {
        onInteraction?.Invoke();

        interactionCount++;
        if (interactionCount >= 5)
        {
            interactionCount = 0;
            AdMobManager.Instance.ShowTransitionAd();
        }

        PlayerPrefs.SetInt("InteractionCount", interactionCount);
        PlayerPrefs.Save();
    }

    // ---------- Feeding ----------
    void AttemptFeed(FoodType food)
    {
        onInteraction?.Invoke();
        if (!CharacterBusy.TryLock()) return;

        if (usesRemaining > 0)
        {
            usesRemaining--;
            SaveUses();
            UpdateUsesUI();
            PlayFoodAnimation(food);
        }
        else
        {
            Debug.Log("🍽️ No more feed uses. Watch an ad to get more.");
            CharacterBusy.Unlock();

            AdMobManager.Instance.ShowRewardedAd(() =>
            {
                usesRemaining++;
                SaveUses();
                UpdateUsesUI();
            });
        }

        RecordInteraction();
    }

    void PlayFoodAnimation(FoodType food)
    {
        HideAllFoods();

        TextureFader textureFader = tomAnimator.GetComponent<TextureFader>();

        switch (food)
        {
            case FoodType.Chili:
                chiliObj.SetActive(true);
                break;

            case FoodType.Pide:
                pideObj.SetActive(true);
                break;

            case FoodType.Banana:
                bananaObj.SetActive(true);
                break;

            case FoodType.Gum:
                gumPacketObj.SetActive(true);
                break;

            case FoodType.Apple:
                appleObj.SetActive(true);
                break;

            case FoodType.Cola:
                colaObj.SetActive(true);
                break;
        }

        tomAnimator.SetTrigger(food switch
        {
            FoodType.Chili => "Eat_Chili",
            FoodType.Pide => "Eat_Pide",
            FoodType.Banana => "Eat_Banana",
            FoodType.Gum => "Eat_Gum",
            FoodType.Apple => "Eat_Apple",
            FoodType.Cola => "Eat_Cola",
            _ => ""
        });
    }

    public void AddFeedUses(int amount)
    {
        if (unlimited) return;
        usesRemaining += amount;
        SaveUses();
        UpdateUsesUI();
        Debug.Log($"✅ Added {amount} feed uses via IAP");
    }

    public void SetUnlimited(bool on)
    {
        unlimited = on;
        if (unlimited)
        {
            usesText.text = "∞";
            Debug.Log("✅ Unlimited feed activated");
        }
    }

    // ---------- UI & Save ----------
    void ToggleFeedPanel() => feedOptionsPanel.SetActive(!feedOptionsPanel.activeSelf);
    void UpdateUsesUI()
    {
        if (unlimited)
            usesText.text = "∞";
        else
            usesText.text = usesRemaining.ToString();
    }
    void SaveUses() { PlayerPrefs.SetInt("FeedUsesRemaining", usesRemaining); PlayerPrefs.Save(); }
}
