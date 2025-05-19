using UnityEngine;

public class GumChewController : MonoBehaviour
{
    [Header("Gum Objects")]
    [Tooltip("Sakız paketi (elle açılan)")]
    public GameObject gumPacketObj;
    [Tooltip("Ağızda şişirilip patlatılan sakız")]
    public GameObject gumChewObj;

    void Start()
    {
        gumPacketObj?.SetActive(false);
        gumChewObj?.SetActive(false);
    }

    public void ShowGumChew()
    {
        gumChewObj?.SetActive(true);
    }

    public void HideGumChewAndPacket()
    {
        gumChewObj?.SetActive(false);
        gumPacketObj?.SetActive(false);
    }

    public void HideGumPacket()
    {
        gumPacketObj?.SetActive(false);
    }
}
