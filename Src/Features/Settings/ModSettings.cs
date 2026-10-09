using UnityModManagerNet;

namespace KeyLimiter.Features.Settings;

public sealed class ModSettings : UnityModManager.ModSettings
{
    public bool Enable = true;
    public int LimitCount = 12;
    public KeyLimitExceededAction ExeededAction = KeyLimitExceededAction.Ignore;

    public bool ShowHud = true;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        Save(this, modEntry);
    }
}
