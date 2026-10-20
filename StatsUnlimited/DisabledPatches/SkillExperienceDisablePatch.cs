#if false
using HarmonyLib;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(MinionResume), "ForceAddSkillPoint")]
public static class SkillExperienceDisablePatch
{
	public static void Prefix()
	{
		SkillExp.TurnOn = false;
	}

	public static void Postfix()
	{
		SkillExp.TurnOn = true;
	}
}
#endif
