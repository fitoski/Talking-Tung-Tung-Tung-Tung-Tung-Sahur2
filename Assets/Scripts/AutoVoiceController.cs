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

    [Range(0.6f, 1.5f)]
    public float playbackPitch = 1.3f;   

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
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("🎤 No microphone!");
            enabled = false;
            return;
        }
        micDevice = Microphone.devices[0];
        StartDetection();
    }

    void StartDetection()
    {
        detectionClip = Microphone.Start(micDevice, true, 1, sampleRate);
        StartCoroutine(MicWarmup());
    }
    IEnumerator MicWarmup()
    {
        while (Microphone.GetPosition(micDevice) == 0)
            yield return null;
    }
    void StopDetection()
    {
        Microphone.End(micDevice);
        detectionClip = null;
    }

    void Update()
    {
        if (detectionClip == null || !CharacterBusy.TryLock()) return;

        if (GetRMS(detectionClip) > detectionThreshold)
        {
            StopDetection();
            StartCoroutine(ListenRecordSpeakRoutine());
        }
        else CharacterBusy.Unlock();
    }

    float GetRMS(AudioClip clip)
    {
        int pos = Microphone.GetPosition(micDevice);
        if (pos < sampleWindow) return 0f;
        float[] buf = new float[sampleWindow];
        clip.GetData(buf, pos - sampleWindow);
        double sum = 0;
        for (int i = 0; i < buf.Length; i++) sum += buf[i] * buf[i];
        return Mathf.Sqrt((float)(sum / sampleWindow));
    }

    IEnumerator ListenRecordSpeakRoutine()
    {
        animator.SetTrigger(triggerStartListening);

        var recordClip = Microphone.Start(micDevice, false,
                          Mathf.CeilToInt(maxRecordTime), sampleRate);

        float silenceTimer = 0f;
        int lastPos = 0;
        List<float> buffer = new();

        float startTime = Time.time;
        while (true)
        {
            int pos = Microphone.GetPosition(micDevice);
            int diff = pos - lastPos; if (diff < 0) diff += recordClip.samples;

            if (diff > 0)
            {
                var chunk = new float[diff];
                recordClip.GetData(chunk, lastPos);
                buffer.AddRange(chunk);

                float lvl = 0f;
                for (int i = 0; i < chunk.Length; i++) lvl += chunk[i] * chunk[i];
                lvl = Mathf.Sqrt(lvl / chunk.Length);
                silenceTimer = (lvl < detectionThreshold) ? silenceTimer + Time.deltaTime : 0f;
                lastPos = pos;
            }

            if (silenceTimer >= silenceDuration ||
                Time.time - startTime >= maxRecordTime) break;

            yield return null;
        }

        Microphone.End(micDevice);
        animator.SetTrigger(triggerStopListening);

        var st = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(st.length);

        animator.SetTrigger(triggerStartSpeaking);

        int channels = recordClip.channels;
        var playClip = AudioClip.Create("playback", buffer.Count / channels,
                                        channels, sampleRate, false);
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
