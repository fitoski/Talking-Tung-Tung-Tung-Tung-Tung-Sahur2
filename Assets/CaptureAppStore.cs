#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public static class CaptureAppStore
{
    //   iPhone 6.7" : Ctrl/Cmd + Alt + K
    [MenuItem("Tools/Capture ▶ iPhone 6.7\" (1290x2796) %&k")]
    private static void Cap_iPhone67()   => Capture(1290, 2796, "iPhone6p7");

    //   iPad 13"  P : Ctrl/Cmd + Alt + I
    [MenuItem("Tools/Capture ▶ iPad 13\" Portrait (2064x2752) %&i")]
    private static void Cap_iPad13_P()   => Capture(2064, 2752, "iPad13_P");

    //   iPad 13"  L : Ctrl/Cmd + Alt + O
    [MenuItem("Tools/Capture ▶ iPad 13\" Landscape (2752x2064) %&o")]
    private static void Cap_iPad13_L()   => Capture(2752, 2064, "iPad13_L");

    //   iPad 12.9" P : Ctrl/Cmd + Alt + U
    [MenuItem("Tools/Capture ▶ iPad 12.9\" Portrait (2048x2732) %&u")]
    private static void Cap_iPad129_P()  => Capture(2048, 2732, "iPad129_P");

    //   iPad 12.9" L : Ctrl/Cmd + Alt + Y
    [MenuItem("Tools/Capture ▶ iPad 12.9\" Landscape (2732x2048) %&y")]
    private static void Cap_iPad129_L()  => Capture(2732, 2048, "iPad129_L");

    // ---------- Ortak çekim fonksiyonu ----------
    private static void Capture(int width, int height, string tag)
    {
        var cam = Camera.main;
        if (!cam)
        {
            Debug.LogError("❌ Main Camera not found! Kameranıza 'MainCamera' tag’i verin.");
            return;
        }

        var rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
UnityEngine.Object.DestroyImmediate(rt);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename  = $"AppStoreShot_{tag}_{timestamp}.png";
        string path      = Path.Combine(Application.dataPath, "..", filename);

        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log($"✅ {width}×{height} ekran görüntüsü kaydedildi → {path}");

        AssetDatabase.Refresh();
    }
}
#endif
