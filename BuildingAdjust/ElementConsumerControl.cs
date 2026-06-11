using System;
using System.Linq;
using System.Reflection;
using BuildingAdjust.STRINGS;
using HarmonyLib;
using KSerialization;
using UnityEngine;

namespace BuildingAdjust
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ElementConsumerRadiusControl : KMonoBehaviour, IIntSliderControl
    {
        public int SliderDecimalPlaces(int index)
        {
            return 0;
        }

        public float GetSliderMin(int index)
        {
            return 1;
        }

        public float GetSliderMax(int index)
        {
            return 256;
        }

        public float GetSliderValue(int index)
        {
            return consumptionRadius;
        }

        public void SetSliderValue(float percent, int index)
        {
            var value = Convert.ToInt32(percent);
            if (value == consumptionRadius) return;
            consumptionRadius = value;
            UpdateElementConsumer();
        }

        private void UpdateRangeView()
        {
            if (!ShowRangeModEnabled)
            {
                return;
            }

            UpdateElementConsumerVisualizer();
            UpdateSimVisualizerParams();
            ResetSimRangeVisualizer();
        }

        private void UpdateElementConsumerVisualizer()
        {
            if (ElementConsumerVisualizerType is null)
            {
                return;
            }

            var elementConsumerVisualizerComponent = gameObject.GetComponent(ElementConsumerVisualizerType);
            if (elementConsumerVisualizerComponent is not null)
            {
                Traverse.Create(elementConsumerVisualizerComponent).Field<int>("radius").Value = consumptionRadius;
            }
        }

        private void UpdateSimVisualizerParams()
        {
            if (SimVisualizerParamsType is null || SimVisualizerType is null)
            {
                return;
            }

            var simVisualizerParamsComponent = gameObject.GetComponent(SimVisualizerParamsType);
            if (simVisualizerParamsComponent is null)
            {
                return;
            }

            var offset = GetSampleCellOffset();
            var visualizers = Array.CreateInstance(SimVisualizerType, 1);
            visualizers.SetValue(Activator.CreateInstance(SimVisualizerType, offset, consumptionRadius), 0);
            var worstCaseRadius = Math.Max(Math.Abs(offset.x) + consumptionRadius, Math.Abs(offset.y) + consumptionRadius);

            Traverse.Create(simVisualizerParamsComponent).Field("visualizers").SetValue(visualizers);
            Traverse.Create(simVisualizerParamsComponent).Field("worstCaseRadius").SetValue(worstCaseRadius);
        }

        private void ResetSimRangeVisualizer()
        {
            if (SimRangeVisualizerType is null || CameraController.Instance is null ||
                CameraController.Instance.overlayNoDepthCamera is null)
            {
                return;
            }

            var simRangeVisualizerComponent =
                CameraController.Instance.overlayNoDepthCamera.GetComponent(SimRangeVisualizerType);
            if (simRangeVisualizerComponent is null)
            {
                return;
            }

            Traverse.Create(simRangeVisualizerComponent).Field("lastTransform").SetValue(null);
            Traverse.Create(simRangeVisualizerComponent).Field("lastCell").SetValue(Grid.InvalidCell);
        }

        private CellOffset GetSampleCellOffset()
        {
            var sampleCellOffset = elementConsumer.sampleCellOffset;
            return new CellOffset(Mathf.RoundToInt(sampleCellOffset.x), Mathf.RoundToInt(sampleCellOffset.y));
        }

        public string GetSliderTooltipKey(int index)
        {
            return "STRINGS.UI.UISIDESCREENS.ELEMENTCONSUMERRADIUSCONTROLUISIDESCREEN.TOOLTIP";
        }

        public string GetSliderTooltip(int index)
        {
            return UI.UISIDESCREENS.ELEMENTCONSUMERRADIUSCONTROLUISIDESCREEN.TOOLTIP;
        }

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.ELEMENTCONSUMERRADIUSCONTROLUISIDESCREEN.TITLE";
        public string SliderUnits => "";

        [Serialize] public int consumptionRadius;
        [MyCmpReq] public ElementConsumer elementConsumer;
        [MyCmpReq] public Building building;
        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;
        private bool ShowRangeModEnabled { get; set; }
        public Type ElementConsumerVisualizerType { get; set; }
        public Type SimRangeVisualizerType { get; set; }
        public Type SimVisualizerParamsType { get; set; }
        public Type SimVisualizerType { get; set; }

        private static readonly EventSystem.IntraObjectHandler<ElementConsumerRadiusControl> OnCopySettingsDelegate =
            new((control, obj) => control.OnCopySettings(obj));

        private void OnCopySettings(object data)
        {
            var sourceControl = (data as GameObject)?.GetComponent<ElementConsumerRadiusControl>();
            if (sourceControl is null)
            {
                return;
            }

            consumptionRadius = sourceControl.consumptionRadius;
            UpdateElementConsumer();
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettingsDelegate);
            var showRangeMod = Global.Instance.modManager.mods.FirstOrDefault(mod => mod.label.id == "1960996649");
            ShowRangeModEnabled = showRangeMod is {loaded_mod_data: {dlls: {Count: >0}}};
            if (ShowRangeModEnabled)
            {
                var assembly = showRangeMod?.loaded_mod_data.dlls.First();
                ElementConsumerVisualizerType = assembly?.GetType("PeterHan.ShowRange.ElementConsumerVisualizer");
                SimRangeVisualizerType = assembly?.GetType("PeterHan.ShowRange.SimRangeVisualizer");
                SimVisualizerParamsType = assembly?.GetType("PeterHan.ShowRange.SimVisualizerParams");
                SimVisualizerType = assembly?.GetType("PeterHan.ShowRange.SimVisualizer");
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (consumptionRadius < 1)
            {
                consumptionRadius = elementConsumer.consumptionRadius;
            }
            else
            {
                UpdateElementConsumer();
            }
        }

        public void ApplyStoredRadius()
        {
            if (consumptionRadius < 1)
            {
                return;
            }

            elementConsumer.consumptionRadius = (byte) consumptionRadius;
            UpdateRangeView();
        }

        private void UpdateElementConsumer()
        {
            elementConsumer.consumptionRadius = (byte) consumptionRadius;
            var simHandle = Traverse.Create(elementConsumer).Field<int>("simHandle").Value;
            if (simHandle != -2 && Sim.IsValidHandle(simHandle))
            {
                AccessTools.Method(typeof(SimComponent), "SimUnregister").Invoke(elementConsumer, new object[] { });
                AccessTools.Method(typeof(SimComponent), "SimRegister").Invoke(elementConsumer, new object[] { });
            }
            UpdateRangeView();
        }
    }
}
