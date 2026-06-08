using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace ResourceSensor;

public class ResourceSensorMod : UserMod2
{
	public override void OnLoad(Harmony harmony)
	{
		base.OnLoad(harmony);
		PUtil.InitLibrary();
		LocString.CreateLocStringKeys(typeof(UI.UISIDESCREENS));
	}
}
