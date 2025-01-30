using System.Numerics;

namespace AuraTracker;

public class AuraSettings
{
    public string Name;
    public string DisplayName;
    public Vector4 TextColor;
    public Vector4 BarColor;
    public bool Enabled;

    public AuraSettings(bool enabled, string name, string displayName, Vector4 textcolor, Vector4 barColor) {
        Enabled = enabled;
        Name = name;
        DisplayName = displayName;
        TextColor = textcolor;
        BarColor = barColor;
    }
}
