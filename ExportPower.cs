using ColossalFramework.UI;
using ICities;

namespace ExportPower
{
    public class ExportPower : IUserMod
    {
        public string Name => "Export Excess Power Capacity";
        public string Description => "Sell excess electricity capacity";

        public void OnSettingsUI(UIHelperBase helper) => SettingsUI.Configure(helper);
    }


    public static class SettingsUI
    {
        // Adapted from ModTools source to create a slider that displays its current value
        public static UISlider AddSliderWithDynamicLabel(
            this UIHelperBase helper,
            string text,
            string tooltip,
            float min, float max, float step, float defaultValue,
            OnValueChanged onValueChanged)
        {
            UISlider slider = null;
            slider = helper.AddSlider(
                text, min, max, step, defaultValue,
                Func) as UISlider;
            slider.tooltip = tooltip;

            void Func(float value)
            {
                onValueChanged?.Invoke(value);
                if (slider)
                {
                    slider.parent.Find<UILabel>("Label").text = $"{text}: {value}";
                }
            }

            Func(defaultValue);

            return slider;
        }

        public static void Configure(UIHelperBase helper)
        {
            var settings = PowerManager.Instance.Settings;
            var settingGroup = helper.AddGroup("Sell Excess Electrical Capacity");

            settingGroup.AddSliderWithDynamicLabel(
                "Premium Factor",
                "Price adjustment for premium capacity (capacity up to used capacity)",
                1, 2, 0.1f,
                settings.PremiumFactor,
                val => settings.PremiumFactor = val
            );


            settingGroup.AddSliderWithDynamicLabel(
                "Discount Factor",
                "Price adjustment for discounted capacity (capacity beyond 3x used capacity)",
                0.1f, 1, 0.1f,
                settings.DiscountFactor,
                val => settings.DiscountFactor = val
            );

            var debugGroup = helper.AddGroup("Debugging");
            debugGroup.AddCheckbox(
                "Enable Debug Logging",
                settings.Debug,
                isChecked => settings.Debug = isChecked
            );
        }
    }
}