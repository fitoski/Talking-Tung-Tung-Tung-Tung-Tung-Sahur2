using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class TomIdleBehaviour : MonoBehaviour
{
    [Header("Blink Settings")]
    public float minBlinkInterval = 3f;
    public float maxBlinkInterval = 8f;

    [Header("Head Look (Optional)")]
    public Transform headBone;
    public float lookIntervalMin = 4f;
    public float lookIntervalMax = 10f;
    public float maxHeadTurnAngle = 20f;
    public float headTurnDuration = 0.5f;

    [Header("Hunger Settings")]
    [Tooltip("Hiç etkileşim olmazsa ne kadar sürede açlık animasyonuna geçsin")]
    public float hungerDelay = 15f;
    public string hungerTrigger = "Hunger";
    public AudioClip hungerSfx;

    Animator animator;
    AudioSource audioSource;
    float blinkTimer;
    float lookTimer;
    float hungerTimer;
    Quaternion headOriginalRot;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        TomInteraction.onInteraction += ResetHungerTimer;
        SpecialActionsManager.onInteraction += ResetHungerTimer;
        FeedManager.onInteraction += ResetHungerTimer;
    }

    void OnDisable()
    {
        TomInteraction.onInteraction -= ResetHungerTimer;
        SpecialActionsManager.onInteraction -= ResetHungerTimer;
        FeedManager.onInteraction -= ResetHungerTimer;
    }

    void Start()
    {
        ResetBlinkTimer();
        ResetLookTimer();
        ResetHungerTimer();

        if (headBone != null)
            headOriginalRot = headBone.localRotation;
    }

    void Update()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);
        bool inFeeding = state.IsTag("Feeding");
        bool inIdle = state.IsName("Idle");

        if (!CharacterBusy.IsBusy && !inFeeding)
        {
            HandleBlink(state);

            if (inIdle)
                HandleHeadLook();
        }

        if (!CharacterBusy.IsBusy && inIdle)
            HandleHunger(state);
    }

    void ResetBlinkTimer()
    {
        blinkTimer = UnityEngine.Random.Range(minBlinkInterval, maxBlinkInterval);
    }

    void HandleBlink(AnimatorStateInfo state)
    {
        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0f)
        {
            if (!state.IsTag("Feeding"))
                animator.SetTrigger("Blink");

            ResetBlinkTimer();
        }
    }

    void ResetLookTimer()
    {
        lookTimer = UnityEngine.Random.Range(lookIntervalMin, lookIntervalMax);
    }

    void HandleHeadLook()
    {
        lookTimer -= Time.deltaTime;
        if (lookTimer <= 0f)
        {
            StartCoroutine(HeadLookRoutine());
            ResetLookTimer();
        }
    }

    IEnumerator HeadLookRoutine()
    {
        float angle = UnityEngine.Random.Range(-maxHeadTurnAngle, maxHeadTurnAngle);
        Quaternion tgt = headOriginalRot * Quaternion.Euler(0f, angle, 0f);
        float elapsed = 0f;

        while (elapsed < headTurnDuration)
        {
            headBone.localRotation = Quaternion.Slerp(headOriginalRot, tgt, elapsed / headTurnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        headBone.localRotation = tgt;

        yield return new WaitForSeconds(0.5f);

        elapsed = 0f;
        while (elapsed < headTurnDuration)
        {
            headBone.localRotation = Quaternion.Slerp(tgt, headOriginalRot, elapsed / headTurnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        headBone.localRotation = headOriginalRot;
    }

    void HandleHunger(AnimatorStateInfo state)
    {
        if (state.IsTag("Feeding") || CharacterBusy.IsBusy) return;

        hungerTimer -= Time.deltaTime;
        if (hungerTimer <= 0f)
        {
            animator.SetTrigger(hungerTrigger);
            if (hungerSfx != null)
                audioSource.PlayOneShot(hungerSfx);
            ResetHungerTimer();
        }
    }

    public void ResetHungerTimer()
    {
        hungerTimer = hungerDelay;
    }
}
