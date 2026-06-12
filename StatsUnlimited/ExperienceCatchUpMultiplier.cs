using UnityEngine;

namespace StatsUnlimitedSO;

internal static class ExperienceCatchUpMultiplier
{
	private static bool initialized;
	private static float maxRequired;

	public static float Get(MinionResume resume)
	{
		if (!initialized)
		{
			Refresh();
		}

		float currentRequired = GetExperienceForNextSkillPoint(resume);
		if (currentRequired <= 0f)
		{
			return 1f;
		}
		return Mathf.Max(1f, maxRequired / currentRequired);
	}

	public static void Refresh()
	{
		maxRequired = 0f;
		foreach (MinionResume minionResume in Components.MinionResumes.Items)
		{
			if (minionResume.HasTag(GameTags.Dead))
			{
				continue;
			}

			maxRequired = Mathf.Max(maxRequired, GetExperienceForNextSkillPoint(minionResume));
		}

		initialized = true;
	}

	private static float GetExperienceForNextSkillPoint(MinionResume resume)
	{
		int skillPoints = resume.TotalSkillPointsGained;
		return MinionResume.CalculateNextExperienceBar(skillPoints) - MinionResume.CalculatePreviousExperienceBar(skillPoints);
	}
}
