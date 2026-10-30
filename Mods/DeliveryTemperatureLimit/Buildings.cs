using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace DeliveryTemperatureLimit
{
    public class Buildings_Patch
    {
        public static void Patch( Harmony harmony )
        {
            MethodInfo method = AccessTools.Method( typeof( BuildingConfigManager ), nameof( BuildingConfigManager.ConfigurePost ));
            if( method != null )
                harmony.Patch( method, postfix: new HarmonyMethod( typeof( Buildings_Patch ), nameof( ConfigurePost )));
            else
                Debug.LogError( "DeliveryTemperatureLimit: 找不到 BuildingConfigManager.ConfigurePost，无法注册建筑温度限制。" );
        }

        public static void ConfigurePost( Dictionary< IBuildingConfig, BuildingDef > ___configTable )
        {
            int added = 0;
            foreach( KeyValuePair< IBuildingConfig, BuildingDef > entry in ___configTable )
            {
                BuildingDef def = entry.Value;
                if( def == null || !IsEligible( entry.Key, def.BuildingComplete ))
                    continue;
                if( def.BuildingComplete.GetComponent<TemperatureLimit>() == null )
                {
                    def.BuildingComplete.AddOrGet<TemperatureLimit>();
                    ++added;
                }
            }
            Debug.Log( $"DeliveryTemperatureLimit: 已为 {added} 种建筑注册温度限制。" );
        }

        private static bool IsEligible( IBuildingConfig config, GameObject go )
        {
            if( go == null )
                return false;
            // 存储地砖和轨道货舱没有普通储物箱的组件，但仍可接受搬运。
            if( config is StorageTileConfig || config is OrbitalCargoModuleConfig )
                return true;
            string assemblyName = config.GetType().Assembly.GetName().Name;
            if( assemblyName == "MoveThisHere" || assemblyName == "Storage Pod" )
                return true;
            Storage storage = go.GetComponent<Storage>();
            return go.GetComponent<ManualDeliveryKG>() != null
                || ( storage != null && storage.allowUIItemRemoval )
                || go.GetComponent<StorageLocker>() != null
                || go.GetComponent<ObjectDispenser>() != null
                || go.GetComponent<SolidConduitInbox>() != null
                || go.GetComponent<BottleEmptier>() != null
                || go.GetComponent<CreatureFeeder>() != null
                || go.GetComponent<RationBox>() != null
                || go.GetComponent<Refrigerator>() != null;
        }
    }
}
