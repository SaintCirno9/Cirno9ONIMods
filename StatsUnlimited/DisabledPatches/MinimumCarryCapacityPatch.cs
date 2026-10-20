#if false
using System;
using HarmonyLib;
using Klei.AI;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(AttributeInstance), "GetTotalValue", new Type[] { })]
public static class MinimumCarryCapacityPatch
{
	public static void Postfix(AttributeInstance __instance, ref float __result)
	{
		if (Config.Cfg.IsMinimumCarryWeightEnabled && ((Resource)__instance.Attribute).IdHash == ((Resource)((ModifierSet)Db.Get()).Attributes.CarryAmount).IdHash && !((double)__result >= (double)Config.Cfg.MinimumCarryWeight))
		{
			__result = Config.Cfg.MinimumCarryWeight;
		}
	}
}
#endif
