using StardewModdingAPI;
using HarmonyLib;
using ShopAnywhereAndroid;

namespace ShopAnywhereAndroid
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                Monitor.Log("This mod only supports the mobile version of the game.", LogLevel.Error);
                return;
            }

            Config.Instance = helper.ReadConfig<Config>();

            Harmony harmony = new Harmony(this.ModManifest.UniqueID);

            new H_Patches(harmony, helper, Monitor);

            new S_Events(helper, Monitor);

            new Shops(helper, Monitor);
        }
    }
}
