using System;
using Harmony;
using KSerialization;
using UnityEngine;

namespace ColonyTweaks
{
    public class VentControl : KMonoBehaviour
    {
        [Serialize] public bool ignorePressure;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            gameObject.AddOrGet<CopyBuildingSettings>();
            Subscribe((int) GameHashes.RefreshUserMenu, OnRefreshUserMenu);
            Subscribe((int) GameHashes.CopySettings, OnCopySettings);
        }

        private void OnCopySettings(object data)
        {
            if (data is GameObject go && go.GetComponent<VentControl>() is { } sourceControl)
            {
                ignorePressure = sourceControl.ignorePressure;
            }
        }

        private void OnRefreshUserMenu(object _)
        {
            Game.Instance.userMenu.AddButton(gameObject, ignorePressure
                ? new KIconButtonMenu.ButtonInfo("action_building_disabled", "启用超压判定", tooltipText: "启用出口超压判定",
                    on_click: () => ignorePressure = !ignorePressure)
                : new KIconButtonMenu.ButtonInfo("action_building_disabled", "禁用超压判定", tooltipText: "禁用出口超压判定",
                    on_click: () => ignorePressure = !ignorePressure));
        }
    }
}