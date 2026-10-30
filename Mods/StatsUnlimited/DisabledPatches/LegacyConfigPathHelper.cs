using System.IO;

namespace StatsUnlimitedSO;

public static class LegacyConfigPathHelper
{
	public static string GetConfigFileName()
	{
		return Path.Combine(Path.GetDirectoryName(typeof(ModEntry).Assembly.Location), "AttributeLevelCapConfig.TxT");
	}
}
