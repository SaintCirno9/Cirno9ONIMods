#if false
using System;
using HarmonyLib;
using UnityEngine;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(NewBaseScreen), "SpawnMinions", new Type[] { typeof(GameObject) })]
public static class StartingDuplicantHealthPatch
{
	public static void Postfix()
	{
		GameScheduler.Instance.Schedule("Hit", 3f, (Action<object>)delegate
		{
			foreach (MinionIdentity item in Components.LiveMinionIdentities.Items)
			{
				DuplicantHealthSetupPatch.SetHitPoint(((Component)item).gameObject, IsStarting: true);
			}
		}, (object)null, (SchedulerGroup)null);
		foreach (MinionIdentity item2 in Components.LiveMinionIdentities.Items)
		{
			DuplicantHealthSetupPatch.SetHitPoint(((Component)item2).gameObject, IsStarting: true);
		}
	}
}
#endif
