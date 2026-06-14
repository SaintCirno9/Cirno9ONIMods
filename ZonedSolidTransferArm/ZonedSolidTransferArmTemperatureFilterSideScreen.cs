using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace ZonedSolidTransferArm;

public class ZonedSolidTransferArmTemperatureFilterSideScreen : SideScreenContent
{
    private ZonedSolidTransferArmTemperatureFilter targetFilter;
    private TMP_InputField minInput;
    private TMP_InputField maxInput;

    public override string GetTitle()
    {
        return Strings.Get("STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.TEMPERATUREFILTERTITLE");
    }

    public override bool IsValidForTarget(GameObject target)
    {
        return target != null &&
               target.GetComponent<ZonedSolidTransferArmTemperatureFilter>() is { } filter &&
               filter.IsTemperatureFilterEnabled();
    }

    public override void SetTarget(GameObject target)
    {
        targetFilter = target == null ? null : target.GetComponent<ZonedSolidTransferArmTemperatureFilter>();
        RefreshInputs();
    }

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        PPanel panel = new("ZonedSolidTransferArmTemperatureFilterPanel")
        {
            Direction = PanelDirection.Vertical,
            Spacing = 10
        };

        panel.AddChild(CreateLabel("MinTemperatureLabel",
            Strings.Get("STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.MINTEMPERATURELABEL") +
            " " +
            GameUtil.GetTemperatureUnitSuffix()));
        panel.AddChild(CreateTemperatureField(
            "MinTemperatureField",
            Strings.Get("STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.MINTEMPERATURETOOLTIP"),
            value => targetFilter?.SetMinTemperature(ConvertInputToKelvin(value)),
            input => minInput = input));
        panel.AddChild(CreateLabel("MaxTemperatureLabel",
            Strings.Get("STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.MAXTEMPERATURELABEL") +
            " " +
            GameUtil.GetTemperatureUnitSuffix()));
        panel.AddChild(CreateTemperatureField(
            "MaxTemperatureField",
            Strings.Get("STRINGS.UI.UISIDESCREENS.ZONEDSOLIDTRANSFERARMCONTROL.MAXTEMPERATURETOOLTIP"),
            value => targetFilter?.SetMaxTemperature(ConvertInputToKelvin(value)),
            input => maxInput = input));

        ContentContainer = panel.AddTo(gameObject, 0);
    }

    private static PLabel CreateLabel(string name, string text)
    {
        return new PLabel(name)
        {
            Text = text,
            TextStyle = PUITuning.Fonts.TextDarkStyle
        };
    }

    private static PTextField CreateTemperatureField(
        string name,
        string tooltip,
        System.Action<float> onValueChanged,
        System.Action<TMP_InputField> onRealize)
    {
        return new PTextField(name)
        {
            Type = PTextField.FieldType.Float,
            MaxLength = 16,
            MinWidth = 96,
            ToolTip = tooltip,
            OnTextChanged = (_, text) =>
            {
                if (float.TryParse(text, out float value))
                {
                    onValueChanged(value);
                }
            }
        }.AddOnRealize(go => onRealize(go.GetComponent<TMP_InputField>()));
    }

    private void RefreshInputs()
    {
        if (targetFilter == null)
        {
            return;
        }

        if (minInput != null)
        {
            minInput.text = ConvertToPreferredUnit(targetFilter.MinTemperatureK).ToString("F1");
        }
        if (maxInput != null)
        {
            maxInput.text = ConvertToPreferredUnit(targetFilter.MaxTemperatureK).ToString("F1");
        }
    }

    private static float ConvertToPreferredUnit(float kelvin)
    {
        return (int)GameUtil.temperatureUnit switch
        {
            0 => kelvin - 273.15f,
            1 => (kelvin - 273.15f) * 9f / 5f + 32f,
            _ => kelvin
        };
    }

    private static float ConvertInputToKelvin(float input)
    {
        return (int)GameUtil.temperatureUnit switch
        {
            0 => input + 273.15f,
            1 => (input - 32f) * 5f / 9f + 273.15f,
            _ => input
        };
    }
}

[HarmonyLib.HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
public static class DetailsScreenOnPrefabInitTemperatureFilterPatch
{
    public static void Postfix()
    {
        PUIUtils.AddSideScreenContent<ZonedSolidTransferArmTemperatureFilterSideScreen>();
    }
}
