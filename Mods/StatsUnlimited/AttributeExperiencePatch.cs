using HarmonyLib;
using Klei.AI;
using TUNING;
using UnityEngine;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(AttributeLevel), "AddExperience")]
public static class AttributeExperiencePatch
{
	public static bool Prefix(ref bool __result, AttributeLevel __instance, AttributeLevels levels, float experience)
	{
		ModConfig config = ModConfig.Cfg;
		if (__instance.level >= config.AttributeLevelCap)
		{
			__result = false;
			return false;
		}

		MinionResume resume = levels.GetComponent<MinionResume>();
		__instance.experience += experience * config.AttributeExperienceMultiplier * ExperienceCatchUpMultiplier.Get(resume);
		__instance.experience = Mathf.Max(0f, __instance.experience);
		float experienceForNextLevel = GetExperienceForNextLevel(__instance.level);
		if (__instance.experience >= experienceForNextLevel)
		{
			__instance.LevelUp(levels);
			__result = true;
			return false;
		}

		__result = false;
		return false;
	}

	public static void Init()
	{
	}

	private static float GetExperienceForNextLevel(int level)
	{
		float current = Mathf.Pow((float)level / DUPLICANTSTATS.ATTRIBUTE_LEVELING.MAX_GAINED_ATTRIBUTE_LEVEL, DUPLICANTSTATS.ATTRIBUTE_LEVELING.EXPERIENCE_LEVEL_POWER) * DUPLICANTSTATS.ATTRIBUTE_LEVELING.TARGET_MAX_LEVEL_CYCLE * 600f;
		float next = Mathf.Pow((level + 1f) / DUPLICANTSTATS.ATTRIBUTE_LEVELING.MAX_GAINED_ATTRIBUTE_LEVEL, DUPLICANTSTATS.ATTRIBUTE_LEVELING.EXPERIENCE_LEVEL_POWER) * DUPLICANTSTATS.ATTRIBUTE_LEVELING.TARGET_MAX_LEVEL_CYCLE * 600f;
		return next - current;
	}
}
