using System.Reflection;
using Harmony;
using KSerialization;

namespace ColonyTweaks
{
    public class BottleFillerAutoDropControl : KMonoBehaviour
    {
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(493375141, OnRefreshUserMenu);
            Subscribe((int) GameHashes.OnStorageChange, OnStorageChanged);
        }

        public void OnRefreshUserMenu(object _)
        {
            var text = autoDrop ? "禁用自动掉落" : "启用自动掉落";
            var tooltipText = autoDrop
                ? "禁用满瓶自动掉落"
                : "启用满瓶自动掉落";
            var button = new KIconButtonMenu.ButtonInfo("action_building_disabled", text,
                OnChangeAutoDrop, Action.CinemaZoomSpeedMinus, OnRefreshUserMenu, null, null,
                tooltipText);
            Game.Instance.userMenu.AddButton(gameObject, button);
        }

        public void OnStorageChanged(object data)
        {
            if (!autoDrop)
            {
                return;
            }

            if (storage.RemainingCapacity() < 0.5f)
            {
                storage.DropAll();
            }
        }

        private void OnChangeAutoDrop()
        {
            autoDrop = !autoDrop;
            if (autoDrop)
            {
                storage.DropAll();
            }
        }


        [Serialize] public bool autoDrop = true;
        [MyCmpReq] public Storage storage;
        [MyCmpReq] public DropAllWorkable dropAllWorkable;
    }
}