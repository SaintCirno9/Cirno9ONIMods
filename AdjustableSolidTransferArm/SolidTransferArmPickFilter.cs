using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using KSerialization;
using TUNING;
using UnityEngine;

namespace AdjustableSolidTransferArm;

public class SolidTransferArmPickFilter : KMonoBehaviour, ISidescreenButtonControl
{
    private static readonly ConcurrentDictionary<SolidTransferArm, HashSet<Tag>> AcceptedTags = new();
    private static readonly List<Tag> FilterCategories = STORAGEFILTERS.SOLID_TRANSFER_ARM_CONVEYABLE
        .Concat(STORAGEFILTERS.LIQUIDS)
        .Concat(STORAGEFILTERS.GASES)
        .Distinct()
        .ToList();

    [Serialize] public bool filterInitialized;
    [Serialize] public bool enableFilterSideScreen;

    public static bool AllowsPickup(SolidTransferArm arm, Pickupable pickupable)
    {
        return !AcceptedTags.TryGetValue(arm, out var tags) ||
               tags.Contains(pickupable.KPrefabID.PrefabTag);
    }

    public static void Configure(GameObject gameObject)
    {
        Storage storage = gameObject.AddOrGet<Storage>();
        storage.storageFilters = FilterCategories;

        TreeFilterable filterable = gameObject.AddOrGet<TreeFilterable>();
        filterable.dropIncorrectOnFilterChange = false;
        filterable.allResourceFilterLabelString = STRINGS.UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON;
        gameObject.AddOrGet<SolidTransferArmPickFilter>();
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
        OnFilterChanged(treeFilterable.GetTags());
    }

    protected override void OnCleanUp()
    {
        treeFilterable.OnFilterChanged -= OnFilterChanged;
        AcceptedTags.TryRemove(solidTransferArm, out _);
        base.OnCleanUp();
    }

    private void OnFilterChanged(HashSet<Tag> tags)
    {
        AcceptedTags[solidTransferArm] = new HashSet<Tag>(tags);
    }

    public bool ShouldShowFilterSideScreen()
    {
        return enableFilterSideScreen;
    }

    public string SidescreenButtonText =>
        Strings.Get(enableFilterSideScreen
            ? "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.HIDEFILTERBUTTON"
            : "STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.SHOWFILTERBUTTON");

    public string SidescreenButtonTooltip =>
        Strings.Get("STRINGS.UI.UISIDESCREENS.SOLIDTRANSFERARMCONTROLUISIDESCREEN.SHOWFILTERBUTTONTOOLTIP");

    public string SidescreenTitle => SidescreenButtonText;

    public void SetButtonTextOverride(ButtonMenuTextOverride textOverride)
    {
    }

    bool ISidescreenButtonControl.SidescreenEnabled()
    {
        return true;
    }

    public bool SidescreenButtonInteractable()
    {
        return true;
    }

    public void OnSidescreenButtonPressed()
    {
        enableFilterSideScreen = !enableFilterSideScreen;
        RefreshSelected();
    }

    public int HorizontalGroupID()
    {
        return -1;
    }

    public int ButtonSideScreenSortOrder()
    {
        return 1;
    }

    private static HashSet<Tag> CreateDefaultAcceptedTags()
    {
        var tags = new HashSet<Tag>();
        foreach (var prefab in Assets.Prefabs)
        {
            if (prefab != null && prefab.HasAnyTags(FilterCategories))
            {
                tags.Add(prefab.PrefabTag);
            }
        }
        return tags;
    }

    [MyCmpReq] public SolidTransferArm solidTransferArm;
    [MyCmpReq] public TreeFilterable treeFilterable;

    private void RefreshSelected()
    {
        var selectable = GetComponent<KSelectable>();
        if (selectable != null && selectable.IsSelected)
        {
            SelectTool.Instance.Select(null, true);
            SelectTool.Instance.Select(selectable, true);
        }
    }
}
