using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using System.Numerics;

namespace AuraTracker;

public class SeenBuff
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public float Duration { get; set; }

    public SeenBuff(string name, string displayName, float duration) {
        Name = name;
        DisplayName = displayName;
        Duration = duration;
    }
}

public sealed class AuraTrackerSettings : ISettings {
    public ToggleNode Enable { get; set; } = new(true);

    public bool TrackMagic = false;
    public bool TrackRares = true;
    public bool TrackUniques = true;

    public bool TrackedAurasHeaderOpen = true;

    public int SnapshotHeight = 100;
    public bool SnapshotUnique = true;
    public bool SnapshotRare = true;
    public bool SnapshotMagic = false;

    public bool CaptureHeaderOpen = true;
    public bool CaptureBuffs = false;
    public bool CaptureBuffsSave = false;
    public int CaptureEveryXTicks = 10;
    public int CapturetHeight = 100;

    public bool BarHeaderOpen = true;
    public int BarHPadding = 4;
    public int BarVPadding = 1;
    public int BarWidth = 160;
    public int BarSpacing = 0;
    public Vector4 BarBackgroundColor = new(0, 0, 0, 0.75f);

    public List<AuraSettings> AuraSettingsList { get; set; } = new();
    public List<SeenBuff> SeenBuffs { get; set; } = new ();

    public void InitializeDefaultAuras()
    {
        if (AuraSettingsList.Count == 0)
        {  // Only add defaults if list is empty
            AuraSettingsList.AddRange(new List<AuraSettings> {
            new AuraSettings(false, "visual_archnemesis_mod_display_buff", "Unicorn Farts", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.51082253f, 0.51082253f, 0.51082253f, 1.0f)),
            new AuraSettings(true, "monster_flask_drain_aura", "Drought", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.50980395f, 0.26666668f, 0.050980393f, 1.0f)),
            new AuraSettings(true, "proximal_intangibility", "Intangibility", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.50980395f, 0.15686275f, 0.050980393f, 1.0f)),
            new AuraSettings(true, "stun_display_buff", "Stunned", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.6666667f, 0.6392157f, 0.19607843f, 1.0f)),
            new AuraSettings(true, "frozen", "Frozen", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.07058824f, 0.5921569f, 0.7058824f, 1.0f)),
            new AuraSettings(false, "chilled", "Chilled", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.27450982f, 0.59607846f, 0.6666667f, 1.0f)),
        });
        }
    }

    public void AddAura() {
        AuraSettingsList.Add( new AuraSettings(true, "Name", "", Vector4.One, new Vector4(.5f, .5f, .5f, 1)) );
    }

    public void AddAura(string name, string displayName, Vector4 textColor, Vector4 barColor)
    {
        // Add aura only if doesn't already exist
        if (!AuraSettingsList.Any(aura => aura.Name == name))
        {
            AuraSettingsList.Add(new AuraSettings(false, name, displayName, textColor, barColor));
        }
    }

    public void RemoveAura(int index) {
        if (index >= 0 && index < AuraSettingsList.Count) {
            AuraSettingsList.RemoveAt(index);
        }
    }

    public void RemoveDuplicateAuras()
    {
        AuraSettingsList = AuraSettingsList
            .GroupBy(aura => aura.Name)
            .Select(g => g.First())
            .ToList();
    }
}