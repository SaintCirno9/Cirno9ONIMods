using Harmony;

namespace FunctionEnhancement
{
    public class MopPatch
    {
        [HarmonyPatch(typeof(MopTool),"OnPrefabInit")]
        public class MopToolOnPrefabInitPatch
        {
            public static void Postfix()
            {
                MopTool.maxMopAmt = float.MaxValue;
            }
        }
    }
}