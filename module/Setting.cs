using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;

namespace StaleBodySweeper
{
    [FileLocation(nameof(StaleBodySweeper))]
    public class Setting : ModSetting
    {
        public const string kSection = "Main";

        public Setting(IMod mod) : base(mod)
        {

        }


        [SettingsUISlider(min = 0, max = 1000, step = 1, scalarMultiplier = 1, unit = Unit.kDataMegabytes)]
        [SettingsUISection(kSection)]
        public int abandonedBodyCount { get; set; } = 100;

        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(kSection)]
        public bool ButtonWithConfirmation { set { SetDefaults(); } }


        public DropdownItem<int>[] GetIntDropdownItems()
        {
            var items = new List<DropdownItem<int>>();

            for (var i = 0; i < 3; i += 1)
            {
                items.Add(new DropdownItem<int>()
                {
                    value = i,
                    displayName = i.ToString(),
                });
            }

            return items.ToArray();
        }

        public override void SetDefaults()
        {
            abandonedBodyCount = 100;
        }
    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "StaleBodySweeper" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.abandonedBodyCount)), "Int slider" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.abandonedBodyCount)), $"Use int property with getter and setter and [{nameof(SettingsUISliderAttribute)}] to get int slider" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ButtonWithConfirmation)), "Button with confirmation" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ButtonWithConfirmation)), $"Button can show confirmation message. Use [{nameof(SettingsUIConfirmationAttribute)}]" },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.ButtonWithConfirmation)), "is it confirmation text which you want to show here?" },

            };
        }

        public void Unload()
        {

        }
    }
}
