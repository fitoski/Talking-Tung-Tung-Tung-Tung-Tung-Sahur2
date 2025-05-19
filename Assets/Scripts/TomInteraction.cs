using UnityEngine;
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]
public class TomInteraction : MonoBehaviour
{
    public static event Action onInteraction;

    [Header("Triggers")]
    public string pokeHeadTrigger = "Poke_Head";
    public string pokeBellyTrigger = "Poke_Belly";
    public string fallTrigger = "Fall";

    [Header("Logic")]
    public int maxPokes = 5;

    [Header("Poke SFX")]
    public AudioClip headPokeSfx;
    public AudioClip bellyPokeSfx;

    [Header("Fall SFX (event driven)")]
    public AudioClip dizzySfx;
    public AudioClip fallImpactSfx;

    private Animator animator;
    private AudioSource audioSource;
    private Camera cam;
    private int pokeCount;
    private bool isPokeAnimating;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        cam = Camera.main;
    }

    void Update()
    {
        if (isPokeAnimating) return;

        if (Input.GetMouseButtonDown(0))
            TryProcess(Input.mousePosition);
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryProcess(Input.GetTouch(0).position);
    }

    private void TryProcess(Vector2 screenPos)
    {
        if (!CharacterBusy.TryLock()) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);

        if (Physics.Raycast(ray, out var hit, 100f) && hit.transform.IsChildOf(transform))
        {
            if (hit.collider.CompareTag("HeadCollider"))
                BeginPoke(pokeHeadTrigger, headPokeSfx);
            else if (hit.collider.CompareTag("BellyCollider"))
                BeginPoke(pokeBellyTrigger, bellyPokeSfx);
            else
                CharacterBusy.Unlock();
        }
        else
        {
            CharacterBusy.Unlock();
        }
    }

    private void BeginPoke(string trigger, AudioClip sfx)
    {
        onInteraction?.Invoke();

        isPokeAnimating = true;
        animator.SetTrigger(trigger);

        if (sfx != null)
            audioSource.PlayOneShot(sfx);

        pokeCount++;
        if (pokeCount >= maxPokes)
        {
            pokeCount = 0;
            animator.SetTrigger(fallTrigger);
        }
    }

    public void OnPokeAnimationEnd()
    {
        onInteraction?.Invoke();

        isPokeAnimating = false;
        CharacterBusy.Unlock();
    }

    public void OnFallAnimationEnd()
    {
        onInteraction?.Invoke();

        animator.Play("Idle", 0, 0f);
        isPokeAnimating = false;
        CharacterBusy.Unlock();
    }

    public void PlayDizzySfx()
    {
        if (dizzySfx != null)
            audioSource.PlayOneShot(dizzySfx);
    }

    public void PlayFallImpactSfx()
    {
        if (fallImpactSfx != null)
            audioSource.PlayOneShot(fallImpactSfx);
    }
}
