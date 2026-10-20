using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace BottleEmptierThreshold;

[SerializationConfig(MemberSerialization.OptIn)]
public sealed class BottleEmptierThresholdControl : KMonoBehaviour, IIntSliderControl
{
    public const int DefaultRefillBelowPercent = 0;

    [MyCmpReq]
    private BottleEmptier bottleEmptier;

    [MyCmpReq]
    private Storage storage;

    [MyCmpReq]
    private Operational operational;

    [Serialize]
    private int refillBelowPercent = DefaultRefillBelowPercent;

    private FetchChore refillChore;

    private static readonly EventSystem.IntraObjectHandler<BottleEmptierThresholdControl> OnCopySettingsDelegate =
        new((component, data) => component.OnCopySettings(data));

    private static readonly EventSystem.IntraObjectHandler<BottleEmptierThresholdControl> OnStorageChangeDelegate =
        new((component, data) => component.OnStorageChange(data));

    private static readonly EventSystem.IntraObjectHandler<BottleEmptierThresholdControl> OnOperationalChangedDelegate =
        new((component, data) => component.OnOperationalChanged(data));

    public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.BOTTLEEMPTIERTHRESHOLD.TITLE";

    public string SliderUnits => UI.UNITSUFFIXES.PERCENT;

    public int SliderDecimalPlaces(int index)
    {
        return 0;
    }

    public float GetSliderMin(int index)
    {
        return 0f;
    }

    public float GetSliderMax(int index)
    {
        return 100f;
    }

    public float GetSliderValue(int index)
    {
        return refillBelowPercent;
    }

    public void SetSliderValue(float value, int index)
    {
        int newValue = Mathf.Clamp(Mathf.RoundToInt(value), 0, 100);
        if (newValue == refillBelowPercent)
        {
            return;
        }

        refillBelowPercent = newValue;
        RefreshFetchChore();
    }

    public string GetSliderTooltipKey(int index)
    {
        return "STRINGS.UI.UISIDESCREENS.BOTTLEEMPTIERTHRESHOLD.TOOLTIP";
    }

    public string GetSliderTooltip(int index)
    {
        return string.Format(
            "{0}\n{1}: {2}{3}",
            Strings.Get("STRINGS.UI.UISIDESCREENS.BOTTLEEMPTIERTHRESHOLD.TOOLTIP"),
            "补充下限",
            refillBelowPercent,
            UI.UNITSUFFIXES.PERCENT);
    }

    public float RefillBelowMass => storage.capacityKg * refillBelowPercent / 100f;

    public bool ShouldRefill => operational.IsOperational && storage.ExactMassStored() <= RefillBelowMass && storage.RemainingCapacity() > 0f;

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        Subscribe((int)GameHashes.OnStorageChange, OnStorageChangeDelegate);
        Subscribe((int)GameHashes.OperationalChanged, OnOperationalChangedDelegate);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        RefreshFetchChore();
    }

    protected override void OnCleanUp()
    {
        CancelRefillChore("CleanUp");
        base.OnCleanUp();
    }

    public void RefreshFetchChore()
    {
        if (!ShouldRefill)
        {
            CancelRefillChore("No refill needed");
            return;
        }

        if (refillChore != null && !refillChore.isComplete)
        {
            return;
        }

        CreateRefillChore();
    }

    private void CreateRefillChore()
    {
        CancelRefillChore("Refresh");

        HashSet<Tag> tags = GetComponent<TreeFilterable>().GetTags();
        Tag[] forbiddenTags = bottleEmptier.allowManualPumpingStationFetching
            ? System.Array.Empty<Tag>()
            : new[]
            {
                GameTags.LiquidSource,
                GameTags.GasSource
            };

        float amount = storage.RemainingCapacity();
        if (amount <= 0f || tags.Count == 0)
        {
            return;
        }

        refillChore = new FetchChore(
            Db.Get().ChoreTypes.StorageFetch,
            storage,
            amount,
            tags,
            FetchChore.MatchCriteria.MatchID,
            Tag.Invalid,
            forbiddenTags);
    }

    private void CancelRefillChore(string reason)
    {
        if (refillChore == null)
        {
            return;
        }

        refillChore.Cancel(reason);
        refillChore = null;
    }

    private void OnCopySettings(object data)
    {
        var source = (data as GameObject)?.GetComponent<BottleEmptierThresholdControl>();
        if (source == null)
        {
            return;
        }

        refillBelowPercent = source.refillBelowPercent;
        RefreshFetchChore();
    }

    private void OnStorageChange(object data)
    {
        if (refillChore != null && refillChore.isComplete)
        {
            refillChore = null;
        }

        RefreshFetchChore();
    }

    private void OnOperationalChanged(object data)
    {
        RefreshFetchChore();
    }
}
