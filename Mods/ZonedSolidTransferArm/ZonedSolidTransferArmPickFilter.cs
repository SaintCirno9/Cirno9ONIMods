using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using KSerialization;
using TUNING;
using UnityEngine;

namespace ZonedSolidTransferArm;

[SerializationConfig(MemberSerialization.OptIn)]
public class ZonedSolidTransferArmPickFilter : KMonoBehaviour
{
    private static readonly ConcurrentDictionary<SolidTransferArm, HashSet<Tag>> AcceptedTags = new();
    private static readonly List<Tag> FilterCategories = STORAGEFILTERS.SOLID_TRANSFER_ARM_CONVEYABLE
        .Concat(STORAGEFILTERS.LIQUIDS)
        .Concat(STORAGEFILTERS.GASES)
        .Distinct()
        .ToList();

    [Serialize]
    private bool filterInitialized;

    [Serialize]
    private bool filterEnabled;

    [MyCmpReq]
    public SolidTransferArm solidTransferArm;

    [MyCmpReq]
    public TreeFilterable treeFilterable;

    public static bool AllowsPickup(SolidTransferArm arm, Pickupable pickupable)
    {
        return !AcceptedTags.TryGetValue(arm, out HashSet<Tag> tags) ||
               tags.Contains(pickupable.KPrefabID.PrefabTag);
    }

    public bool IsFilterEnabled()
    {
        return filterEnabled;
    }

    public static void Configure(GameObject gameObject)
    {
        Storage storage = gameObject.AddOrGet<Storage>();
        storage.storageFilters = FilterCategories;

        TreeFilterable filterable = gameObject.AddOrGet<TreeFilterable>();
        filterable.dropIncorrectOnFilterChange = false;
        filterable.allResourceFilterLabelString = STRINGS.UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON;
        gameObject.AddOrGet<ZonedSolidTransferArmPickFilter>();
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        if (!filterInitialized)
        {
            treeFilterable.UpdateFilters(CreateDefaultAcceptedTags());
            filterInitialized = true;
        }
        treeFilterable.OnFilterChanged += OnFilterChanged;
        SyncAcceptedTags();
    }

    protected override void OnCleanUp()
    {
        treeFilterable.OnFilterChanged -= OnFilterChanged;
        AcceptedTags.TryRemove(solidTransferArm, out _);
        base.OnCleanUp();
    }

    private void OnFilterChanged(HashSet<Tag> tags)
    {
        SyncAcceptedTags();
    }

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        Subscribe(493375141, OnRefreshUserMenu);
    }

    private void OnRefreshUserMenu(object data)
    {
        Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
            "action_switch_toggle",
            ZonedSolidTransferArmStrings.Text(filterEnabled
                ? ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.DISABLEFILTERBUTTON
                : ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ENABLEFILTERBUTTON),
            ToggleFilter,
            global::Action.NumActions,
            null,
            null,
            null,
            ZonedSolidTransferArmStrings.Text(filterEnabled
                ? ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.DISABLEFILTERBUTTONTOOLTIP
                : ZonedSolidTransferArmStrings.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.ENABLEFILTERBUTTONTOOLTIP)), 0.45f);
    }

    private void ToggleFilter()
    {
        filterEnabled = !filterEnabled;
        SyncAcceptedTags();
        Game.Instance.Trigger(1980521255, gameObject);
        DetailsScreen.Instance.Refresh(gameObject);
    }

    private void SyncAcceptedTags()
    {
        if (solidTransferArm == null || treeFilterable == null)
        {
            return;
        }

        if (filterEnabled)
        {
            AcceptedTags[solidTransferArm] = new HashSet<Tag>(treeFilterable.GetTags());
        }
        else
        {
            AcceptedTags.TryRemove(solidTransferArm, out _);
        }
    }

    private static HashSet<Tag> CreateDefaultAcceptedTags()
    {
        HashSet<Tag> tags = new();
        foreach (KPrefabID prefab in Assets.Prefabs)
        {
            if (prefab != null && prefab.HasAnyTags(FilterCategories))
            {
                tags.Add(prefab.PrefabTag);
            }
        }
        return tags;
    }
}
