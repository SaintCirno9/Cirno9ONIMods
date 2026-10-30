using System;
using KSerialization;
using UnityEngine;

namespace ColonyTweaks
{
    public class StorageControl : KMonoBehaviour, IUserControlledCapacity
    {
        [MyCmpReq] public Storage storage;
        [Serialize] public float capacityKg = -1f;
        public float maxCapacityKg = -1f;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            gameObject.AddOrGet<CopyBuildingSettings>();
            Subscribe((int) GameHashes.CopySettings, OnCopySettings);
        }


        protected override void OnSpawn()
        {
            base.OnSpawn();

            if (Math.Abs(capacityKg - (-1f)) < 0.1f)
            {
                capacityKg = storage.capacityKg;
            }

            if (Math.Abs(maxCapacityKg - (-1f)) < 0.1f)
            {
                maxCapacityKg = storage.capacityKg;
            }
        }

        private void OnCopySettings(object data)
        {
            if (data is GameObject go && go.GetComponent<StorageControl>() is { } storageControl)
            {
                UserMaxCapacity = storageControl.UserMaxCapacity;
            }
        }

        public float UserMaxCapacity
        {
            get => Mathf.Min(capacityKg, storage.capacityKg);
            set
            {
                capacityKg = value;
                if (TryGetFilteredStorage(out var filteredStorage))
                {
                    filteredStorage.FilterChanged();
                }
                else
                {
                    storage.capacityKg = capacityKg;
                }
            }
        }

        public float AmountStored => storage.MassStored();
        public float MinCapacity => 0f;
        public float MaxCapacity => maxCapacityKg;
        public bool WholeValues => false;
        public LocString CapacityUnits => GameUtil.GetCurrentMassUnit();
        public bool ControlEnabled()
        {
            return true;
        }

        private bool TryGetFilteredStorage(out FilteredStorage filteredStorage)
        {
            filteredStorage = GetComponent<FilteredStorage>();
            return filteredStorage is not null;
        }
    }
}
