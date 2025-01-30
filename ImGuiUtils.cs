using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AuraTracker;

/// <summary>
/// Provides helper methods for ImGui controls.
/// </summary>
public static class ImGuiUtils
{
    /// <summary>
    /// Renders a checkbox with an optional tooltip.
    /// </summary>
    /// <param name="label">The label for the checkbox.</param>
    /// <param name="tooltip">\Ttooltip text to display when the checkbox is hovered.</param>
    /// <param name="value">The boolean value of the checkbox.</param>
    public static void Checkbox(string label, string tooltip, ref bool value) {
        ImGui.Checkbox(label, ref value);
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.Text(tooltip);
            ImGui.EndTooltip();
        }
    }
    private static Vector2 _popupOffset = new(10, 10);
    public static void ColorSwatch(string desc_id, ref Vector4 color) {
        if (ImGui.ColorButton(desc_id, color)) {
            ImGui.SetNextWindowPos(ImGui.GetMousePos() + _popupOffset, ImGuiCond.Always);
            ImGui.OpenPopup(desc_id);
        }
        if (ImGui.BeginPopup(desc_id)) {
            ImGui.ColorPicker4(desc_id, ref color);
            ImGui.EndPopup();
        }
    }

    public static bool CollapsingHeader(string label, ref bool open) {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;
        if (open) flags |= ImGuiTreeNodeFlags.DefaultOpen;

        open = ImGui.CollapsingHeader(label, flags);

        return open;
    }

    private static readonly Vector4 _defaultTint = new Vector4(1f, 1f, 1f, 1f);

    /// <summary>
    /// Shows an icon picker window.
    /// </summary>
    /// <param name="name">The name of the window.</param>
    /// <param name="selectedIcon">The currently selected icon index.</param>
    /// <param name="iconAtlas">The icon atlas to use for the picker.</param>
    /// <param name="tint">The optional tint to apply to the icons.</param>
    /// <returns>True if the window is open </returns>


    public static System.Drawing.Color Vector4ToColor(Vector4 vector) {
        // Ensure the vector components are in the range [0, 1]
        vector = Vector4.Clamp(vector, Vector4.Zero, Vector4.One);

        // Convert the vector components to a Color
        int alpha = (int)(vector.W * 255);
        int red = (int)(vector.X * 255);
        int green = (int)(vector.Y * 255);
        int blue = (int)(vector.Z * 255);

        return System.Drawing.Color.FromArgb(alpha, red, green, blue);
    }

}
