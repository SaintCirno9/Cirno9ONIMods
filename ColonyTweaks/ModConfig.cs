using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ColonyTweaks;

// ColonyTweaks 的 PLib 配置。
//
// 当前仅一个作弊开关，控制动物吃复制人食物时的排泄产出量。
// 食谱在加载期由 DietManager 收集，改开关需重启游戏才能生效，故标记 [RestartRequired]。
[JsonObject(MemberSerialization.OptIn)]
[ConfigFile("ColonyTweaks.json", true, true)]
[RestartRequired]
public sealed class ModConfig : SingletonOptions<ModConfig>, IOptions
{
    [Option("复制人食物喂养：对齐原版产出", "开启后，动物吃复制人食物的排泄量对齐到其原版食谱的最大产出质量（作弊）。默认关闭。")]
    [JsonProperty]
    public bool BoostCritterPoopFromDuplicantFood { get; set; } = false;

    public IEnumerable<IOptionsEntry> CreateOptions() => null;

    public void OnOptionsChanged() { }
}
