using System.Collections.Concurrent;
using KSerialization;
using UnityEngine;

namespace ZonedSolidTransferArm;

[SerializationConfig(MemberSerialization.OptIn)]
public class ZonedSolidTransferArmTemperatureFilter : KMonoBehaviour
{
    private readonly struct TemperatureRange
    {
        public readonly float MinK;
        public readonly float MaxK;

        public TemperatureRange(float minK, float maxK)
        {
            MinK = minK;
            MaxK = maxK;
        }

        public bool Contains(float temperatureK)
        {
            return temperatureK >= MinK && temperatureK <= MaxK;
        }
    }

    private const float DefaultMinTemperatureK = 0f;
    private const float DefaultMaxTemperatureK = 1000f;
    private static readonly ConcurrentDictionary<SolidTransferArm, TemperatureRange> TemperatureRanges = new();

    [Serialize]
    private bool temperatureFilterEnabled;

    [Serialize]
    private float minTemperatureK = DefaultMinTemperatureK;

    [Serialize]
    private float maxTemperatureK = DefaultMaxTemperatureK;

    [MyCmpReq]
    public SolidTransferArm solidTransferArm;

    public float MinTemperatureK => minTemperatureK;

    public float MaxTemperatureK => maxTemperatureK;

    public static bool AllowsPickup(SolidTransferArm arm, Pickupable pickupable)
    {
        if (!TemperatureRanges.TryGetValue(arm, out TemperatureRange range))
        {
            return true;
        }

        PrimaryElement primaryElement = pickupable.GetComponent<PrimaryElement>();
        return primaryElement != null && range.Contains(primaryElement.Temperature);
    }

    public bool IsTemperatureFilterEnabled()
    {
        return temperatureFilterEnabled;
    }

    public void SetMinTemperature(float valueK)
    {
        minTemperatureK = valueK;
        SyncTemperatureRange();
    }

    public void SetMaxTemperature(float valueK)
    {
        maxTemperatureK = valueK;
        SyncTemperatureRange();
    }

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        Subscribe(493375141, OnRefreshUserMenu);
        Subscribe(-905833192, OnCopySettings);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        SyncTemperatureRange();
    }

    protected override void OnCleanUp()
    {
        TemperatureRanges.TryRemove(solidTransferArm, out _);
        base.OnCleanUp();
    }

    private void OnRefreshUserMenu(object data)
    {
        Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
            "action_switch_toggle",
            Strings.Get(temperatureFilterEnabled
                ? "STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.DISABLETEMPERATUREFILTERBUTTON"
                : "STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ENABLETEMPERATUREFILTERBUTTON"),
            ToggleTemperatureFilter,
            global::Action.NumActions,
            null,
            null,
            null,
            Strings.Get(temperatureFilterEnabled
                ? "STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.DISABLETEMPERATUREFILTERBUTTONTOOLTIP"
                : "STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ENABLETEMPERATUREFILTERBUTTONTOOLTIP")), 0.45f);
    }

    private void ToggleTemperatureFilter()
    {
        temperatureFilterEnabled = !temperatureFilterEnabled;
        SyncTemperatureRange();
        Game.Instance.Trigger(1980521255, gameObject);
        DetailsScreen.Instance.Refresh(gameObject);
    }

    private void OnCopySettings(object data)
    {
        if (data is not GameObject go ||
            go.GetComponent<ZonedSolidTransferArmTemperatureFilter>() is not { } sourceFilter)
        {
            return;
        }

        temperatureFilterEnabled = sourceFilter.temperatureFilterEnabled;
        minTemperatureK = sourceFilter.minTemperatureK;
        maxTemperatureK = sourceFilter.maxTemperatureK;
        SyncTemperatureRange();
    }

    private void SyncTemperatureRange()
    {
        if (solidTransferArm == null)
        {
            return;
        }

        if (temperatureFilterEnabled)
        {
            TemperatureRanges[solidTransferArm] = new TemperatureRange(minTemperatureK, maxTemperatureK);
        }
        else
        {
            TemperatureRanges.TryRemove(solidTransferArm, out _);
        }
    }
}
