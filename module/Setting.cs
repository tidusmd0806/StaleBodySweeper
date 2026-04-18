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

        [SettingsUISlider(min = 0, max = 500, step = 1)]
        [SettingsUISection(kSection)]
        public int abandonedBodyCount { get; set; } = 25;

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
            abandonedBodyCount = 25;
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
                { m_Setting.GetSettingsLocaleID(), "Stale Body Sweeper" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.abandonedBodyCount)), "Abandoned Body Count" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.abandonedBodyCount)), "Maximum number of abandoned bodies to sweep." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ButtonWithConfirmation)), "Reset to Defaults" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ButtonWithConfirmation)), "Reset all settings to their default values" },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.ButtonWithConfirmation)), "Are you sure you want to reset all settings to defaults?" },
            };
        }

        public void Unload()
        {

        }
    }
}
