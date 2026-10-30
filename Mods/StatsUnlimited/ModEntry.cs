using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace StatsUnlimitedSO;

public class ModEntry : UserMod2
{
	public override void OnLoad(Harmony harmony)
	{
		base.OnLoad(harmony);
		PUtil.InitLibrary(logVersion: false);
		new POptions().RegisterOptions(this, typeof(ModConfig));
		Debug.Log((object)"[StatsUnlimited] Loaded.");
	}
}
