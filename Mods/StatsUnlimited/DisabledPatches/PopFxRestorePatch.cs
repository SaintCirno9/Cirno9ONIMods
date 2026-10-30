#if false
using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(PopFXManager), "SpawnFX", new Type[]
{
	typeof(Sprite),
	typeof(string),
	typeof(Transform),
	typeof(Vector3),
	typeof(float),
	typeof(bool),
	typeof(bool)
})]
public static class PopFxRestorePatch
{
	public static void Postfix(PopFX __result)
	{
		if ((UnityEngine.Object)(object)__result != (UnityEngine.Object)null)
		{
			((TMP_Text)__result.TextDisplay).fontSize = 24f;
			KMonoBehaviourExtensions.SetAlpha(__result.IconDisplay, 1f);
		}
	}
}
#endif
