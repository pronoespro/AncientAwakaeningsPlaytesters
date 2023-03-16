using Modding;
using Satchel.BetterMenus;

namespace Ancient_Awakenings_SoulNail_charm.Auxiliar.UI
{
    public static class AAMenuSettings
    {

        public static Menu MenuRef;

        public static Menu PrepareSettingsMenu(ModToggleDelegates toggleDelegates)
        {
            return new Menu("Ancient Awakenings Settings", new Element[]
            {

                new HorizontalOption(
                        AALanguageHelper.GeneralLanguage["Zote's 57 Precepts_NAME"],AALanguageHelper.GeneralLanguage["Zote's 57 Precepts_DESC"],
                        new string[]{ AALanguageHelper.GeneralLanguage["Zote's 57 Precepts_Disabled"] , AALanguageHelper.GeneralLanguage["Zote's 57 Precepts_Enabled"] },
                        (setting) => { AncientAwakeningsMod.Instance.zotePowerEnabled=!AncientAwakeningsMod.Instance.zotePowerEnabled; ; },
                        ()=>AncientAwakeningsMod.Instance.zotePowerEnabled?1:0,Id:"Zote's Pacifism"
                    )

            });
        }

        public static MenuScreen GetMenu(MenuScreen lastMenu, ModToggleDelegates? toggleDelegates)
        {
            if (MenuRef == null)
            {
                MenuRef = PrepareSettingsMenu((ModToggleDelegates)toggleDelegates);
            }
            return MenuRef.GetMenuScreen(lastMenu);
        }

    }
}
