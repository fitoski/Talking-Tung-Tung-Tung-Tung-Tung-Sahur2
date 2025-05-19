using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;  

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    private static IStoreController controller;
    private static IExtensionProvider extensions;

    public const string SKU_100 = "feed_100";
    public const string SKU_400 = "feed_400";
    public const string SKU_UNLIMITED = "unlimited_feed";

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        var module = StandardPurchasingModule.Instance();
        var builder = ConfigurationBuilder.Instance(module);
        builder.AddProduct(SKU_100, ProductType.Consumable);
        builder.AddProduct(SKU_400, ProductType.Consumable);
        builder.AddProduct(SKU_UNLIMITED, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController ctrl, IExtensionProvider ext)
    {
        controller = ctrl;
        extensions = ext;
        Debug.Log("✅ IAP initialized");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"❌ IAP init failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"❌ IAP init failed: {error} — {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var id = args.purchasedProduct.definition.id;
        switch (id)
        {
            case SKU_100:
                FeedManager.Instance.AddFeedUses(100);
                break;
            case SKU_400:
                FeedManager.Instance.AddFeedUses(400);
                break;
            case SKU_UNLIMITED:
                FeedManager.Instance.SetUnlimited(true);
                PlayerPrefs.SetInt("IAP_UnlimitedFeeds", 1);
                PlayerPrefs.Save();
                AdMobManager.Instance.DisableAds();
                break;
            default:
                Debug.LogWarning($"⚠️ Unknown IAP SKU: {id}");
                break;
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogWarning($"⚠️ Purchase {product.definition.id} failed: {reason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failure)
    {
        Debug.LogWarning($"⚠️ Purchase {product.definition.id} failed: {failure.reason}, {failure.message}");
    }

    public void Buy100() => controller.InitiatePurchase(SKU_100);
    public void Buy400() => controller.InitiatePurchase(SKU_400);
    public void BuyUnlimited() => controller.InitiatePurchase(SKU_UNLIMITED);
}
