using HarmonyLib;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(MinionResume), "AddExperience")]
public static class SkillPointExperiencePatch
{
	public static bool TurnOn = true;

	public static void Prefix(MinionResume __instance, ref float amount)
	{
		if (TurnOn)
		{
			amount *= ModConfig.Cfg.SkillPointExperienceMultiplier * ExperienceCatchUpMultiplier.Get(__instance);
		}
	}
}

[HarmonyPatch(typeof(MinionResume), "OnSkillPointGained")]
public static class SkillPointGainedPatch
{
	public static void Postfix()
	{
		ExperienceCatchUpMultiplier.Refresh();
	}
}
