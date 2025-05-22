#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;

static class IOSAudioRoute
{
    [DllImport("__Internal")]
    private static extern void _ForceSpeaker();

    [UnityEngine.RuntimeInitializeOnLoadMethod]
    private static void Init() => _ForceSpeaker();
}
#endif
