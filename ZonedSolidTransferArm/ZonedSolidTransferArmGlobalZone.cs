using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace ZonedSolidTransferArm;

public static class ZonedSolidTransferArmGlobalZone
{
    public const float TemporaryConstructionZoneDuration = 60f;
    public const float TemporaryClearZoneDuration = 300f;

    [Flags]
    private enum TemporaryZoneSource
    {
        None = 0,
        Construction = 1,
        Clear = 2
    }

    private readonly struct TemporaryZoneCell
    {
        public readonly TemporaryZoneSource Sources;
        public readonly float ExpireTime;

        public TemporaryZoneCell(TemporaryZoneSource sources, float expireTime)
        {
            Sources = sources;
            ExpireTime = expireTime;
        }
    }

    private static readonly ConcurrentDictionary<int, byte> Cells = new();
    private static readonly ConcurrentDictionary<int, TemporaryZoneCell> TemporaryCells = new();
    private static readonly FieldInfo ConstructableFetchListField = AccessTools.Field(typeof(Constructable), "fetchList");
    private static bool temporaryConstructionZonesEnabled = true;
    private static bool temporaryClearZonesEnabled = true;
    private static bool temporarySyncScheduled;
    private static int revision;

    public static IEnumerable<int> MarkedCells
    {
        get
        {
            foreach (int cell in Cells.Keys)
            {
                yield return cell;
            }
            foreach (int cell in TemporaryCells.Keys)
            {
                yield return cell;
            }
        }
    }

    public static int CellCount => Cells.Count + TemporaryCells.Count;

    public static int Revision => revision;

    public static bool TemporaryConstructionZonesEnabled => temporaryConstructionZonesEnabled;

    public static bool TemporaryClearZonesEnabled => temporaryClearZonesEnabled;

    public static bool ContainsCell(int cell)
    {
        return Cells.ContainsKey(cell) || TemporaryCells.ContainsKey(cell);
    }

    public static bool ContainsPersistentCell(int cell)
    {
        return Cells.ContainsKey(cell);
    }

    public static void AddCell(int cell)
    {
        Cells[cell] = 0;
        NotifyChanged();
    }

    public static void AddCells(IEnumerable<int> cells)
    {
        bool changed = false;
        foreach (int cell in cells)
        {
            if (Grid.IsValidCell(cell))
            {
                Cells[cell] = 0;
                changed = true;
            }
        }
        if (changed)
        {
            NotifyChanged();
        }
    }

    public static void RemoveCell(int cell)
    {
        if (Cells.TryRemove(cell, out _))
        {
            NotifyChanged();
        }
    }

    public static void RemoveCells(IEnumerable<int> cells)
    {
        bool changed = false;
        foreach (int cell in cells)
        {
            if (Cells.TryRemove(cell, out _))
            {
                changed = true;
            }
        }
        if (changed)
        {
            NotifyChanged();
        }
    }

    public static void Clear()
    {
        Cells.Clear();
        TemporaryCells.Clear();
        temporaryConstructionZonesEnabled = true;
        temporaryClearZonesEnabled = true;
        temporarySyncScheduled = false;
        revision++;
    }

    public static void AddTemporaryConstructionCells(IEnumerable<int> cells)
    {
        if (!temporaryConstructionZonesEnabled)
        {
            return;
        }

        AddTemporaryCells(cells, TemporaryZoneSource.Construction, TemporaryConstructionZoneDuration);
    }

    public static void AddTemporaryClearCell(int cell)
    {
        if (!temporaryClearZonesEnabled)
        {
            return;
        }

        AddTemporaryCells(new[] { cell }, TemporaryZoneSource.Clear, TemporaryClearZoneDuration);
    }

    public static void SetTemporaryConstructionZonesEnabled(bool enabled)
    {
        temporaryConstructionZonesEnabled = enabled;
    }

    public static void SetTemporaryClearZonesEnabled(bool enabled)
    {
        temporaryClearZonesEnabled = enabled;
    }

    private static void AddTemporaryCells(IEnumerable<int> cells, TemporaryZoneSource source, float duration)
    {
        float expireTime = GameClock.Instance.GetTime() + duration;
        bool changed = false;
        foreach (int cell in cells)
        {
            if (!Grid.IsValidCell(cell))
            {
                continue;
            }

            TemporaryCells.AddOrUpdate(
                cell,
                new TemporaryZoneCell(source, expireTime),
                (_, oldValue) => new TemporaryZoneCell(
                    oldValue.Sources | source,
                    Math.Max(oldValue.ExpireTime, expireTime)));
            changed = true;
        }
        if (changed)
        {
            ScheduleTemporarySync();
        }
    }

    public static void SyncTemporaryCells()
    {
        temporarySyncScheduled = false;
        RemoveExpiredTemporaryCells();
        ZonedSolidTransferArmControl.OnGlobalZoneChanged();
        revision++;
    }

    private static void ScheduleTemporarySync()
    {
        if (temporarySyncScheduled || GameScheduler.Instance == null)
        {
            return;
        }

        temporarySyncScheduled = true;
        GameScheduler.Instance.Schedule(
            "ZonedSolidTransferArm.TemporaryGlobalZoneSync",
            1f,
            _ => SyncTemporaryCells());
    }

    public static void RemoveExpiredTemporaryCells()
    {
        bool changed = false;
        foreach (KeyValuePair<int, TemporaryZoneCell> entry in TemporaryCells)
        {
            TemporaryZoneSource activeSources = GetActiveSources(entry.Key, entry.Value.Sources);
            if (activeSources == TemporaryZoneSource.None)
            {
                if (TemporaryCells.TryRemove(entry.Key, out _))
                {
                    changed = true;
                }
            }
            else if (activeSources != entry.Value.Sources)
            {
                TemporaryCells[entry.Key] = new TemporaryZoneCell(activeSources, entry.Value.ExpireTime);
                changed = true;
            }
        }
        if (changed)
        {
            NotifyChanged();
        }
    }

    private static void NotifyChanged()
    {
        revision++;
        ZonedSolidTransferArmControl.OnGlobalZoneChanged();
    }

    private static TemporaryZoneSource GetActiveSources(int cell, TemporaryZoneSource sources)
    {
        TemporaryZoneSource activeSources = TemporaryZoneSource.None;
        if ((sources & TemporaryZoneSource.Construction) != 0 && HasConstructableNeedingMaterialsAtCell(cell))
        {
            activeSources |= TemporaryZoneSource.Construction;
        }
        if ((sources & TemporaryZoneSource.Clear) != 0 && HasMarkedClearableAtCell(cell))
        {
            activeSources |= TemporaryZoneSource.Clear;
        }
        return activeSources;
    }

    private static bool HasConstructableNeedingMaterialsAtCell(int cell)
    {
        if (!Grid.IsValidCell(cell))
        {
            return false;
        }

        for (int layer = 0; layer < (int)ObjectLayer.NumLayers; layer++)
        {
            GameObject gameObject = Grid.Objects[cell, layer];
            if (gameObject == null)
            {
                continue;
            }

            Constructable constructable = gameObject.GetComponent<Constructable>();
            if (constructable != null && ConstructableFetchListField.GetValue(constructable) != null)
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasMarkedClearableAtCell(int cell)
    {
        GameObject gameObject = Grid.Objects[cell, 3];
        if (gameObject == null)
        {
            return false;
        }

        ObjectLayerListItem item = gameObject.GetComponent<Pickupable>().objectLayerListItem;
        while (item != null)
        {
            Pickupable pickupable = item.pickupable;
            if (pickupable != null &&
                pickupable.KPrefabID.HasTag(GameTags.Garbage) &&
                pickupable.Clearable != null &&
                pickupable.Clearable.isClearable)
            {
                return true;
            }
            item = item.nextItem;
        }
        return false;
    }

    public static string Serialize()
    {
        return string.Join("_", Cells.Keys);
    }

    public static string SerializeTemporary()
    {
        RemoveExpiredTemporaryCells();
        List<string> values = new();
        foreach (KeyValuePair<int, TemporaryZoneCell> entry in TemporaryCells)
        {
            values.Add(entry.Key.ToString(CultureInfo.InvariantCulture) +
                ":" +
                ((int)entry.Value.Sources).ToString(CultureInfo.InvariantCulture) +
                ":" +
                entry.Value.ExpireTime.ToString(CultureInfo.InvariantCulture));
        }
        return string.Join("_", values);
    }

    public static void Load(string data)
    {
        Clear();
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        foreach (string value in data.Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(value, out int cell))
            {
                Cells[cell] = 0;
            }
        }
        NotifyChanged();
    }

    public static void LoadTemporary(string data)
    {
        TemporaryCells.Clear();
        temporarySyncScheduled = false;
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        foreach (string value in data.Split(new[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = value.Split(new[] { ':' }, 3);
            if (parts.Length == 2 &&
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int oldCell) &&
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float oldExpireTime))
            {
                TemporaryCells[oldCell] = new TemporaryZoneCell(TemporaryZoneSource.Construction, oldExpireTime);
            }
            else if (parts.Length == 3 &&
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int cell) &&
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int sources) &&
                float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float expireTime))
            {
                TemporaryCells[cell] = new TemporaryZoneCell((TemporaryZoneSource)sources, expireTime);
            }
        }
        RemoveExpiredTemporaryCells();
        NotifyChanged();
    }

    public static void RemoveInvalidCells()
    {
        bool changed = false;
        foreach (int cell in Cells.Keys)
        {
            if (!Grid.IsValidCell(cell) && Cells.TryRemove(cell, out _))
            {
                changed = true;
            }
        }
        foreach (int cell in TemporaryCells.Keys)
        {
            if (!Grid.IsValidCell(cell) && TemporaryCells.TryRemove(cell, out _))
            {
                changed = true;
            }
        }
        if (changed)
        {
            NotifyChanged();
        }
    }
}

[SerializationConfig(MemberSerialization.OptIn)]
public class ZonedSolidTransferArmGlobalZoneSaveData : KMonoBehaviour, ISaveLoadable
{
    [Serialize]
    private string globalZoneCells = "";
    [Serialize]
    private string temporaryGlobalZoneCells = "";
    [Serialize]
    private bool temporaryConstructionZonesEnabled = true;
    [Serialize]
    private bool temporaryClearZonesEnabled = true;
    private SchedulerHandle cleanupHandle;

    [OnSerializing]
    private void OnSerializing()
    {
        globalZoneCells = ZonedSolidTransferArmGlobalZone.Serialize();
        temporaryGlobalZoneCells = ZonedSolidTransferArmGlobalZone.SerializeTemporary();
        temporaryConstructionZonesEnabled = ZonedSolidTransferArmGlobalZone.TemporaryConstructionZonesEnabled;
        temporaryClearZonesEnabled = ZonedSolidTransferArmGlobalZone.TemporaryClearZonesEnabled;
    }

    [OnDeserialized]
    private void OnDeserialized()
    {
        ZonedSolidTransferArmGlobalZone.Load(globalZoneCells);
        ZonedSolidTransferArmGlobalZone.LoadTemporary(temporaryGlobalZoneCells);
        ZonedSolidTransferArmGlobalZone.SetTemporaryConstructionZonesEnabled(temporaryConstructionZonesEnabled);
        ZonedSolidTransferArmGlobalZone.SetTemporaryClearZonesEnabled(temporaryClearZonesEnabled);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        ScheduleCleanup();
    }

    protected override void OnCleanUp()
    {
        cleanupHandle.ClearScheduler();
        base.OnCleanUp();
    }

    private void ScheduleCleanup()
    {
        if (GameScheduler.Instance != null)
        {
            cleanupHandle = GameScheduler.Instance.Schedule(
                "ZonedSolidTransferArm.TemporaryGlobalZoneCleanup",
                1f,
                CleanupTemporaryCells);
        }
    }

    private void CleanupTemporaryCells(object data)
    {
        ZonedSolidTransferArmGlobalZone.RemoveExpiredTemporaryCells();
        ScheduleCleanup();
    }
}

public class ZonedSolidTransferArmGlobalZoneOverlay : OverlayModes.Mode
{
    public static readonly HashedString ID = "ZonedSolidTransferArmGlobalZone";

    public override HashedString ViewMode()
    {
        return ID;
    }

    public override void Enable()
    {
        ZonedSolidTransferArmGlobalZone.RemoveInvalidCells();
    }

    public override string GetSoundName()
    {
        return "SuitRequired";
    }

    public static Color GetCellColor(SimDebugView instance, int cell)
    {
        return ZonedSolidTransferArmGlobalZone.ContainsCell(cell)
            ? ZonedSolidTransferArmControl.ZoneColor
            : Color.black;
    }

    public override List<LegendEntry> GetCustomLegendData()
    {
        return new List<LegendEntry>
        {
            new(
                ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.OVERLAYS.GLOBALZONE.MARKED),
                ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.OVERLAYS.GLOBALZONE.TOOLTIP),
                ZonedSolidTransferArmControl.ZoneColor)
        };
    }
}

public class ZonedSolidTransferArmGlobalZoneTool : DragTool
{
    public enum EditMode
    {
        Add,
        Remove
    }

    private static ZonedSolidTransferArmGlobalZoneTool instance;
    private static EditMode mode;
    internal const string AddParameter = "ZONEDSOLIDTRANSFERARM_GLOBALZONE_ADD";
    internal const string RemoveParameter = "ZONEDSOLIDTRANSFERARM_GLOBALZONE_REMOVE";
    internal const string TemporaryConstructionParameter = "ZONEDSOLIDTRANSFERARM_GLOBALZONE_TEMPORARY_CONSTRUCTION";
    internal const string TemporaryClearParameter = "ZONEDSOLIDTRANSFERARM_GLOBALZONE_TEMPORARY_CLEAR";
    private static readonly Color RemoveZoneColor = new(1f, 0.08f, 0.05f, 0.9f);
    private ToolParameterMenu.ToggleData addParameter;
    private ToolParameterMenu.ToggleData removeParameter;
    private ToolParameterMenu.ToggleData temporaryConstructionParameter;
    private ToolParameterMenu.ToggleData temporaryClearParameter;

    public static void SetEditMode(EditMode editMode)
    {
        mode = editMode;
        instance?.ApplyActiveColor();
    }

    public static void ActivateRemoveMode()
    {
        SetEditMode(EditMode.Remove);
        PlayerController.Instance.ActivateTool(Instance);
    }

    private static ZonedSolidTransferArmGlobalZoneTool Instance => instance;

    protected override void OnPrefabInit()
    {
        instance = this;
        CopyDragToolVisuals();
        base.OnPrefabInit();
        visualizer = CreateVisualizer();
        interceptNumberKeysForPriority = true;
        viewMode = ZonedSolidTransferArmGlobalZoneOverlay.ID;
    }

    protected override void OnActivateTool()
    {
        base.OnActivateTool();
        PopulateParameterMenu();
        ApplyActiveColor();
        SetMode(Mode.Box);
    }

    protected override void OnDeactivateTool(InterfaceTool newTool)
    {
        ToolParameterMenu menu = ToolMenu.Instance?.toolParameterMenu;
        if (menu != null)
        {
            menu.onParametersChanged -= OnParametersChanged;
            menu.ClearMenu();
        }
        base.OnDeactivateTool(newTool);
    }

    protected override void OnDragTool(int cell, int distFromOrigin)
    {
        if (!Grid.IsValidCell(cell) || !Grid.IsVisible(cell))
        {
            return;
        }

        if (mode == EditMode.Remove)
        {
            ZonedSolidTransferArmGlobalZone.RemoveCell(cell);
        }
        else
        {
            ZonedSolidTransferArmGlobalZone.AddCell(cell);
        }
    }

    public override void GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors)
    {
        colors = new HashSet<ToolMenu.CellColorData>();
        foreach (int cell in ZonedSolidTransferArmGlobalZone.MarkedCells)
        {
            colors.Add(new ToolMenu.CellColorData(cell, ZonedSolidTransferArmControl.ZoneColor));
        }
    }

    private void CopyDragToolVisuals()
    {
        DisinfectTool source = DisinfectTool.Instance;
        if (source == null)
        {
            return;
        }

        Traverse.Create(this)
            .Field("areaVisualizer")
            .SetValue(Traverse.Create(source).Field<GameObject>("areaVisualizer").Value);
        Traverse.Create(this)
            .Field("areaVisualizerTextPrefab")
            .SetValue(Traverse.Create(source).Field<GameObject>("areaVisualizerTextPrefab").Value);
        Traverse.Create(this).Field("areaColour").SetValue((Color32)GetActiveColor());

        Texture2D sourceCursor = Traverse.Create(source).Field<Texture2D>("cursor").Value;
        cursor = sourceCursor;
        Traverse.Create(this).Field("boxCursor").SetValue(sourceCursor);
    }

    private static GameObject CreateVisualizer()
    {
        GameObject root = new("ZonedSolidTransferArmGlobalZoneToolVisualizer");
        root.SetActive(false);

        GameObject offset = new("ZonedSolidTransferArmGlobalZoneToolCursor");
        offset.transform.SetParent(root.transform);
        offset.transform.SetLocalPosition(new Vector3(0f, Grid.HalfCellSizeInMeters, 0f));
        offset.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
        offset.SetLayerRecursively(LayerMask.NameToLayer("Overlay"));

        SpriteRenderer renderer = offset.AddComponent<SpriteRenderer>();
        renderer.sprite = Assets.GetSprite("cursorIcon") ?? Assets.GetSprite("icon_action_store");
        renderer.color = ZonedSolidTransferArmControl.ZoneColor;
        renderer.sortingOrder = 10;

        return root;
    }

    private static Color GetActiveColor()
    {
        return mode == EditMode.Remove ? RemoveZoneColor : ZonedSolidTransferArmControl.ZoneColor;
    }

    private void PopulateParameterMenu()
    {
        ToolParameterMenu menu = ToolMenu.Instance?.toolParameterMenu;
        if (menu == null)
        {
            return;
        }

        addParameter = new ToolParameterMenu.ToggleData(
            AddParameter,
            mode == EditMode.Add ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off);
        removeParameter = new ToolParameterMenu.ToggleData(
            RemoveParameter,
            mode == EditMode.Remove ? ToolParameterMenu.ToggleState.On : ToolParameterMenu.ToggleState.Off);
            temporaryConstructionParameter = new ToolParameterMenu.ToggleData(
                TemporaryConstructionParameter,
                ZonedSolidTransferArmGlobalZone.TemporaryConstructionZonesEnabled
                    ? ToolParameterMenu.ToggleState.On
                    : ToolParameterMenu.ToggleState.Off,
                true);
            temporaryClearParameter = new ToolParameterMenu.ToggleData(
                TemporaryClearParameter,
                ZonedSolidTransferArmGlobalZone.TemporaryClearZonesEnabled
                    ? ToolParameterMenu.ToggleState.On
                    : ToolParameterMenu.ToggleState.Off,
                true);
        menu.onParametersChanged -= OnParametersChanged;
        menu.PopulateMenu(new[]
        {
            addParameter,
            removeParameter,
            temporaryConstructionParameter,
            temporaryClearParameter
        });
        menu.onParametersChanged += OnParametersChanged;
    }

    private void OnParametersChanged()
    {
        if (temporaryConstructionParameter != null)
        {
            ZonedSolidTransferArmGlobalZone.SetTemporaryConstructionZonesEnabled(temporaryConstructionParameter.IsOn);
        }
        if (temporaryClearParameter != null)
        {
            ZonedSolidTransferArmGlobalZone.SetTemporaryClearZonesEnabled(temporaryClearParameter.IsOn);
        }

        if (removeParameter is { IsOn: true })
        {
            SetEditMode(EditMode.Remove);
        }
        else if (addParameter is { IsOn: true })
        {
            SetEditMode(EditMode.Add);
        }
    }

    private void ApplyActiveColor()
    {
        Color color = GetActiveColor();
        Traverse.Create(this).Field("areaColour").SetValue((Color32)color);
        if (areaVisualizerSpriteRenderer != null)
        {
            areaVisualizerSpriteRenderer.color = color;
            Renderer renderer = areaVisualizerSpriteRenderer.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
        if (visualizer != null)
        {
            SpriteRenderer renderer = visualizer.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = color;
            }
        }
    }
}

