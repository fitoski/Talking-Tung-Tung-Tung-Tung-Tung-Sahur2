using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureFader : MonoBehaviour
{
    [Header("Body Mesh Renderers")]
    public SkinnedMeshRenderer[] bodyRenderers;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    List<Material> _mats;
    int _blendPropID;

    Coroutine _routine;

    void Awake()
    {
        // 1) Body renderers’dan materyalleri al
        _mats = new List<Material>();
        foreach (var smr in bodyRenderers)
            _mats.Add(smr.material);

        // 2) Shader’daki _BlendFactor property ID’si
        _blendPropID = Shader.PropertyToID("_BlendFactor");
    }

    public void StartBananaFade()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(BlendTo(1f));
    }

    public void EndBananaFade()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(BlendTo(0f));
    }

    IEnumerator BlendTo(float target)
    {
        // 0→1 veya 1→0 arası blend
        float start = _mats[0].GetFloat(_blendPropID);
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float f = Mathf.Lerp(start, target, t / fadeDuration);
            SetBlend(f);
            yield return null;
        }
        SetBlend(target);
    }

    void SetBlend(float b)
    {
        foreach (var mat in _mats)
            mat.SetFloat(_blendPropID, b);
    }
}
