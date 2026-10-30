using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;
using UnityEngine;

namespace StatsUnlimitedSO;

[JsonObject]
[ConfigFile("StatsUnlimited.json", true, true)]
public class ModConfig : IOptions
{
	public const int CURRENT_VER = 8;

	[NonSerialized]
	private static ModConfig cfg;

	[JsonProperty]
	public int Ver { get; set; } = CURRENT_VER;

	[Option("Attribute Level Cap", "Maximum level trainable attributes can reach.", "1 Attributes")]
	[Limit(1.0, 1000.0)]
	[JsonProperty]
	public int AttributeLevelCap { get; set; } = 50;

	[Option("Attribute Experience Multiplier", "Multiplier for trainable attribute experience gain.", "2 Experience")]
	[Limit(0.0, 1000.0)]
	[JsonProperty]
	public float AttributeExperienceMultiplier { get; set; } = 1f;

	[Option("Skill Point Experience Multiplier", "Multiplier for skill point experience gain.", "2 Experience")]
	[Limit(0.0, 1000.0)]
	[JsonProperty]
	public float SkillPointExperienceMultiplier { get; set; } = 1f;

	public static ModConfig Cfg
	{
		get
		{
			if (cfg != null)
			{
				return cfg;
			}
			cfg = POptions.ReadSettings<ModConfig>();
			if (cfg == null)
			{
				cfg = new ModConfig();
				Save();
			}
			return cfg;
		}
	}

	public static void Save()
	{
		try
		{
			POptions.WriteSettings(Cfg);
		}
		catch (Exception ex)
		{
			Debug.Log((object)("StatsUnlimited: can not save config, Exception: " + ex));
		}
	}

	public IEnumerable<IOptionsEntry> CreateOptions()
	{
		return Array.Empty<IOptionsEntry>();
	}

	public void OnOptionsChanged()
	{
		cfg = this;
	}
}
