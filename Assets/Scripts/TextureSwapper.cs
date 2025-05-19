using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TextureSwapper : MonoBehaviour
{
    [Header("What to swap in during banana-eat")]
    public Texture2D bananaTexture;

    private Renderer _renderer;
    private Material _material;
    private string _propName;
    private Texture _originalTexture;

    void Awake()
    {
        // grab whichever Renderer is on this object
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material; // instanced material

        // pick the right texture property:
        // URP Lit uses "_BaseMap", Standard uses "_MainTex"
        if (_material.HasProperty("_BaseMap")) _propName = "_BaseMap";
        else if (_material.HasProperty("_MainTex")) _propName = "_MainTex";
        else Debug.LogError($"[{name}] Shader has no _BaseMap or _MainTex!");

        // cache the original so we can restore it
        _originalTexture = _material.GetTexture(_propName);
    }

    /// <summary>
    /// Call this via an Animation Event at the frame you want the banana
    /// texture to appear.
    /// </summary>
    public void SwapToBanana()
    {
        _material.SetTexture(_propName, bananaTexture);
    }

    /// <summary>
    /// Call this via an Animation Event at the very end of your eat clip.
    /// </summary>
    public void RestoreOriginal()
    {
        _material.SetTexture(_propName, _originalTexture);
    }
}
