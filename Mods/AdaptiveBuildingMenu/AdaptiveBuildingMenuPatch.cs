using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace AdaptiveBuildingMenu
{
    internal static class AdaptiveBuildingMenuPatch
    {
        private const int DefaultColumnCount = 3;
        private const int MaxGridColumnCount = 5;
        private const int HeaderHeight = 24;
        private const int MaxHeaderHeight = 72;
        private const float DefaultPanelWidth = 264f;
        private const float ListPanelWidth = 320f;
        private const float SearchBarHeight = 36f;
        private const float ProductInfoGap = 8f;
        private const float PanelRightPadding = 18f;
        private const float MaxButtonWidth = 120f;

        private static readonly FieldInfo BuildGridWidthField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_width");
        private static readonly FieldInfo BuildGridRowHeightField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_rowHeight");
        private static readonly FieldInfo BuildGridBorderHeightField = AccessTools.Field(typeof(PlanScreen), "buildGrid_bg_borderHeight");
        private static readonly FieldInfo UseSubCategoryLayoutField = AccessTools.Field(typeof(PlanScreen), "useSubCategoryLayout");
        private static readonly FieldInfo ActiveCategoryInfoField = AccessTools.Field(typeof(PlanScreen), "activeCategoryInfo");
        private static readonly MethodInfo ClearButtonsMethod = AccessTools.Method(typeof(PlanScreen), "ClearButtons");
        private static readonly MethodInfo ForceUpdateAllCategoryTogglesMethod = AccessTools.Method(typeof(PlanScreen), "ForceUpdateAllCategoryToggles");

        [HarmonyPatch(typeof(PlanScreen), "ConfigurePanelSize")]
        private static class ConfigurePanelSizePatch
        {
            private static void Postfix(PlanScreen __instance)
            {
                ApplyLayout(__instance);
            }
        }

        [HarmonyPatch(typeof(PlanScreen), "RefreshScale")]
        private static class RefreshScalePatch
        {
            private static void Postfix(PlanScreen __instance)
            {
                ApplyLayout(__instance);
            }
        }

        [HarmonyPatch(typeof(PlanScreen), "OnSelectBuilding")]
        private static class OnSelectBuildingPatch
        {
            private static void Postfix(PlanScreen __instance, GameObject button_go)
            {
                if (__instance.SelectedBuildingGameObject == button_go && __instance.buildingGroupsRoot.gameObject.activeSelf)
                {
                    CloseCategoryPanelImmediate(__instance);
                    MoveProductInfoToLeft(__instance);
                }
            }
        }

        private static void ApplyLayout(PlanScreen screen)
        {
            if (screen == null)
            {
                return;
            }

            List<GridState> groups = GetGroups(screen);
            if (groups.Count == 0)
            {
                return;
            }

            bool useListLayout = GetFieldValue<bool>(UseSubCategoryLayoutField, screen);
            int baseColumns = useListLayout ? 1 : DefaultColumnCount;
            int maxRows = GetMaxVisibleRows(screen);

            PickColumnCounts(groups, baseColumns, GetMaxColumns(useListLayout, baseColumns), maxRows);

            float width = GetPanelWidth(groups, useListLayout, baseColumns);
            ApplyGridSettings(groups, useListLayout, width);
            RebuildGrids(groups);

            width = GetMeasuredWidth(screen, groups, width);
            ApplyGridSettings(groups, useListLayout, width);
            SetFieldValue(BuildGridWidthField, screen, width);
            ResizePanel(screen, groups, width, maxRows);
            MoveProductInfo(screen, width);
        }

        private static List<GridState> GetGroups(PlanScreen screen)
        {
            var groups = new List<GridState>();
            if (screen.GroupsTransform == null)
            {
                return groups;
            }

            for (int i = 0; i < screen.GroupsTransform.childCount; i++)
            {
                Transform groupTransform = screen.GroupsTransform.GetChild(i);
                GridLayoutGroup grid = groupTransform.GetComponentInChildren<GridLayoutGroup>(true);
                if (grid == null)
                {
                    continue;
                }

                groups.Add(new GridState(grid, CountActiveChildren(grid.transform)));
            }

            return groups;
        }

        private static int CountActiveChildren(Transform parent)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).gameObject.activeSelf)
                {
                    count++;
                }
            }

            return count;
        }

        private static void PickColumnCounts(List<GridState> groups, int baseColumns, int maxColumns, int maxRows)
        {
            foreach (GridState group in groups)
            {
                group.Columns = baseColumns;
            }

            while (GetTotalRows(groups) > maxRows)
            {
                GridState bestGroup = null;
                int bestSavedRows = 0;

                foreach (GridState group in groups)
                {
                    if (group.ActiveChildren <= 0 || group.Columns >= maxColumns)
                    {
                        continue;
                    }

                    int savedRows = GetRows(group.ActiveChildren, group.Columns) - GetRows(group.ActiveChildren, group.Columns + 1);
                    if (savedRows > bestSavedRows)
                    {
                        bestGroup = group;
                        bestSavedRows = savedRows;
                    }
                }

                if (bestGroup == null)
                {
                    break;
                }

                bestGroup.Columns++;
            }
        }

        private static int GetMaxColumns(bool useListLayout, int baseColumns)
        {
            return useListLayout ? baseColumns : Math.Max(baseColumns, MaxGridColumnCount);
        }

        private static void ApplyGridSettings(List<GridState> groups, bool useListLayout, float panelWidth)
        {
            foreach (GridState group in groups)
            {
                group.Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                group.Grid.constraintCount = group.Columns;

                if (useListLayout)
                {
                    float spacing = group.Grid.spacing.x * Math.Max(0, group.Columns - 1);
                    float cellWidth = Math.Max(1f, (panelWidth - 24f - spacing) / group.Columns);
                    group.Grid.cellSize = new Vector2(cellWidth, group.Grid.cellSize.y);
                }
            }
        }

        private static float GetPanelWidth(List<GridState> groups, bool useListLayout, int baseColumns)
        {
            int columns = baseColumns;
            foreach (GridState group in groups)
            {
                columns = Math.Max(columns, group.Columns);
            }

            float baseWidth = useListLayout ? ListPanelWidth : DefaultPanelWidth;
            if (columns <= baseColumns)
            {
                return baseWidth;
            }

            GridLayoutGroup sampleGrid = groups[0].Grid;
            float cellWidth = useListLayout ? Math.Max(1f, baseWidth - 24f) : GetCellWidth(sampleGrid, baseWidth, baseColumns);
            return baseWidth + (columns - baseColumns) * (cellWidth + sampleGrid.spacing.x);
        }

        private static float GetCellWidth(GridLayoutGroup grid, float baseWidth, int baseColumns)
        {
            if (grid.cellSize.x > 0f && grid.cellSize.x <= MaxButtonWidth)
            {
                return grid.cellSize.x;
            }

            float spacing = grid.spacing.x * Math.Max(0, baseColumns - 1);
            float padding = grid.padding.left + grid.padding.right;
            float width = (baseWidth - padding - spacing) / Math.Max(1, baseColumns);
            return width > 0f && width <= MaxButtonWidth ? width : 78f;
        }

        private static void RebuildGrids(List<GridState> groups)
        {
            foreach (GridState group in groups)
            {
                if (group.Grid.transform is RectTransform rectTransform)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                }
            }
        }

        private static float GetMeasuredWidth(PlanScreen screen, List<GridState> groups, float fallbackWidth)
        {
            if (screen.buildingGroupsRoot == null)
            {
                return fallbackWidth;
            }

            var corners = new Vector3[4];
            RectTransform root = screen.buildingGroupsRoot;
            float rootLeft = root.rect.xMin;
            float right = rootLeft;

            foreach (GridState group in groups)
            {
                for (int i = 0; i < group.Grid.transform.childCount; i++)
                {
                    Transform child = group.Grid.transform.GetChild(i);
                    if (!child.gameObject.activeSelf || !(child is RectTransform childRect))
                    {
                        continue;
                    }

                    childRect.GetWorldCorners(corners);
                    for (int j = 0; j < corners.Length; j++)
                    {
                        Vector3 localCorner = root.InverseTransformPoint(corners[j]);
                        right = Math.Max(right, localCorner.x);
                    }
                }
            }

            float measuredWidth = right - rootLeft + PanelRightPadding;
            if (measuredWidth <= 0f)
            {
                return fallbackWidth;
            }

            float minWidth = Math.Min(fallbackWidth, ListPanelWidth);
            return Math.Max(minWidth, Math.Min(measuredWidth, fallbackWidth));
        }

        private static void ResizePanel(PlanScreen screen, List<GridState> groups, float width, int maxRows)
        {
            if (screen.buildingGroupsRoot == null)
            {
                return;
            }

            int totalRows = GetTotalRows(groups);
            int visibleRows = Math.Min(totalRows, maxRows);
            float rowHeight = GetFieldValue<float>(BuildGridRowHeightField, screen);
            float borderHeight = GetFieldValue<float>(BuildGridBorderHeightField, screen);
            int headerHeight = Math.Min(MaxHeaderHeight, CountNonEmptyGroups(groups) * HeaderHeight);
            float height = borderHeight + headerHeight + SearchBarHeight + visibleRows * rowHeight;

            SetRectSize(screen.buildingGroupsRoot, width, height);
            SetRectWidth(screen.BuildButtonBGPanel, width);
            SetRectWidth(screen.BuildingGroupContentsRect, width);
            SetScrollbar(screen, totalRows > maxRows);
            LayoutRebuilder.ForceRebuildLayoutImmediate(screen.buildingGroupsRoot);
        }

        private static void SetRectSize(RectTransform rect, float width, float height)
        {
            if (rect == null)
            {
                return;
            }

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private static void SetRectWidth(RectTransform rect, float width)
        {
            if (rect == null)
            {
                return;
            }

            if (!Mathf.Approximately(rect.anchorMin.x, rect.anchorMax.x))
            {
                rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
                return;
            }

            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        private static void SetScrollbar(PlanScreen screen, bool visible)
        {
            ScrollRect scrollRect = screen.BuildingGroupContentsRect?.GetComponent<ScrollRect>();
            Scrollbar scrollbar = scrollRect?.verticalScrollbar;
            if (scrollbar != null)
            {
                scrollbar.gameObject.SetActive(visible);
            }
        }

        private static void MoveProductInfo(PlanScreen screen, float menuWidth)
        {
            if (screen.ProductInfoScreen == null)
            {
                return;
            }

            RectTransform rect = screen.ProductInfoScreen.GetComponent<RectTransform>();
            if (rect != null)
            {
                float x = screen.buildingGroupsRoot.gameObject.activeSelf ? menuWidth + ProductInfoGap : ProductInfoGap;
                rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
            }
        }

        private static void MoveProductInfoToLeft(PlanScreen screen)
        {
            if (screen.ProductInfoScreen == null)
            {
                return;
            }

            RectTransform rect = screen.ProductInfoScreen.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(ProductInfoGap, rect.anchoredPosition.y);
            }
        }

        private static void CloseCategoryPanelImmediate(PlanScreen screen)
        {
            ActiveCategoryInfoField?.SetValue(screen, null);
            ExpandRevealUIContent reveal = screen.buildingGroupsRoot.GetComponent<ExpandRevealUIContent>();
            if (reveal != null)
            {
                reveal.Collapsing = false;
                reveal.Expanding = false;
                reveal.CollapsedImmediate();
            }

            ClearButtonsMethod?.Invoke(screen, null);
            screen.buildingGroupsRoot.gameObject.SetActive(false);
            screen.PlanCategoryLabel.text = "";
            ForceUpdateAllCategoryTogglesMethod?.Invoke(screen, new object[] { null });
        }

        private static int GetMaxVisibleRows(PlanScreen screen)
        {
            float rowHeight = Math.Max(1f, GetFieldValue<float>(BuildGridRowHeightField, screen));
            int rows = Math.Max(1, (int)(Screen.height / rowHeight) - 3);
            return Math.Max(1, Math.Min(8, rows));
        }

        private static int GetTotalRows(List<GridState> groups)
        {
            int rows = 0;
            foreach (GridState group in groups)
            {
                rows += GetRows(group.ActiveChildren, group.Columns);
            }

            return rows;
        }

        private static int CountNonEmptyGroups(List<GridState> groups)
        {
            int count = 0;
            foreach (GridState group in groups)
            {
                if (group.ActiveChildren > 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetRows(int itemCount, int columns)
        {
            if (itemCount <= 0)
            {
                return 0;
            }

            return (itemCount + Math.Max(1, columns) - 1) / Math.Max(1, columns);
        }

        private static T GetFieldValue<T>(FieldInfo field, object instance)
        {
            return field == null ? default : (T)field.GetValue(instance);
        }

        private static void SetFieldValue(FieldInfo field, object instance, object value)
        {
            field?.SetValue(instance, value);
        }

        private sealed class GridState
        {
            public GridState(GridLayoutGroup grid, int activeChildren)
            {
                Grid = grid;
                ActiveChildren = activeChildren;
            }

            public GridLayoutGroup Grid { get; }
            public int ActiveChildren { get; }
            public int Columns { get; set; }
        }
    }
}
