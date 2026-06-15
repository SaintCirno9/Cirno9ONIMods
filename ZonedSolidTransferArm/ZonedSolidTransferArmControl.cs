using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using KSerialization;
using UnityEngine;

namespace ZonedSolidTransferArm;

[SerializationConfig(MemberSerialization.OptIn)]
public class ZonedSolidTransferArmControl : KMonoBehaviour
{
    public static readonly Color ZoneColor = new Color(0.45f, 0.85f, 0.78f, 0.35f);

    private static readonly ConcurrentDictionary<SolidTransferArm, HashSet<int>> ZoneSnapshots = new();
    private static readonly ConcurrentDictionary<ZonedSolidTransferArmControl, byte> Controls = new();
    private readonly HashSet<int> cells = new();

    [Serialize]
    private bool zoneModeEnabled;

    [Serialize]
    private bool useGlobalZone = true;

    [Serialize]
    private string serializedCells = "";

    private bool savedRangeVisualizer;
    private Vector2I savedOriginOffset;
    private Vector2I savedRangeMin;
    private Vector2I savedRangeMax;
    private Vector2I savedTexSize;
    private bool savedTestLineOfSight;
    private bool savedBlockingTileVisible;
    private Func<int, bool> savedBlockingCb;

    [MyCmpReq]
    public SolidTransferArm solidTransferArm;

    [MyCmpReq]
    public RangeVisualizer rangeVisualizer;

    [MyCmpGet]
    public Rotatable rotatable;

    [MyCmpAdd]
    public CopyBuildingSettings copyBuildingSettings;

    public static bool IsCellAllowed(SolidTransferArm arm, int cell)
    {
        if (!ZoneSnapshots.TryGetValue(arm, out HashSet<int> snapshot))
        {
            return true;
        }

        return snapshot.Count > 0 && snapshot.Contains(cell);
    }

    public static bool TryGetZoneSnapshot(SolidTransferArm arm, out HashSet<int> snapshot)
    {
        return ZoneSnapshots.TryGetValue(arm, out snapshot);
    }

    public bool UsesOnlyGlobalZone()
    {
        return zoneModeEnabled && useGlobalZone && cells.Count == 0;
    }

    public bool UsesGlobalZone()
    {
        return zoneModeEnabled && useGlobalZone;
    }

    public IEnumerable<int> Cells => cells;

    public int CellCount => cells.Count;

    public static void OnGlobalZoneChanged()
    {
        foreach (ZonedSolidTransferArmControl control in Controls.Keys)
        {
            control.SyncSnapshot();
            control.SyncRangeVisualizer();
        }
    }

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        Subscribe(-905833192, OnCopySettings);
        Subscribe(493375141, OnRefreshUserMenu);
        Subscribe(-1643076535, OnRotated);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        Controls[this] = 0;
        LoadCells();
        if (zoneModeEnabled)
        {
            SaveRangeVisualizer();
        }
        SyncSnapshot();
        SyncRangeVisualizer();
    }

    protected override void OnCleanUp()
    {
        Controls.TryRemove(this, out _);
        ZoneSnapshots.TryRemove(solidTransferArm, out _);
        RestoreRangeVisualizer();
        base.OnCleanUp();
    }

    [OnSerializing]
    private void OnSerializing()
    {
        serializedCells = string.Join("_", cells);
    }

    [OnDeserialized]
    private void OnDeserialized()
    {
        LoadCells();
        SyncSnapshot();
    }

    public void AddCell(int cell)
    {
        if (cells.Add(cell))
        {
            SyncSnapshot();
            SyncRangeVisualizer();
        }
    }

    public void RemoveCell(int cell)
    {
        if (cells.Remove(cell))
        {
            SyncSnapshot();
            SyncRangeVisualizer();
        }
    }

    public bool ContainsCell(int cell)
    {
        return cells.Contains(cell);
    }

    private void OnRefreshUserMenu(object data)
    {
        Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
            "action_switch_toggle",
            ZonedSolidTransferArmStrings.Text(zoneModeEnabled
                ? ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.DISABLEZONEBUTTON
                : ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ENABLEZONEBUTTON),
            ToggleZoneMode,
            global::Action.NumActions,
            null,
            null,
            null,
            ZonedSolidTransferArmStrings.Text(zoneModeEnabled
                ? ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.DISABLEZONEBUTTONTOOLTIP
                : ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ENABLEZONEBUTTONTOOLTIP)), 0.45f);
        if (!zoneModeEnabled)
        {
            return;
        }

        Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
            "action_move_to_storage",
            ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ADDZONEBUTTON),
            ActivateAddZoneTool,
            global::Action.NumActions,
            null,
            null,
            null,
            ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ADDZONEBUTTONTOOLTIP)), 0.45f);
        Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
            "action_cancel",
            ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.REMOVEZONEBUTTON),
            ActivateRemoveZoneTool,
            global::Action.NumActions,
            null,
            null,
            null,
            ZonedSolidTransferArmStrings.Text(ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.REMOVEZONEBUTTONTOOLTIP)), 0.45f);
        Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
            "action_switch_toggle",
            ZonedSolidTransferArmStrings.Text(useGlobalZone
                ? ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.DISABLEGLOBALZONEBUTTON
                : ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ENABLEGLOBALZONEBUTTON),
            ToggleGlobalZone,
            global::Action.NumActions,
            null,
            null,
            null,
            ZonedSolidTransferArmStrings.Text(useGlobalZone
                ? ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.DISABLEGLOBALZONEBUTTONTOOLTIP
                : ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ENABLEGLOBALZONEBUTTONTOOLTIP)), 0.45f);
    }

    private void ToggleZoneMode()
    {
        if (!zoneModeEnabled)
        {
            SaveRangeVisualizer();
        }
        zoneModeEnabled = !zoneModeEnabled;
        SyncSnapshot();
        if (zoneModeEnabled)
        {
            SyncRangeVisualizer();
        }
        else
        {
            RestoreRangeVisualizer();
        }
        Game.Instance.Trigger(1980521255, gameObject);
    }

    private void ActivateAddZoneTool()
    {
        ActivateZoneTool(ZonedSolidTransferArmZoneTool.EditMode.Add);
    }

    private void ActivateRemoveZoneTool()
    {
        ActivateZoneTool(ZonedSolidTransferArmZoneTool.EditMode.Remove);
    }

    private void ActivateZoneTool(ZonedSolidTransferArmZoneTool.EditMode mode)
    {
        ZonedSolidTransferArmZoneTool.SetTarget(this, mode);
        PlayerController.Instance.ActivateTool(ZonedSolidTransferArmZoneTool.Instance);
    }

    private void ToggleGlobalZone()
    {
        useGlobalZone = !useGlobalZone;
        SyncSnapshot();
        SyncRangeVisualizer();
        Game.Instance.Trigger(1980521255, gameObject);
    }

    private void OnCopySettings(object data)
    {
        if (data is not GameObject go || go.GetComponent<ZonedSolidTransferArmControl>() is not { } sourceControl)
        {
            return;
        }

        bool wasEnabled = zoneModeEnabled;
        zoneModeEnabled = sourceControl.zoneModeEnabled;
        if (!wasEnabled && zoneModeEnabled)
        {
            SaveRangeVisualizer();
        }
        else if (wasEnabled && !zoneModeEnabled)
        {
            RestoreRangeVisualizer();
        }
        useGlobalZone = sourceControl.useGlobalZone;
        cells.Clear();
        foreach (int cell in sourceControl.Cells)
        {
            cells.Add(cell);
        }
        SyncSnapshot();
        SyncRangeVisualizer();
    }

    private void OnRotated(object data)
    {
        SyncRangeVisualizer();
    }

    private void LoadCells()
    {
        cells.Clear();
        if (string.IsNullOrEmpty(serializedCells))
        {
            return;
        }

        foreach (string value in serializedCells.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(value, out int cell))
            {
                cells.Add(cell);
            }
        }
    }

    private void SyncSnapshot()
    {
        if (solidTransferArm == null)
        {
            return;
        }

        if (zoneModeEnabled)
        {
            ZoneSnapshots[solidTransferArm] = CreateEffectiveCells();
        }
        else
        {
            ZoneSnapshots.TryRemove(solidTransferArm, out _);
        }
    }

    private void SyncRangeVisualizer()
    {
        if (rangeVisualizer == null || solidTransferArm == null || !zoneModeEnabled)
        {
            return;
        }

        rangeVisualizer.OriginOffset = Vector2I.zero;
        rangeVisualizer.TestLineOfSight = false;
        rangeVisualizer.BlockingCb = IsRangeVisualizerCellBlocked;
        rangeVisualizer.BlockingTileVisible = false;
        HashSet<int> effectiveCells = CreateEffectiveCells();
        if (effectiveCells.Count == 0)
        {
            rangeVisualizer.RangeMin = Vector2I.zero;
            rangeVisualizer.RangeMax = new Vector2I(-1, -1);
            rangeVisualizer.TexSize = new Vector2I(1, 1);
            return;
        }

        Grid.CellToXY(Grid.PosToCell(gameObject), out int originX, out int originY);
        int minLocalX = int.MaxValue;
        int minLocalY = int.MaxValue;
        int maxLocalX = int.MinValue;
        int maxLocalY = int.MinValue;
        foreach (int cell in effectiveCells)
        {
            Grid.CellToXY(cell, out int x, out int y);
            Vector2I localOffset = ToLocalOffset(x - originX, y - originY);
            minLocalX = Math.Min(minLocalX, localOffset.x);
            minLocalY = Math.Min(minLocalY, localOffset.y);
            maxLocalX = Math.Max(maxLocalX, localOffset.x);
            maxLocalY = Math.Max(maxLocalY, localOffset.y);
        }

        rangeVisualizer.RangeMin = new Vector2I(minLocalX, minLocalY);
        rangeVisualizer.RangeMax = new Vector2I(maxLocalX, maxLocalY);
        GetRotatedBounds(rangeVisualizer.RangeMin, rangeVisualizer.RangeMax, out Vector2I rotatedMin, out Vector2I rotatedMax);
        rangeVisualizer.TexSize = new Vector2I(rotatedMax.x - rotatedMin.x + 1, rotatedMax.y - rotatedMin.y + 1);
    }

    private bool IsRangeVisualizerCellBlocked(int cell)
    {
        return !cells.Contains(cell) && !ZonedSolidTransferArmGlobalZone.ContainsCell(cell);
    }

    private HashSet<int> CreateEffectiveCells()
    {
        HashSet<int> effectiveCells = new(cells);
        if (!useGlobalZone)
        {
            return effectiveCells;
        }

        foreach (int cell in ZonedSolidTransferArmGlobalZone.MarkedCells)
        {
            effectiveCells.Add(cell);
        }
        return effectiveCells;
    }

    private Vector2I ToLocalOffset(int worldOffsetX, int worldOffsetY)
    {
        Orientation orientation = rotatable == null ? Orientation.Neutral : rotatable.GetOrientation();
        return orientation switch
        {
            Orientation.R90 => new Vector2I(-worldOffsetY, worldOffsetX),
            Orientation.R180 => new Vector2I(-worldOffsetX, -worldOffsetY),
            Orientation.R270 => new Vector2I(worldOffsetY, -worldOffsetX),
            Orientation.FlipH => new Vector2I(-worldOffsetX, worldOffsetY),
            Orientation.FlipV => new Vector2I(worldOffsetX, -worldOffsetY),
            _ => new Vector2I(worldOffsetX, worldOffsetY)
        };
    }

    private Vector2I ToWorldOffset(int localOffsetX, int localOffsetY)
    {
        Orientation orientation = rotatable == null ? Orientation.Neutral : rotatable.GetOrientation();
        return orientation switch
        {
            Orientation.R90 => new Vector2I(localOffsetY, -localOffsetX),
            Orientation.R180 => new Vector2I(-localOffsetX, -localOffsetY),
            Orientation.R270 => new Vector2I(-localOffsetY, localOffsetX),
            Orientation.FlipH => new Vector2I(-localOffsetX, localOffsetY),
            Orientation.FlipV => new Vector2I(localOffsetX, -localOffsetY),
            _ => new Vector2I(localOffsetX, localOffsetY)
        };
    }

    private void GetRotatedBounds(Vector2I localMin, Vector2I localMax, out Vector2I rotatedMin, out Vector2I rotatedMax)
    {
        Vector2I rotatedMinOffset = ToWorldOffset(localMin.x, localMin.y);
        Vector2I rotatedMaxOffset = ToWorldOffset(localMax.x, localMax.y);
        rotatedMin = new Vector2I(
            Math.Min(rotatedMinOffset.x, rotatedMaxOffset.x),
            Math.Min(rotatedMinOffset.y, rotatedMaxOffset.y));
        rotatedMax = new Vector2I(
            Math.Max(rotatedMinOffset.x, rotatedMaxOffset.x),
            Math.Max(rotatedMinOffset.y, rotatedMaxOffset.y));
    }

    private void SaveRangeVisualizer()
    {
        if (rangeVisualizer == null)
        {
            return;
        }

        savedOriginOffset = rangeVisualizer.OriginOffset;
        savedRangeMin = rangeVisualizer.RangeMin;
        savedRangeMax = rangeVisualizer.RangeMax;
        savedTexSize = rangeVisualizer.TexSize;
        savedTestLineOfSight = rangeVisualizer.TestLineOfSight;
        savedBlockingTileVisible = rangeVisualizer.BlockingTileVisible;
        savedBlockingCb = rangeVisualizer.BlockingCb;
        savedRangeVisualizer = true;
    }

    private void RestoreRangeVisualizer()
    {
        if (rangeVisualizer == null || !savedRangeVisualizer)
        {
            return;
        }

        rangeVisualizer.OriginOffset = savedOriginOffset;
        rangeVisualizer.RangeMin = savedRangeMin;
        rangeVisualizer.RangeMax = savedRangeMax;
        rangeVisualizer.TexSize = savedTexSize;
        rangeVisualizer.TestLineOfSight = savedTestLineOfSight;
        rangeVisualizer.BlockingTileVisible = savedBlockingTileVisible;
        rangeVisualizer.BlockingCb = savedBlockingCb;
        savedRangeVisualizer = false;
    }
}


