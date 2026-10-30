#if false
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(Game), "OnSpawn")]
public static class TemporalBonusApplicationPatch
{
	public static void Postfix()
	{
		Components.LiveMinionIdentities.OnAdd += LiveMinionIdentities_OnAdd;
		Config.Reload();
		TemporalBonusEffectPatch.UpdateEffect();
	}

	private static void LiveMinionIdentities_OnAdd(MinionIdentity obj)
	{
		if (Config.Cfg.IsCurseEffectEnabled)
		{
			Effects component = ((Component)obj).GetComponent<Effects>();
			if ((Object)(object)component != (Object)null && component.Get("TemporalBonus") == null)
			{
				component.Add("TemporalBonus", false);
			}
		}
	}
}
#endif
