using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[RequireComponent(typeof(AudioSource))]
public class SpecialActionsManager : MonoBehaviour
{
    public static event Action onInteraction;

    public Button fartButton, breakButton, sfxButton;
    public Animator tomAnimator;
    public Image brokenScreenOverlay;

    public string fartTrigger = "Fart";
    public string breakTrigger = "BreakScreen";

    public AudioClip[] fartSfxList;
    public AudioClip sfxClip;

    [Header("Break SFX Variants")]
    public AudioClip[] breakSfxClips;

    [Header("Single Break Sound (for final smash)")]
    public AudioClip breakSfx;

    public float fartAnimDuration = 0.8f;
    public float breakAnimDuration = 1f;
    public float overlayFadeDuration = 1f;

    AudioSource audioSource;
    FeedManager feedManager;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        feedManager = FindFirstObjectByType<FeedManager>();

        fartButton.onClick.AddListener(HandleFart);
        breakButton.onClick.AddListener(HandleBreak);
        sfxButton.onClick.AddListener(HandleSfx);

        brokenScreenOverlay.gameObject.SetActive(false);
        var c = brokenScreenOverlay.color;
        brokenScreenOverlay.color = new Color(c.r, c.g, c.b, 0f);
    }

    void HandleFart()
    {
        onInteraction?.Invoke();
        if (!CharacterBusy.TryLock()) return;
        StartCoroutine(FartRoutine());
    }

    IEnumerator FartRoutine()
    {
        tomAnimator.SetTrigger(fartTrigger);
        onInteraction?.Invoke();
        feedManager?.RecordInteraction();
        yield return new WaitForSeconds(fartAnimDuration);
        CharacterBusy.Unlock();
    }

    void HandleBreak()
    {
        onInteraction?.Invoke();
        if (!CharacterBusy.TryLock()) return;
        StartCoroutine(BreakRoutine());
    }

    IEnumerator BreakRoutine()
    {
        tomAnimator.SetTrigger(breakTrigger);
        onInteraction?.Invoke();
        feedManager?.RecordInteraction();
        yield return new WaitForSeconds(breakAnimDuration);
        CharacterBusy.Unlock();
    }

    void HandleSfx()
    {
        onInteraction?.Invoke();

        if (!CharacterBusy.TryLock()) return;
        StartCoroutine(SfxRoutine());
    }

    IEnumerator SfxRoutine()
    {
        if (sfxClip != null)
            audioSource.PlayOneShot(sfxClip);
        feedManager?.RecordInteraction();
        yield return new WaitForSeconds(0.5f);
        CharacterBusy.Unlock();
    }

    public void TriggerBrokenScreenEffect()
    {
        brokenScreenOverlay.gameObject.SetActive(true);
        if (breakSfx != null)
            audioSource.PlayOneShot(breakSfx);
        StartCoroutine(FadeBrokenOverlay());
    }

    public void PlayRandomFartSfx()
    {
        if (fartSfxList == null || fartSfxList.Length == 0) return;
        int idx = UnityEngine.Random.Range(0, fartSfxList.Length);
        audioSource.PlayOneShot(fartSfxList[idx]);
    }

    public void PlayBreakSfxAtIndex(int index)
    {
        if (breakSfxClips == null ||
            index < 0 ||
            index >= breakSfxClips.Length) return;

        audioSource.PlayOneShot(breakSfxClips[index]);
    }

    IEnumerator FadeBrokenOverlay()
    {
        float t = 0f;
        var col = brokenScreenOverlay.color;
        while (t < overlayFadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / overlayFadeDuration);
            brokenScreenOverlay.color = new Color(col.r, col.g, col.b, a);
            yield return null;
        }
        brokenScreenOverlay.gameObject.SetActive(false);
        brokenScreenOverlay.color = new Color(col.r, col.g, col.b, 1f);
    }
}
