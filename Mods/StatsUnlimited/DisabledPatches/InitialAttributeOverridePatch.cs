#if false
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(MinionStartingStats), "ApplyExperience")]
public static class InitialAttributeOverridePatch
{
	public static bool Prefix(MinionStartingStats __instance, GameObject go)
	{
		if (!Config.Cfg.CanChangeDuplicantInitialStat)
		{
			return true;
		}
		AttributeLevels component = go.GetComponent<AttributeLevels>();
		foreach (AttributeLevel item in component)
		{
			if (item.attribute.Attribute.IsTrainable && item.attribute.Attribute.IsProfession && Config.Cfg.CanChangeDuplicantInitialStat)
			{
				component.SetLevel(item.attribute.Id, Config.Cfg.DuplicantInitialStats);
			}
		}
		return false;
	}
}
#endif
