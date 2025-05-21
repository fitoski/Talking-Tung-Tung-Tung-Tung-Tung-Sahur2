// Assets/Editor/CaptureAppStore.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public static class CaptureAppStore
{
    // Captures a 6.7-inch App Store screenshot with a unique timestamped filename
    [MenuItem("Tools/Capture 6.7-inch Screenshot %#k")]
    public static void Capture()
    {
        const int W = 1290, H = 2796;
        // Create a RenderTexture at the App Store resolution
        var rt = new RenderTexture(W, H, 24);
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("❌ Main Camera not found! Ensure your camera has the 'MainCamera' tag.");
            return;
        }
        // Render the camera's view into the texture
        cam.targetTexture = rt;
        cam.Render();

        // Read pixels from the RenderTexture
        RenderTexture.active = rt;
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        tex.Apply();

        // Clean up
        cam.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(rt);

        // Generate a unique filename with timestamp
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"AppStoreShot_6p7_{timestamp}.png";
        string path = Path.Combine(Application.dataPath, "..", filename);

        // Write the PNG file
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log($"✅ Screenshot saved: {path}");

        // Refresh the AssetDatabase so the file appears in Unity
        AssetDatabase.Refresh();
    }
}
#endif
