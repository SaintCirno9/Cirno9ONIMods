using System;
using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace ColonyTweaks
{
    public class SimCellOccupierControl : KMonoBehaviour, ICheckboxControl, ISim200ms
    {
        [MyCmpReq] private Building building;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            gameObject.AddOrGet<CopyBuildingSettings>();
            Subscribe((int) GameHashes.CopySettings, OnCopySettings);
        }

        protected override void OnSpawn()
        {
            if (!isInitialized)
            {
                isFluidImpermeable = defaultFluidImpermeable;
                isInitialized = true;
            }

            UpdateCellProperties();
        }

        protected override void OnCleanUp()
        {
            isFluidImpermeable = false;
            UpdateCellProperties();
        }

        private void OnCopySettings(object data)
        {
            if (data is GameObject go)
            {
                isFluidImpermeable = go.GetComponent<SimCellOccupierControl>().isFluidImpermeable;
                UpdateCellProperties();
            }
        }

        public bool GetCheckboxValue()
        {
            return isFluidImpermeable;
        }

        public void SetCheckboxValue(bool value)
        {
            if (isFluidImpermeable != value)
            {
                isFluidImpermeable = value;
                UpdateCellProperties();
            }
        }

        public string CheckboxTitleKey => "STRINGS.UI.UISIDESCREENS.SIMCELLOCCUPIERCONTROLUISIDESCREEN.TITLE";
        public string CheckboxLabel => STRINGS.UI.UISIDESCREENS.SIMCELLOCCUPIERCONTROLUISIDESCREEN.CHECKBOXLABEL;

        public string CheckboxTooltip => STRINGS.UI.UISIDESCREENS.SIMCELLOCCUPIERCONTROLUISIDESCREEN.CHECKBOXTOOLTIP;

        [Serialize] public bool isFluidImpermeable;
        [Serialize] public bool isInitialized;
        public bool defaultFluidImpermeable;

        private static Sim.Cell.Properties GetSimCellProperties()
        {
            return Sim.Cell.Properties.GasImpermeable | Sim.Cell.Properties.LiquidImpermeable |
                   Sim.Cell.Properties.SolidImpermeable;
        }

        private void UpdateCellProperties()
        {
            if (isFluidImpermeable)
            {
                foreach (var placementCell in building.PlacementCells)
                {
                    SimMessages.ReplaceAndDisplaceElement(placementCell, SimHashes.Void, null, 0f);
                    SimMessages.SetCellProperties(placementCell, (byte) GetSimCellProperties());
                }
            }
            else
            {
                foreach (var placementCell in building.PlacementCells)
                {
                    if (Grid.Element[placementCell].id == SimHashes.Void)
                    {
                        SimMessages.ReplaceAndDisplaceElement(placementCell, SimHashes.Vacuum, null, 0f);
                    }

                    SimMessages.ClearCellProperties(placementCell, (byte) GetSimCellProperties());
                }
            }
        }

        public void Sim200ms(float dt)
        {
            if (!isFluidImpermeable) return;
            foreach (var placementCell in building.PlacementCells)
            {
                if (Grid.Element[placementCell].id != SimHashes.Void)
                {
                    SimMessages.ReplaceAndDisplaceElement(placementCell, SimHashes.Void, null, 0f);
                }
            }
        }
    }
}