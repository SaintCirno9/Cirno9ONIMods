using HarmonyLib;
using KMod;

namespace SuperElectrolyzer
{
    public class ElectrolyzerPatch : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
        }
        
        [HarmonyPatch(typeof(Electrolyzer), "OverPressure")]
        public static class ElectrolyzerOverPressurePatch
        {
            public static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }
    }
}