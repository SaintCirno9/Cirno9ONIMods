using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace AdjustableSolidTransferArm
{
    public static class NoPickZone
    {
        public static readonly Color ZoneColor = new Color(1f, 0.04f, 0f, 0.9f);

        private static readonly ConcurrentDictionary<int, byte> Cells = new ConcurrentDictionary<int, byte>();

        public static IEnumerable<int> MarkedCells => Cells.Keys;

        public static bool ContainsCell(int cell)
        {
            return Cells.ContainsKey(cell);
        }

        public static void AddCell(int cell)
        {
            Cells[cell] = 0;
        }

        public static void RemoveCell(int cell)
        {
            Cells.TryRemove(cell, out _);
        }

        public static void Clear()
        {
            Cells.Clear();
        }

        public static string Serialize()
        {
            return string.Join("_", Cells.Keys);
        }

        public static void Load(string data)
        {
            Clear();
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            foreach (string value in data.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(value, out int cell))
                {
                    AddCell(cell);
                }
            }
        }

        public static void RemoveInvalidCells()
        {
            foreach (int cell in Cells.Keys)
            {
                if (!Grid.IsValidCell(cell))
                {
                    RemoveCell(cell);
                }
            }
        }
    }

    [SerializationConfig(MemberSerialization.OptIn)]
    public class NoPickZoneSaveData : KMonoBehaviour, ISaveLoadable
    {
        [Serialize]
        private string noPickZoneCells = "";

        [OnSerializing]
        private void OnSerializing()
        {
            noPickZoneCells = NoPickZone.Serialize();
        }

        [OnDeserialized]
        private void OnDeserialized()
        {
            NoPickZone.Load(noPickZoneCells);
        }
    }

    public class NoPickZoneOverlay : OverlayModes.Mode
    {
        public static readonly HashedString ID = "AdjustableSolidTransferArmNoPickZone";

        public override HashedString ViewMode()
        {
            return ID;
        }

        public override void Enable()
        {
            NoPickZone.RemoveInvalidCells();
        }

        public override string GetSoundName()
        {
            return "SuitRequired";
        }

        public static Color GetCellColor(SimDebugView instance, int cell)
        {
            return NoPickZone.ContainsCell(cell) ? NoPickZone.ZoneColor : Color.black;
        }

        public override List<LegendEntry> GetCustomLegendData()
        {
            return new List<LegendEntry>
            {
                new LegendEntry(
                    SolidTransferArmControlStrings.UI.OVERLAYS.NOPICKZONE.MARKED,
                    SolidTransferArmControlStrings.UI.OVERLAYS.NOPICKZONE.TOOLTIP,
                    NoPickZone.ZoneColor)
            };
        }
    }

    public class NoPickZoneTool : DragTool
    {
        protected override void OnPrefabInit()
        {
            CopyDragToolVisuals();
            base.OnPrefabInit();
            visualizer = CreateVisualizer();
            interceptNumberKeysForPriority = true;
            viewMode = NoPickZoneOverlay.ID;
        }

        protected override void OnActivateTool()
        {
            base.OnActivateTool();
            SetMode(Mode.Box);
        }

        protected override void OnDragTool(int cell, int distFromOrigin)
        {
            if (!Grid.IsValidCell(cell) || !Grid.IsVisible(cell))
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                NoPickZone.RemoveCell(cell);
            }
            else
            {
                NoPickZone.AddCell(cell);
            }
        }

        public override void GetOverlayColorData(out HashSet<ToolMenu.CellColorData> colors)
        {
            colors = new HashSet<ToolMenu.CellColorData>();
            foreach (int cell in NoPickZone.MarkedCells)
            {
                colors.Add(new ToolMenu.CellColorData(cell, NoPickZone.ZoneColor));
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
            Traverse.Create(this).Field("areaColour").SetValue((Color32)NoPickZone.ZoneColor);

            Texture2D sourceCursor = Traverse.Create(source).Field<Texture2D>("cursor").Value;
            cursor = sourceCursor;
            Traverse.Create(this).Field("boxCursor").SetValue(sourceCursor);
        }

        private static GameObject CreateVisualizer()
        {
            GameObject root = new GameObject("NoPickZoneToolVisualizer");
            root.SetActive(false);

            GameObject offset = new GameObject("NoPickZoneToolCursor");
            offset.transform.SetParent(root.transform);
            offset.transform.SetLocalPosition(new Vector3(0f, Grid.HalfCellSizeInMeters, 0f));
            offset.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
            offset.SetLayerRecursively(LayerMask.NameToLayer("Overlay"));

            SpriteRenderer renderer = offset.AddComponent<SpriteRenderer>();
            renderer.sprite = Assets.GetSprite("cursorIcon") ?? Assets.GetSprite("icon_action_cancel");
            renderer.color = NoPickZone.ZoneColor;
            renderer.sortingOrder = 10;

            return root;
        }
    }
}
