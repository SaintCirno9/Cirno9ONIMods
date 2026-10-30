#if false
using System.Reflection;
using HarmonyLib;
using Klei.AI;

namespace StatsUnlimitedSO;

[HarmonyPatch(typeof(Game), "OnPrefabInit")]
internal class TemporalBonusEffectPatch
{
	public const string CURSEEFFECTNAME = "TemporalBonus";

	public static void Postfix()
	{
		UpdateEffect();
	}

	public static void UpdateEffect()
	{
		Effect val = (Effect)(((object)((ModifierSet)Db.Get()).effects.TryGet("TemporalBonus")) ?? ((object)new Effect("TemporalBonus", "Stats Unlimited Temporal Bonus", "Temporal Bonus", 0f, true, false, false, (string)null, 0f, (string)null, "")));
		val.SelfModifiers.Clear();
		if (Config.Cfg.CurseCreativity != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Art).Id, (float)Config.Cfg.CurseCreativity, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseAthletics != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Athletics).Id, (float)Config.Cfg.CurseAthletics, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseFarming != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Botanist).Id, (float)Config.Cfg.CurseFarming, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseKindness != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Caring).Id, (float)Config.Cfg.CurseKindness, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseConstruction != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Construction).Id, (float)Config.Cfg.CurseConstruction, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseCooking != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Cooking).Id, (float)Config.Cfg.CurseCooking, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseDigging != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Digging).Id, (float)Config.Cfg.CurseDigging, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseLearning != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Learning).Id, (float)Config.Cfg.CurseLearning, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseTinkering != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Machinery).Id, (float)Config.Cfg.CurseTinkering, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseRanching != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Ranching).Id, (float)Config.Cfg.CurseRanching, (string)null, false, false, true));
		}
		if (Config.Cfg.CurseStrength != 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Strength).Id, (float)Config.Cfg.CurseStrength, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseScaldingThreshold != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.ScaldingThreshold).Id, Config.Cfg.CurseScaldingThreshold, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseRadiationRes != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.RadiationResistance).Id, Config.Cfg.CurseRadiationRes, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseAirCom != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.AirConsumptionRate).Id, Config.Cfg.CurseAirCom, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseDecor != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Decor).Id, Config.Cfg.CurseDecor, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseCarryAmount != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.CarryAmount).Id, Config.Cfg.CurseCarryAmount, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseSpaceNavigation != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.SpaceNavigation).Id, Config.Cfg.CurseSpaceNavigation, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseFoodExp != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.FoodExpectation).Id, Config.Cfg.CurseFoodExp, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseDiseaseCureSpd != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.DiseaseCureSpeed).Id, Config.Cfg.CurseDiseaseCureSpd, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseMorale != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.QualityOfLife).Id, Config.Cfg.CurseMorale, (string)null, false, false, true));
		}
		if ((double)Config.Cfg.CurseGermResistance != 0.0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.GermResistance).Id, Config.Cfg.CurseGermResistance, (string)null, false, false, true));
		}
		if (val.SelfModifiers.Count == 0)
		{
			val.Add(new AttributeModifier(((Resource)((ModifierSet)Db.Get()).Attributes.Athletics).Id, 0f, (string)null, false, false, true));
		}
		((ModifierSet)Db.Get()).effects.Add(val);
		Debug.Log((object)("TemporalBonus effect is added to game." + Assembly.GetExecutingAssembly().Location + "."));
	}
}
#endif
