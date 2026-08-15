using StardewValley;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ShopAnywhereAndroid
{
    public class ModEventHandler
    {
        public static ModEventHandler Instance { get; private set; }
        readonly IMonitor Monitor;
        readonly IModHelper helper;
        public const int Delay = 50;

        public ModEventHandler(IModHelper helper, IMonitor monitor)
        {
            Instance = this;
            this.helper = helper;
            this.Monitor = monitor;

            helper.Events.GameLoop.GameLaunched += this.InitializeConfig;
            helper.Events.GameLoop.SaveLoaded += this.CabinDemolishFix;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Input.ButtonReleased += this.OnButtonReleased;
        }
        public void InitializeConfig(object s, GameLaunchedEventArgs e)
        {
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) { return; }

            configMenu.Register(
                mod: ModEntry.Instance.ModManifest,
                reset: () => Config.Instance = new Config(),
                save: () => helper.WriteConfig(Config.Instance)
            );

            configMenu.AddBoolOption(
                mod: ModEntry.Instance.ModManifest,
                name: () => helper.Translation.Get("config.enableKeybind"),
                getValue: () => Config.Instance.EnableKeybind,
                setValue: value => Config.Instance.EnableKeybind = value
            );

            configMenu.AddBoolOption(
                mod: ModEntry.Instance.ModManifest,
                name: () => helper.Translation.Get("config.allowMultipleBuild"),
                getValue: () => Config.Instance.AllowMultipleBuild,
                setValue: value => Config.Instance.AllowMultipleBuild = value
            );
        }
        private void CabinDemolishFix(object s, SaveLoadedEventArgs e)
        {
            foreach (GameLocation location in Game1.locations)
            {
                foreach (var building in location.buildings)
                {
                    if (building.GetIndoors() is Cabin cabin)
                    {
                        if (cabin.owner == null)
                        {
                            cabin.CreateFarmhand();
                        }
                    }
                }
            }
            ShopAnywhereMenu.currentPage = 0;
        }
        private void OnMenuChanged(object s, MenuChangedEventArgs e)
        {
            if (Context.IsWorldReady)
            {
                if (Shops.Instance.canSkipDialogue)
                {
                    if (e.OldMenu is ShopMenu shopMenu)
                    {
                        if (shopMenu.ShopId == Game1.shop_blacksmithUpgrades)
                        {
                            if (e.NewMenu is DialogueBox d)
                            {
                                Shops.Instance.StopDialogue(d);
                            }
                        }
                        if (shopMenu.ShopId == Game1.shop_adventurersGuildItemRecovery)
                        {
                            if (e.NewMenu is DialogueBox d)
                            {
                                Shops.Instance.StopDialogue(d);
                            }
                        }
                    }

                    if (e.OldMenu is CarpenterMenu c || e.OldMenu is PurchaseAnimalsMenu p)
                    {
                        if (e.NewMenu is DialogueBox d)
                        {
                            Shops.Instance.StopDialogue(d);
                        }
                    }
                }
                if (Shops.Instance.canSkip && Game1.activeClickableMenu == null)
                {
                    Shops.Instance.canSkip = false;
                }
            }
        }
        private void OnButtonReleased(object s, ButtonReleasedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.player.IsBusyDoingSomething()) { return; }

            if (Config.Instance.EnableKeybind)
            {
                if (e.Button == Config.Instance.Keybind)
                {
                    Game1.activeClickableMenu = new ShopAnywhereMenu();
                    return;
                }
            }

            if (Game1.player.CurrentItem?.QualifiedItemId == Shops.KTShop)
            {
                if (e.Button == SButton.MouseLeft && e.Cursor.Tile == Game1.player.getTileLocation())
                {
                    if (Game1.options.weaponControl == 0 || Game1.options.weaponControl == 1)
                    {
                        Game1.activeClickableMenu = new ShopAnywhereMenu();
                    }
                }
            }
        }
    }
}