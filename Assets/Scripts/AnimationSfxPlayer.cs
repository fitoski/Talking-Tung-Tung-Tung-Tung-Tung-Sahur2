using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimationSfxPlayer : MonoBehaviour
{
    AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
            _src.PlayOneShot(clip);
    }
}
