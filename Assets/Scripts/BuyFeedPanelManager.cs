using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuyFeedPanelManager : MonoBehaviour
{
    [Header("References")]
    public GameObject buyFeedPanel;
    public Button closeButton;
    public Button watchAdOption;
    public Button buy100Option;
    public Button buy400Option;
    public Button buyUnlimitedOption;
    public TMP_Text buy100PriceText;
    public TMP_Text buy400PriceText;
    public TMP_Text buyUnlimitedPriceText;

    void Awake()
    {
        buyFeedPanel.SetActive(false);

        closeButton.onClick.AddListener(() => buyFeedPanel.SetActive(false));
        watchAdOption.onClick.AddListener(OnWatchAdClicked);

        buy100Option.onClick.AddListener(OnBuy100Clicked);
        buy400Option.onClick.AddListener(OnBuy400Clicked);
        buyUnlimitedOption.onClick.AddListener(OnBuyUnlimitedClicked);

        buy100PriceText.text = "$0.99";
        buy400PriceText.text = "$2.99";
        buyUnlimitedPriceText.text = "$9.99";
    }

    public void ShowBuyFeedPanel()
    {
        buyFeedPanel.SetActive(true);
    }

    void OnWatchAdClicked()
    {
        buyFeedPanel.SetActive(false);
        AdMobManager.Instance.ShowRewardedAd(() =>
        {
            FeedManager.Instance.AddFeedUses(1);
        });
    }

    void OnBuy100Clicked()
    {
        buyFeedPanel.SetActive(false);
        IAPManager.Instance.Buy100();
    }

    void OnBuy400Clicked()
    {
        buyFeedPanel.SetActive(false);
        IAPManager.Instance.Buy400();
    }

    void OnBuyUnlimitedClicked()
    {
        buyFeedPanel.SetActive(false);
        IAPManager.Instance.BuyUnlimited();
    }
}
