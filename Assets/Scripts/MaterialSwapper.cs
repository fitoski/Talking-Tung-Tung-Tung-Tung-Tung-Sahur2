using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialSwapper : MonoBehaviour
{
    [Header("Drag your two mats here:")]
    public Material OriginalMat;  // the default
    public Material BananaMat;    // the one with banana texture

    private Renderer _r;

    void Awake()
    {
        _r = GetComponent<Renderer>();
        // Make sure the character starts with OriginalMat
        _r.material = OriginalMat;
    }

    /// <summary>
    /// Call this via an Animation Event at bite-start.
    /// </summary>
    public void SwapToBanana()
    {
        _r.material = BananaMat;
    }

    /// <summary>
    /// Call this via an Animation Event at the very end of the clip.
    /// </summary>
    public void RestoreOriginal()
    {
        _r.material = OriginalMat;
    }
}
