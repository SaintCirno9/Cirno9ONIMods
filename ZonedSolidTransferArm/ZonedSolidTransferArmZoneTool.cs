using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ZonedSolidTransferArm;

public class ZonedSolidTransferArmZoneTool : DragTool
{
    public enum EditMode
    {
        Add,
        Remove
    }

    public static ZonedSolidTransferArmZoneTool Instance { get; private set; }

    private static ZonedSolidTransferArmControl target;
    private static KSelectable pendingSelection;
    private static EditMode mode;
    private static readonly Color RemoveZoneColor = new Color(1f, 0.08f, 0.05f, 0.9f);

    public static ZonedSolidTransferArmControl Target => target;

    public static void SetTarget(ZonedSolidTransferArmControl control, EditMode editMode)
    {
        target = control;
        mode = editMode;
        Instance?.ApplyActiveColor();
    }

    protected override void OnPrefabInit()
    {
        Instance = this;
        CopyDragToolVisuals();
        base.OnPrefabInit();
        visualizer = CreateVisualizer();
        interceptNumberKeysForPriority = true;
        viewMode = ZonedSolidTransferArmZoneOverlay.ID;
    }

    protected override void OnActivateTool()
    {
        base.OnActivateTool();
        ApplyActiveColor();
        SetMode(Mode.Box);
    }

    protected override void OnDeactivateTool(InterfaceTool newTool)
    {
        base.OnDeactivateTool(newTool);
        if (newTool != this)
        {
            ZonedSolidTransferArmControl previousTarget = target;
            target = null;
            if (newTool == null || newTool == SelectTool.Instance)
            {
                QueueTargetSelection(previousTarget);
            }
        }
    }

    public static void RestorePendingSelection()
    {
        KSelectable selectable = pendingSelection;
        pendingSelection = null;
        if (selectable != null)
        {
            SelectTool.Instance.Select(selectable, true);
        }
    }

    protected override void OnDragTool(int cell, int distFromOrigin)
    {
        if (target == null || !Grid.IsValidCell(cell) || !Grid.IsVisible(cell))
        {
            return;
        }

        if (mode == EditMode.Remove)
        {
            target.RemoveCell(cell);
        }
        else
        {
            target.AddCell(cell);
        }
    }

    public override void GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors)
    {
        colors = new HashSet<ToolMenu.CellColorData>();
        if (target == null)
        {
            return;
        }

        foreach (int cell in target.Cells)
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
        GameObject root = new GameObject("ZonedSolidTransferArmZoneToolVisualizer");
        root.SetActive(false);

        GameObject offset = new GameObject("ZonedSolidTransferArmZoneToolCursor");
        offset.transform.SetParent(root.transform);
        offset.transform.SetLocalPosition(new Vector3(0f, Grid.HalfCellSizeInMeters, 0f));
        offset.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
        offset.SetLayerRecursively(LayerMask.NameToLayer("Overlay"));

        SpriteRenderer renderer = offset.AddComponent<SpriteRenderer>();
        renderer.sprite = Assets.GetSprite("cursorIcon") ?? Assets.GetSprite("action_move_to_storage");
        renderer.color = ZonedSolidTransferArmControl.ZoneColor;
        renderer.sortingOrder = 10;

        return root;
    }

    private static Color GetActiveColor()
    {
        return mode == EditMode.Remove ? RemoveZoneColor : ZonedSolidTransferArmControl.ZoneColor;
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

    private static void QueueTargetSelection(ZonedSolidTransferArmControl control)
    {
        if (control == null)
        {
            return;
        }

        pendingSelection = control.GetComponent<KSelectable>();
    }
}
