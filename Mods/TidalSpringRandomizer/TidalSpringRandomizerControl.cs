using System;
using KSerialization;
using UnityEngine;

namespace TidalSpringRandomizer;

[SerializationConfig(MemberSerialization.OptIn)]
public sealed class TidalSpringRandomizerControl : KMonoBehaviour
{
    private const float MinMultiplier = 1f;
    private const float MaxMultiplier = 3f;

    [Serialize]
    private float multiplier = -1f;

    private bool applied;
    private SchedulerHandle retryHandle;

    protected override void OnSpawn()
    {
        base.OnSpawn();

        if (multiplier <= 0f)
        {
            multiplier = RollMultiplier();
        }

        ApplyMultiplier();
    }

    private void ApplyMultiplier()
    {
        if (applied)
        {
            return;
        }

        if (!TryGetTargets(out Storage storage, out ElementConsumer elementConsumer, out BreathingGeyser.Instance smi))
        {
            ScheduleRetry();
            return;
        }

        retryHandle.ClearScheduler();

        BreathingGeyser.Def sourceDef = smi.def;
        BreathingGeyser.Def clonedDef = new BreathingGeyser.Def
        {
            inhaleRate = sourceDef.inhaleRate,
            exhaleRate = sourceDef.exhaleRate,
            diseaseIdx = sourceDef.diseaseIdx,
            germsPerKg = sourceDef.germsPerKg
        };
        clonedDef.inhaleRate = sourceDef.inhaleRate * multiplier;
        clonedDef.exhaleRate = sourceDef.exhaleRate * multiplier;
        smi.def = clonedDef;

        storage.capacityKg *= multiplier;
        elementConsumer.capacityKG *= multiplier;
        elementConsumer.consumptionRate = clonedDef.inhaleRate;

        applied = true;
    }

    protected override void OnCleanUp()
    {
        retryHandle.ClearScheduler();
        base.OnCleanUp();
    }

    private bool TryGetTargets(out Storage storage, out ElementConsumer elementConsumer, out BreathingGeyser.Instance smi)
    {
        storage = GetComponent<Storage>();
        elementConsumer = GetComponent<ElementConsumer>();
        smi = GetComponent<StateMachineController>()?.GetSMI<BreathingGeyser.Instance>();
        return storage != null && elementConsumer != null && smi != null;
    }

    private float RollMultiplier()
    {
        int seed = 0;
        SaveLoader saveLoader = SaveLoader.Instance;
        if (saveLoader != null && saveLoader.clusterDetailSave != null)
        {
            seed = saveLoader.clusterDetailSave.globalWorldSeed;
        }

        int worldId = gameObject.GetMyWorldId();
        Vector3 position = transform.GetPosition();
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int hash = seed;
        hash = hash * 397 ^ worldId;
        hash = hash * 397 ^ x;
        hash = hash * 397 ^ y;

        System.Random random = new System.Random(hash);
        return (float)(MinMultiplier + random.NextDouble() * (MaxMultiplier - MinMultiplier));
    }

    private void ScheduleRetry()
    {
        if (applied || retryHandle.IsValid || GameScheduler.Instance == null)
        {
            return;
        }

        retryHandle = GameScheduler.Instance.ScheduleNextFrame(
            "TidalSpringRandomizer.ApplyMultiplier",
            _ =>
            {
                retryHandle = default;
                if (this != null)
                {
                    ApplyMultiplier();
                }
            });
    }
}
