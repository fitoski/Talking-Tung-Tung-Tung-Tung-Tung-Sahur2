using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource), typeof(Animator))]
public class AutoVoiceController : MonoBehaviour
{
    public float detectionThreshold = 0.05f;
    public int sampleWindow = 1024;
    public float silenceDuration = 0.5f;
    public float maxRecordTime = 10f;

    [Header("Animator Triggers")]
    public string triggerStartListening = "StartListening";
    public string triggerStopListening = "StopListening";
    public string triggerStartSpeaking = "StartSpeaking";
    public string triggerStopSpeaking = "StopSpeaking";

    public float playbackPitch = 1.7f;

    AudioSource audioSource;
    Animator animator;
    string micDevice;
    int sampleRate;
    AudioClip detectionClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        sampleRate = AudioSettings.outputSampleRate;
        if (Microphone.devices.Length > 0)
            micDevice = Microphone.devices[0];
        else
            Debug.LogWarning("🎤 Mikrofon bulunamadı!");

        StartDetection();
    }

    void StartDetection()
    {
        detectionClip = Microphone.Start(micDevice, true, 1, sampleRate);
    }

    void StopDetection()
    {
        Microphone.End(micDevice);
        detectionClip = null;
    }

    void Update()
    {
        if (!CharacterBusy.TryLock()) return;

        if (detectionClip == null)
        {
            CharacterBusy.Unlock();
            return;
        }

        float rms = GetRMS(detectionClip, sampleWindow);
        if (rms > detectionThreshold)
        {
            StopDetection();
            StartCoroutine(ListenRecordSpeakRoutine());
        }
        else
        {
            CharacterBusy.Unlock();
        }
    }

    float GetRMS(AudioClip clip, int window)
    {
        int pos = Microphone.GetPosition(micDevice);
        if (pos < window) return 0f;
        float[] buf = new float[window];
        clip.GetData(buf, pos - window);
        float sum = 0f;
        foreach (var s in buf) sum += s * s;
        return Mathf.Sqrt(sum / window);
    }

    IEnumerator ListenRecordSpeakRoutine()
    {
        animator.SetTrigger(triggerStartListening);

        var recordClip = Microphone.Start(micDevice, false,
            Mathf.CeilToInt(maxRecordTime), sampleRate);

        float silenceTimer = 0f;
        int lastPos = 0;
        var buffer = new List<float>();
        float t0 = Time.time;
        while (true)
        {
            int pos = Microphone.GetPosition(micDevice);
            int diff = pos - lastPos; if (diff < 0) diff += recordClip.samples;
            if (diff > 0)
            {
                var chunk = new float[diff];
                recordClip.GetData(chunk, lastPos);
                buffer.AddRange(chunk);
                float sum = 0f; foreach (var s in chunk) sum += s * s;
                float lvl = Mathf.Sqrt(sum / chunk.Length);
                if (lvl < detectionThreshold) silenceTimer += Time.deltaTime;
                else silenceTimer = 0f;
                lastPos = pos;
            }
            if (silenceTimer >= silenceDuration || Time.time - t0 >= maxRecordTime) break;
            yield return null;
        }

        Microphone.End(micDevice);
        animator.SetTrigger(triggerStopListening);

        var st = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(st.length);

        animator.SetTrigger(triggerStartSpeaking);

        int channels = recordClip.channels;
        var playClip = AudioClip.Create("play", buffer.Count / channels,
                          channels, recordClip.frequency, false);
        playClip.SetData(buffer.ToArray(), 0);

        audioSource.pitch = playbackPitch;
        audioSource.PlayOneShot(playClip);
        yield return new WaitForSeconds(playClip.length / playbackPitch);

        animator.SetTrigger(triggerStopSpeaking);

        audioSource.pitch = 1f;
        StartDetection();
        CharacterBusy.Unlock();
    }
}
