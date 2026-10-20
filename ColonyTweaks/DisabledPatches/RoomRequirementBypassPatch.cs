using HarmonyLib;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace ColonyTweaks.DisabledPatches
{
    public class RoomRequirementBypassPatch
    {
        private const float CritterCondoEffectDuration = 1800f;
        private const float CritterCondoHappiness = 3f;

        [HarmonyPatch(typeof(RanchStationConfig), nameof(RanchStationConfig.DoPostConfigureComplete))]
        public class RanchStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                DisableRanchRoomRequirement(go);
            }
        }

        [HarmonyPatch(typeof(UnderwaterRanchStationConfig), nameof(UnderwaterRanchStationConfig.DoPostConfigureComplete))]
        public class UnderwaterRanchStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                DisableRanchRoomRequirement(go);
            }
        }

        [HarmonyPatch(typeof(FarmStationConfig), nameof(FarmStationConfig.DoPostConfigureComplete))]
        public class FarmStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
            }
        }

        [HarmonyPatch(typeof(ShearingStationConfig), nameof(ShearingStationConfig.DoPostConfigureComplete))]
        public class ShearingStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                DisableRanchRoomRequirement(go);
            }
        }

        [HarmonyPatch(typeof(UnderwaterShearingStationConfig), nameof(UnderwaterShearingStationConfig.DoPostConfigureComplete))]
        public class UnderwaterShearingStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                DisableRanchRoomRequirement(go);
            }
        }

        [HarmonyPatch(typeof(MilkingStationConfig), nameof(MilkingStationConfig.DoPostConfigureComplete))]
        public class MilkingStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                DisableRanchRoomRequirement(go);
            }
        }

        [HarmonyPatch(typeof(UnderwaterMilkingStationConfig), nameof(UnderwaterMilkingStationConfig.DoPostConfigureComplete))]
        public class UnderwaterMilkingStationConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                DisableRanchRoomRequirement(go);
            }
        }

        [HarmonyPatch(typeof(CritterCondoConfig), nameof(CritterCondoConfig.DoPostConfigureComplete))]
        public class CritterCondoConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                ReplaceCritterCondoOperationalCheck(go, condo => !condo.GetComponent<Floodable>().IsFlooded);
                ApplyCritterCondoEffect(go);
            }
        }

        [HarmonyPatch(typeof(UnderwaterCritterCondoConfig), nameof(UnderwaterCritterCondoConfig.DoPostConfigureComplete))]
        public class UnderwaterCritterCondoConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                ReplaceCritterCondoOperationalCheck(go, IsUnderwaterCondoOperational);
                ApplyCritterCondoEffect(go);
            }
        }

        [HarmonyPatch(typeof(AirBorneCritterCondoConfig), nameof(AirBorneCritterCondoConfig.DoPostConfigureComplete))]
        public class AirBorneCritterCondoConfigDoPostConfigureCompletePatch
        {
            public static void Postfix(GameObject go)
            {
                RelaxRoomRequirement(go);
                ReplaceCritterCondoOperationalCheck(go, condo => true);
                ApplyCritterCondoEffect(go);
            }
        }

        private static void ApplyCritterCondoEffect(GameObject go)
        {
            var effect = Db.Get().effects.Get(go.GetDef<CritterCondo.Def>().effectId);
            var happinessId = Db.Get().CritterAttributes.Happiness.Id;
            effect.duration = CritterCondoEffectDuration;
            effect.SelfModifiers.RemoveAll(modifier => modifier.AttributeId == happinessId);
            effect.Add(new AttributeModifier(happinessId, CritterCondoHappiness,
                CREATURES.MODIFIERS.CRITTERCONDOINTERACTEFFECT.NAME));
        }

        private static void RelaxRoomRequirement(GameObject go)
        {
            var roomTracker = go.GetComponent<RoomTracker>();
            if (roomTracker != null)
            {
                roomTracker.requirement = RoomTracker.Requirement.TrackingOnly;
            }
        }

        private static void DisableRanchRoomRequirement(GameObject go)
        {
            var def = go.GetDef<RanchStation.Def>();
            if (def != null)
            {
                def.RequiresRoom = false;
            }
        }

        private static void ReplaceCritterCondoOperationalCheck(GameObject go,
            System.Func<CritterCondo.Instance, bool> isOperationalCb)
        {
            go.GetDef<CritterCondo.Def>().IsCritterCondoOperationalCb = isOperationalCb;
        }

        private static bool IsUnderwaterCondoOperational(CritterCondo.Instance condo)
        {
            var building = condo.GetComponent<Building>();
            for (var i = 0; i < building.PlacementCells.Length; i++)
            {
                if (!Grid.IsLiquid(building.PlacementCells[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
