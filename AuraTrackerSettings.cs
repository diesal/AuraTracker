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

public class Aura
{
    public string Name;
    public string DisplayName;
    public Vector4 TextColor;
    public Vector4 BarColor;
    public bool Enabled;

    public Aura(bool enabled, string name, string displayName, Vector4 textcolor, Vector4 barColor)
    {
        Enabled = enabled;
        Name = name;
        DisplayName = displayName;
        TextColor = textcolor;
        BarColor = barColor;
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

    public List<Aura> AuraList { get; set; } = new();
    public List<SeenBuff> SeenBuffs { get; set; } = new ();

    public void InitAuraList()
    {
        if (AuraList.Count == 0)
        {  // Only add defaults if list is empty
            AuraList.AddRange(new List<Aura> {
            new Aura(false, "visual_archnemesis_mod_display_buff", "Unicorn Farts", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.51082253f, 0.51082253f, 0.51082253f, 1.0f)),
            new Aura(true, "monster_flask_drain_aura", "Drought", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.50980395f, 0.26666668f, 0.050980393f, 1.0f)),
            new Aura(true, "proximal_intangibility", "Intangibility", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.50980395f, 0.15686275f, 0.050980393f, 1.0f)),
            new Aura(true, "stun_display_buff", "Stunned", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.6666667f, 0.6392157f, 0.19607843f, 1.0f)),
            new Aura(true, "frozen", "Frozen", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.07058824f, 0.5921569f, 0.7058824f, 1.0f)),
            new Aura(false, "chilled", "Chilled", new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.27450982f, 0.59607846f, 0.6666667f, 1.0f)),
        });
        }
    }

    public void AddAura() {
        AuraList.Add(new Aura(true, "Name", "", Vector4.One, new Vector4(.5f, .5f, .5f, 1)) );
    }

    public void AddAura(string name, string displayName, Vector4 textColor, Vector4 barColor)
    {
        // Add aura only if doesn't already exist
        if (!AuraList.Any(aura => aura.Name == name))
        {
            AuraList.Add(new Aura(true, name, displayName, textColor, barColor));
        }
    }

    public void RemoveAura(int index) {
        if (index >= 0 && index < AuraList.Count) {
            AuraList.RemoveAt(index);
        }
    }

    public void RemoveDuplicateAuras()
    {
        AuraList = AuraList
            .GroupBy(aura => aura.Name)
            .Select(g => g.First())
            .ToList();
    }
}