using UnityModManagerNet;

namespace KeyLimiter.Features.Settings;

public sealed class ModSettings : UnityModManager.ModSettings
{
    public bool Enable = true;
    public int AvailableKeys = 12;
    public KeyLimitExceededAction ExceededAction = KeyLimitExceededAction.Ignore;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }
}
