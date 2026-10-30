#if false
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(MinionStartingStats), "Deliver")]
public static class DuplicantHealthSetupPatch
{
	public static void Postfix(GameObject __result)
	{
		if ((Object)(object)__result.GetComponent<MinionResume>() != (Object)null)
		{
			SetHitPoint(__result.gameObject, IsStarting: false);
		}
	}

	public static void SetHitPoint(GameObject gameObject, bool IsStarting)
	{
		if (Config.Cfg.CanAddRegenerativeTrait)
		{
			Traits component = gameObject.GetComponent<Traits>();
			if (!component.HasTrait("Regeneration"))
			{
				component.Add(((ResourceSet<Trait>)(object)((ModifierSet)Db.Get()).traits).Get("Regeneration"));
			}
		}
		Health component2 = gameObject.GetComponent<Health>();
		if (!((Object)(object)component2 != (Object)null))
		{
			return;
		}
		Traverse.Create((object)component2).Field("effects").SetValue((object)gameObject.GetComponent<Effects>());
		float num = Config.Cfg.DuplicantInitialHp;
		if (!((double)num >= 0.0))
		{
			return;
		}
		if ((double)num == 0.0 && IsStarting)
		{
			num = 1f;
		}
		if ((double)num > 100.0)
		{
			num = 100f;
		}
		if ((double)num < 100.0)
		{
			component2.Damage(100f - num);
			if ((double)num == 0.0)
			{
				component2.Incapacitate(GameTags.RadiationSicknessIncapacitation);
			}
		}
	}
}
#endif
