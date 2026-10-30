using UnityEngine;
using System;
using PeterHan.PLib.UI;
using TMPro;

namespace DeliveryTemperatureLimit
{
    public class TemperatureLimitWidget : KMonoBehaviour
    {
        private GameObject lowInput;
        private GameObject highInput;
        private TMP_InputField lowField;
        private TMP_InputField highField;
        private TemperatureLimit target;
        private bool isUpdating;

        protected override void OnPrefabInit()
        {
            var margin = new RectOffset(4, 4, 4, 4);
            var baseLayout = gameObject.GetComponent<BoxLayoutGroup>();
            if (baseLayout != null)
                baseLayout.Params = new BoxLayoutParams()
                {
                    Alignment = TextAnchor.MiddleLeft,
                    Margin = margin,
                };
            PPanel panel = new PPanel("MainPanel")
            {
                Direction = PanelDirection.Horizontal,
                Margin = margin,
                Spacing = 4,
                FlexSize = Vector2.right
            };
            PTextField lowInputField = new PTextField("lowLimit")
            {
                Type = PTextField.FieldType.Integer,
                OnTextChanged = OnTextChangedLow,
                MinWidth = 72
            };
            lowInputField.AddOnRealize(obj =>
            {
                lowInput = obj;
                lowField = obj.GetComponent<TMP_InputField>();
            });
            PTextField highInputField = new PTextField("highLimit")
            {
                Type = PTextField.FieldType.Integer,
                OnTextChanged = OnTextChangedHigh,
                MinWidth = 72
            };
            highInputField.AddOnRealize(obj =>
            {
                highInput = obj;
                highField = obj.GetComponent<TMP_InputField>();
            });
            panel.AddChild(new PLabel("label")
            {
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                Text = STRINGS.TEMPERATURELIMIT.LABEL
            });
            panel.AddChild(lowInputField);
            panel.AddChild(new PLabel("separator")
            {
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                Text = STRINGS.TEMPERATURELIMIT.RANGE_SEPARATOR
            });
            panel.AddChild(highInputField);
            panel.AddTo(gameObject);
            base.OnPrefabInit();
            UpdateInputs();
        }

        public void SetTarget(TemperatureLimit newTarget)
        {
            target = newTarget;
            UpdateInputs();
        }

        private void UpdateInputs()
        {
            if( target == null || lowField == null || highField == null )
                return;
            // 显示刷新不写回配置，也不触发输入回调，避免清空和自动补值互相递归。
            bool disabled = target.IsDisabled();
            SetInputText( lowField, disabled || target.LowLimit == TemperatureLimit.MinValue
                ? -1 : target.LowLimit );
            SetInputText( highField, disabled || target.HighLimit == TemperatureLimit.MaxValue
                ? -1 : target.HighLimit );
            UpdateToolTip();
        }

        private static void SetInputText(TMP_InputField field, int value)
        {
            string text = value < 0 ? "" : GameUtil.GetFormattedTemperature(value, GameUtil.TimeSlice.None,
                GameUtil.TemperatureInterpretation.Absolute, true, true);
            field.SetTextWithoutNotify(text);
        }

        private void OnTextChangedLow(GameObject source, string text)
        {
            if( isUpdating || target == null )
                return;
            isUpdating = true;
            try
            {
                int value = ParseTemperature(text);
                if( value < 0 )
                {
                    target.SetLowLimit(TemperatureLimit.MinValue);
                    if( target.HighLimit == TemperatureLimit.MaxValue || target.IsDisabled())
                        target.Disable();
                }
                else
                {
                    target.SetLowLimit(value);
                    if( target.IsDisabled())
                        target.SetHighLimit(TemperatureLimit.MaxValue);
                    else if( target.LowLimit > target.HighLimit )
                        target.SetHighLimit(target.LowLimit);
                }
                UpdateInputs();
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void OnTextChangedHigh(GameObject source, string text)
        {
            if( isUpdating || target == null )
                return;
            isUpdating = true;
            try
            {
                int value = ParseTemperature(text);
                if( value < 0 )
                {
                    target.SetHighLimit(TemperatureLimit.MaxValue);
                    if( target.LowLimit == TemperatureLimit.MinValue )
                        target.Disable();
                }
                else
                {
                    if( target.IsDisabled())
                        target.SetLowLimit(TemperatureLimit.MinValue);
                    target.SetHighLimit(value);
                    if( target.HighLimit < target.LowLimit )
                        target.SetLowLimit(target.HighLimit);
                }
                UpdateInputs();
            }
            finally
            {
                isUpdating = false;
            }
        }

        private static int ParseTemperature(string text)
        {
            text = text.Trim();
            if( text.EndsWith(GameUtil.GetTemperatureUnitSuffix()))
                text = text.Remove(text.Length - GameUtil.GetTemperatureUnitSuffix().Length);
            if( !int.TryParse(text, out int result))
                return -1;
            int kelvin = (int)Math.Round(GameUtil.GetTemperatureConvertedToKelvin(result));
            return Math.Max(TemperatureLimit.MinValue, Math.Min(kelvin, TemperatureLimit.MaxValue));
        }

        private void UpdateToolTip()
        {
            string tooltip;
            if( !target.IsDisabled())
                tooltip = string.Format(STRINGS.TEMPERATURELIMIT.TOOLTIP_RANGE,
                    GameUtil.GetFormattedTemperature(target.LowLimit, GameUtil.TimeSlice.None,
                        GameUtil.TemperatureInterpretation.Absolute, true, true),
                    GameUtil.GetFormattedTemperature(target.HighLimit, GameUtil.TimeSlice.None,
                        GameUtil.TemperatureInterpretation.Absolute, true, true));
            else
                tooltip = STRINGS.TEMPERATURELIMIT.TOOLTIP_NOTSET;
            PUIElements.SetToolTip(lowInput, tooltip);
            PUIElements.SetToolTip(highInput, tooltip);
        }

        public bool IsAnyFieldFocused()
        {
            return (lowField != null && lowField.isFocused) || (highField != null && highField.isFocused);
        }

        protected override void OnDisable()
        {
            MaterialSelectionPanel_Patch.CheckResetToConstructionDefaults(target);
        }
    }
}
