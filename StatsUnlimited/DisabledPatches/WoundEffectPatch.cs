#if false
using HarmonyLib;
using Klei.AI;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(Game), "OnPrefabInit")]
public static class WoundEffectPatch
{
	public static bool AlreadyPatched;

	public static void Postfix()
	{
		if (Config.Cfg.CanChangeInjuredEffect && !AlreadyPatched)
		{
			AlreadyPatched = true;
			Effect val = ((ModifierSet)Db.Get()).effects.Get("SevereWounds");
			if (val != null && Config.Cfg.SevereBonus != 0)
			{
				Class1.AddToEffect(val, Config.Cfg.SevereBonus);
			}
			Effect val2 = ((ModifierSet)Db.Get()).effects.Get("ModerateWounds");
			if (val2 != null && Config.Cfg.ModerateBonus != 0)
			{
				Class1.AddToEffect(val2, Config.Cfg.ModerateBonus);
			}
			Effect val3 = ((ModifierSet)Db.Get()).effects.Get("LightWounds");
			if (val3 != null && Config.Cfg.LightBonus != 0)
			{
				Class1.AddToEffect(val3, Config.Cfg.LightBonus);
			}
		}
	}
}
#endif
