
using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.RenderQ;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Interfaces;
using ImGuiNET;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text;
using static ExileCore2.PoEMemory.Components.EffectPack;

namespace AuraTracker;

public class AuraTracker : BaseSettingsPlugin<AuraTrackerSettings>
{
    private Camera Camera => GameController.IngameState.Camera;
    private HashSet<string> _seenBuffNames = new HashSet<string>(StringComparer.Ordinal);

    private int TickCounter { get; set; }
    private string _snapshot = "";
    private string _capturedBuffs = "";
    private Vector4 _defaultTextColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    private Vector4 _defaultBarColor = new Vector4(0.5f, 0.5f, 0.5f, 1);

    public override bool Initialise() {
        Settings.InitAuraList();
        Settings.RemoveDuplicateAuras(); // Clean up existing duplicates if there are any
        UpdateCaptureBuffs();
        InitializeSeenBuffNames();

        return true;
    }

    private void InitializeSeenBuffNames()
    {
        foreach (var b in Settings.SeenBuffs)
            _seenBuffNames.Add(b.Name);
    }

    private void DrawAura(Aura auraSettings, int index) {
        ImGuiUtils.Checkbox($"##EnableAura{index}", "Enable Aura", ref auraSettings.Enabled); ImGui.SameLine();
        ImGuiUtils.ColorSwatch($"Text Color ##{index}", ref auraSettings.TextColor); ImGui.SameLine();
        ImGuiUtils.ColorSwatch($"Bar Color ##{index}", ref auraSettings.BarColor); ImGui.SameLine();
        ImGui.PushItemWidth(200);
        ImGui.InputText($"##AuraName{index}", ref auraSettings.Name, 100); ImGui.SameLine();
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.Text("Aura Name. Use captured auras to find the desired aura.");
            ImGui.EndTooltip();
        };
        ImGui.InputText($"##AuraDisplayName{index}", ref auraSettings.DisplayName, 100); ImGui.SameLine();
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.Text("Name shown for the tracked aura in-game. Defaults to the aura name if not specified.");
            ImGui.EndTooltip();
        }
        if (ImGui.Button($"Remove##{index}")) Settings.RemoveAura(index);

    }
    public override void DrawSettings() {
        ImGuiUtils.Checkbox($"Track Magic Monster", "Track auras on Magic Monsters", ref Settings.TrackMagic);
        ImGuiUtils.Checkbox($"Track Rare Monster", "Track auras onrare Monsters", ref Settings.TrackRares);
        ImGuiUtils.Checkbox($"Track Unique Monster", "Track auras on Unique Monsters", ref Settings.TrackUniques);

        if (ImGuiUtils.CollapsingHeader("Bar Settings", ref Settings.BarHeaderOpen)) {
            ImGui.Indent();

            ImGuiUtils.ColorSwatch("Bar Background Color", ref Settings.BarBackgroundColor); ImGui.SameLine();
            ImGui.Text("Bar Background Color");

            ImGui.PushItemWidth(100);
            ImGui.SliderInt("##HPadding", ref Settings.BarHPadding, 0, 20); ImGui.SameLine();
            ImGui.SliderInt("Bar Padding##VPadding", ref Settings.BarVPadding, 0, 20);
            ImGui.PopItemWidth();

            ImGui.PushItemWidth(200 + ImGui.GetStyle().ItemInnerSpacing.X);
            ImGui.SliderInt("Bar Width", ref Settings.BarWidth, 100, 300);

            ImGui.SliderInt("Bar Spacing", ref Settings.BarSpacing, 0, 10);
            ImGui.PopItemWidth();

            ImGui.Unindent();
        }

        if (ImGuiUtils.CollapsingHeader("Tracked Auras", ref Settings.TrackedAurasHeaderOpen)) {
            ImGui.Indent();
            for (int i = 0; i < Settings.AuraList.Count; i++) { DrawAura(Settings.AuraList[i], i); }
            if (ImGui.Button("Add Aura")) { Settings.AddAura(); }
            ImGui.Unindent();
        }

        if (ImGuiUtils.CollapsingHeader("Captured Auras", ref Settings.CaptureHeaderOpen)) {
            ImGui.Indent();
            ImGui.Checkbox("Capture Auras", ref Settings.CaptureBuffs); ImGui.SameLine();
            if (ImGui.IsItemHovered()) {
                ImGui.BeginTooltip();
                ImGui.Text("Enable this option to capture auras from entities.");
                ImGui.Text("When enabled, the plugin will scan entities for auras and record them.");
                ImGui.Text("Captured auras will be displayed in the text area below.");
                ImGui.Text("Use this feature to identify and track new auras.");
                ImGui.EndTooltip();
            }
            ImGui.Checkbox("Auto Add Captured", ref Settings.CaptureBuffsSave); ImGui.SameLine();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Enable this option to add captured auras to the list above.");
                ImGui.Text("This will only add auras that have a display name");
                ImGui.EndTooltip();
            }
            ImGui.PushItemWidth(100);
            ImGui.SliderInt("Capture rate", ref Settings.CaptureEveryXTicks, 10, 100); ImGui.SameLine();
            if (ImGui.IsItemHovered()) {
                ImGui.BeginTooltip();
                ImGui.Text("Set the interval (in ticks) for attmepting to captures new Auras");
                ImGui.EndTooltip();
            };
            ImGui.SliderInt("Height##Capture", ref Settings.CapturetHeight, 100, 1000);
            ImGui.PopItemWidth();
            ImGui.InputTextMultiline("##CaptureML", ref _capturedBuffs, 10000, new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, Settings.CapturetHeight), ImGuiInputTextFlags.ReadOnly);
            ImGui.Unindent();
        }

        if (ImGui.CollapsingHeader("Snapshot Auras")) {
            ImGui.Indent();
            ImGui.Checkbox("Unique", ref Settings.SnapshotUnique); ImGui.SameLine();
            ImGui.Checkbox("Rare", ref Settings.SnapshotRare); ImGui.SameLine();
            ImGui.Checkbox("Magic", ref Settings.SnapshotMagic); ImGui.SameLine();
            if (ImGui.Button("Snapshot")) { Snapshot(); }; ImGui.SameLine();
            ImGui.PushItemWidth(100);
            ImGui.SliderInt("Height##Snapshot", ref Settings.SnapshotHeight, 100, 1000);
            ImGui.PopItemWidth();
            ImGui.InputTextMultiline("##snapshot", ref _snapshot, 1000, new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X, Settings.SnapshotHeight), ImGuiInputTextFlags.ReadOnly);
            ImGui.Unindent();
        }
    }

    public override void Tick() {
        TickCounter++;
        if (Settings.CaptureBuffs &&
            !TheGame.TheGame.IsEscapeState &&
            TickCounter % Settings.CaptureEveryXTicks != 0)
        {
            CaptureBuffs();
        }
    }

    public override void Render()
    {
        // Cache frequently used local variables
        var shadowOffset = new Vector2(1, 1);
        var shadowColor = Color.Black;

        var barWidth = Settings.BarWidth;
        var barHPadding = Settings.BarHPadding;
        var barVPadding = Settings.BarVPadding;
        var barSpacing = Settings.BarSpacing;
        var barInset = 1.0f;
        var barBackgroundColor = ImGuiUtils.Vector4ToColor(Settings.BarBackgroundColor);

        // The maximum text width, subtracting horizontal padding
        var maxWidth = barWidth - barHPadding * 2;

        // Cache window rectangle to skip off-screen entities
        var windowRect = GameController.Window.GetWindowRectangle();
        var windowWidth = windowRect.Width;
        var windowHeight = windowRect.Height;

        foreach (var entity in GetMonsters())
        {
            // Convert world coords to screen coords once per entity
            var entityScreenCoords = Camera.WorldToScreen(entity.Pos);

            // Skip if off-screen
            if (entityScreenCoords.X < 0 || entityScreenCoords.X > windowWidth ||
                entityScreenCoords.Y < 0 || entityScreenCoords.Y > windowHeight)
            {
                continue;
            }

            // Get Buffs component
            if (!entity.TryGetComponent<Buffs>(out var entityBuffs))
                continue;

            // Find matching auras in one pass
            var matchedAuras = Settings.AuraList
                .Where(auraSettings => auraSettings.Enabled &&
                                       entityBuffs.BuffsList.Any(buff =>
                                           string.Equals(buff.Name, auraSettings.Name, StringComparison.Ordinal)))
                .ToList();

            if (!matchedAuras.Any())
                continue;

            // Center the bar on the monster's X position
            entityScreenCoords.X -= (int)(barWidth / 2);

            foreach (var auraSettings in matchedAuras)
            {
                var entityBuff = entityBuffs.BuffsList
                    .First(buff => string.Equals(buff.Name, auraSettings.Name, StringComparison.Ordinal));

                var displayText = auraSettings.DisplayName;
                var stacksText = entityBuff.BuffStacks > 0 ? $" ({entityBuff.BuffStacks + 1})" : "";
                var timerText = entityBuff.Timer > 0 && entityBuff.Timer < 99
                    ? $" {entityBuff.Timer:F1}s"
                    : "";

                // Combine for measuring & displaying
                var combinedText = displayText + stacksText + timerText;
                var textSize = Graphics.MeasureText(combinedText);

                // Truncate if text exceeds the bar width
                if (textSize.X > maxWidth)
                {
                    var ellipsis = "...";
                    var ellipsisSize = Graphics.MeasureText(ellipsis + stacksText + timerText);
                    var availableWidth = maxWidth - ellipsisSize.X;

                    // Keep chopping from the end until it fits
                    while (Graphics.MeasureText(displayText + timerText).X > availableWidth &&
                           displayText.Length > 0)
                    {
                        displayText = displayText.Substring(0, displayText.Length - 1);
                    }

                    // Re-append ellipsis and measure again
                    displayText += ellipsis;
                    combinedText = displayText + stacksText + timerText;
                    textSize = Graphics.MeasureText(combinedText);
                }

                // Calculate bar fill percentage
                float barWidthMultiplier = 1.0f;
                if (entityBuff.Timer > 0 && entityBuff.Timer < 99 && entityBuff.MaxTime < 99)
                {
                    barWidthMultiplier = entityBuff.Timer / entityBuff.MaxTime;
                }

                // Draw bar background
                var bgPos = entityScreenCoords;
                var bgSize = new Vector2(barWidth, textSize.Y + barVPadding * 2);
                Graphics.DrawBox(bgPos, bgPos + bgSize, barBackgroundColor);

                // Draw the colored fill portion
                var barPos = new Vector2(bgPos.X + barInset, bgPos.Y + barInset);
                var barSize = new Vector2(barWidth - barInset * 2, bgSize.Y - barInset * 2);
                int fillWidth = (int)(barSize.X * barWidthMultiplier);
                Graphics.DrawBox(barPos, new Vector2(barPos.X + fillWidth, barPos.Y + barSize.Y),
                                 ImGuiUtils.Vector4ToColor(auraSettings.BarColor));

                // Draw text, with shadow for visibility
                var textPos = new Vector2(
                    bgPos.X + barHPadding,
                    bgPos.Y - 1 + ((bgSize.Y - textSize.Y) / 2)
                );
                Graphics.DrawText(combinedText, textPos + shadowOffset, shadowColor);
                Graphics.DrawText(combinedText, textPos, ImGuiUtils.Vector4ToColor(auraSettings.TextColor));

                // Move downward for the next aura
                entityScreenCoords.Y += textSize.Y + barVPadding * 2 - 1 + barSpacing;
            }
        }
    }


    public void UpdateCaptureBuffs() {
        var capturedBuffs = new StringBuilder();
        foreach (var seenBuff in Settings.SeenBuffs) {
            capturedBuffs.AppendLine($"Name: {seenBuff.Name} | DisplayName: {seenBuff.DisplayName} | Duration: {seenBuff.Duration}");
        }
        _capturedBuffs = capturedBuffs.ToString();
    }

    public void CaptureBuffs()
    {
        // Cache the entities locally
        foreach (var entity in GetMonsters())
        {
            if (!entity.TryGetComponent<Buffs>(out var entityBuffs))
                continue;

            foreach (var buff in entityBuffs.BuffsList)
            {
                if (!_seenBuffNames.Contains(buff.Name))
                {
                    var seenBuff = new SeenBuff(buff.Name, buff.DisplayName, buff.MaxTime);
                    Settings.SeenBuffs.Add(seenBuff);
                    _seenBuffNames.Add(buff.Name);
                    UpdateCaptureBuffs();

                    if (Settings.CaptureBuffsSave && !string.IsNullOrEmpty(seenBuff.DisplayName))
                        Settings.AddAura(seenBuff.Name, seenBuff.DisplayName, _defaultTextColor, _defaultBarColor);
                }
            }
        }
    }

    public void Snapshot() {
        var snapshot = new StringBuilder();

        foreach (var entity in GameController.Entities)
        {
            if ((entity.Type == EntityType.Monster) || (entity.Type == EntityType.Player)) {

                if ((entity.Type == EntityType.Monster) &&
                    (entity.Rarity != MonsterRarity.Rare && entity.Rarity != MonsterRarity.Magic && entity.Rarity != MonsterRarity.Unique) ||
                    (entity.Rarity == MonsterRarity.Unique && !Settings.SnapshotUnique) ||
                    (entity.Rarity == MonsterRarity.Rare && !Settings.SnapshotRare) ||
                    (entity.Rarity == MonsterRarity.Magic && !Settings.SnapshotMagic) ||
                    !entity.TryGetComponent<Render>(out var entityRender) ||
                    !entity.TryGetComponent<Buffs>(out var entityBuffs))
                                    continue;

                snapshot.AppendLine($"--| {entity.RenderName} | Rarity:{entity.Rarity}");
                foreach (var buff in entityBuffs.BuffsList)
                {
                    var buffInfo = $"----| Buff: {buff.Name}";
                    if (buff.MaxTime > 0 && buff.MaxTime < 999)
                    {
                        buffInfo += $" | Duration: {buff.MaxTime}";
                    }
                    if (buff.Timer > 0 && buff.Timer < 999)
                    {
                        buffInfo += $" | Left: {buff.Timer}";
                    }
                    snapshot.AppendLine(buffInfo);
                }
            }
        }
        _snapshot = snapshot.ToString();
    }

    private bool IsValidMonster(Entity entity) {
        return
            entity.Type == EntityType.Monster &&
            entity.IsValid &&
            !entity.IsHidden &&
            entity.Rarity != MonsterRarity.White &&
            entity.IsAlive &&
            ((entity.Rarity == MonsterRarity.Magic && Settings.TrackMagic) ||
             (entity.Rarity == MonsterRarity.Rare && Settings.TrackRares) ||
             (entity.Rarity == MonsterRarity.Unique && Settings.TrackUniques));
    }

    private IEnumerable<Entity> GetMonsters() {
        return GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster].Where(IsValidMonster);
    }
}
