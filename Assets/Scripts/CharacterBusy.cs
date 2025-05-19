using UnityEngine;

public static class CharacterBusy
{
    public static bool IsBusy { get; private set; }

    public static bool TryLock()
    {
        if (IsBusy) return false;
        IsBusy = true;
        return true;
    }

    public static void Unlock() => IsBusy = false;
}
