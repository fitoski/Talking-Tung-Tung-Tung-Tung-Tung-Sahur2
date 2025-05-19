using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AgeGateManager : MonoBehaviour
{
    public GameObject ageGatePanel;
    public TMP_InputField ageInputField;
    public Button submitButton;
    public TMP_Text errorText;

    public static bool IsChild { get; private set; }
    public static bool Underage15 { get; private set; }

    const string AgeKey = "UserAge";

    void Awake()
    {
        if (PlayerPrefs.HasKey(AgeKey))
        {
            SetFlags(PlayerPrefs.GetInt(AgeKey));
            ageGatePanel.SetActive(false);

            AdMobManager.Instance.ConfigureForAge(IsChild, Underage15);
        }
        else
        {
            ageGatePanel.SetActive(true);
            submitButton.onClick.AddListener(OnSubmit);
        }
    }

    void OnSubmit()
    {
        errorText.text = "";
        if (!int.TryParse(ageInputField.text, out var age) || age < 0 || age > 120)
        {
            errorText.text = "Please enter a valid age.";
            return;
        }

        PlayerPrefs.SetInt(AgeKey, age);
        PlayerPrefs.Save();
        SetFlags(age);
        ageGatePanel.SetActive(false);

        AdMobManager.Instance.ConfigureForAge(IsChild, Underage15);
    }

    static void SetFlags(int age)
    {
        IsChild = age < 13;
        Underage15 = age < 15;
    }
}
