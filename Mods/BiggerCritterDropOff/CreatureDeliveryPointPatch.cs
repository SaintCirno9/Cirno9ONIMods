using KMod;
using HarmonyLib;

namespace BiggerCritterDropOff
{
    public class CreatureDeliveryPointPatch: UserMod2
    {
        [HarmonyPatch(typeof(CreatureDeliveryPoint), "IUserControlledCapacity.MaxCapacity", MethodType.Getter)]
        public static class CreatureDeliveryPointMaxCapacityPatch
        {
            public static void Postfix(ref float __result)
            {
                __result = 1000f;
            }
        }
    }
}