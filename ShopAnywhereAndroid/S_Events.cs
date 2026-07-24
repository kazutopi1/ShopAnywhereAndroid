using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ShopAnywhereAndroid
{
    public class S_Events
    {
        public static S_Events Instance { get; private set; }

        readonly IMonitor Monitor;

        readonly IModHelper helper;

        public Response[] categories, general, combatAndMining, building, animals, oth;

        public StardewValley.GameLocation.afterQuestionBehavior categoriesOptionsLogic, generalLogic, combatAndMiningLogic, buildingLogic, animalsLogic, othLogic;

        public const int Delay = 50;

        public S_Events(IModHelper helper, IMonitor monitor)
        {
            Instance = this;

            this.helper = helper;

            this.Monitor = monitor;

            helper.Events.GameLoop.SaveLoaded += this.InitializeQuestionDialogue;
            helper.Events.GameLoop.GameLaunched += this.InitializeConfig;
            helper.Events.GameLoop.SaveLoaded += this.CabinDemolishFix;
            helper.Events.Display.MenuChanged += this.FlagReset;
            helper.Events.Input.ButtonReleased += this.OpenMain_Key;
            helper.Events.Input.ButtonReleased += this.OnTap;

            Monitor.Log($"Keybind set to {Config.Instance.Keybind}", LogLevel.Trace);
        }

        public void InitializeQuestionDialogue(object sender, SaveLoadedEventArgs e)
        {
            categories = new Response[]
            {
                new Response("General", helper.Translation.Get("option.general")),
                new Response("CombatAndMining", helper.Translation.Get("option.combat")),
                new Response("Building", helper.Translation.Get("option.building")),
                new Response("Animals", helper.Translation.Get("option.animals")),
                new Response("Others", helper.Translation.Get("option.others")),
                new Response("doNothing", helper.Translation.Get("option.close"))
            };
            categoriesOptionsLogic = (Farmer who, string whichAnswer) =>
            {
                switch (whichAnswer)
                {
                    case "General": DelayedAction.functionAfterDelay(Shops.Instance.General, Delay); break;
                    case "CombatAndMining": DelayedAction.functionAfterDelay(Shops.Instance.CombatAndMining, Delay); break;
                    case "Building": DelayedAction.functionAfterDelay(Shops.Instance.Building, Delay); break;
                    case "Animals": DelayedAction.functionAfterDelay(Shops.Instance.Animals, Delay); break;
                    case "Others": DelayedAction.functionAfterDelay(Shops.Instance.Others, Delay); break;
                }
            };
            general = new Response[]
            {
                new Response("seedShop", helper.Translation.Get("shop.pierre")),
                new Response("fishShop", helper.Translation.Get("shop.willy")),
                new Response("saloon", helper.Translation.Get("shop.saloon")),
                new Response("sandyShop", helper.Translation.Get("shop.oasis")),
                new Response("return", helper.Translation.Get("option.return"))
            };
            generalLogic = (Farmer who, string generalanswers) =>
            {
                switch (generalanswers)
                {
                    case "seedShop": Utility.TryOpenShopMenu(Game1.shop_generalStore, null, false); break;
                    case "fishShop": Utility.TryOpenShopMenu(Game1.shop_fish, null, false); break;
                    case "saloon": Utility.TryOpenShopMenu(Game1.shop_saloon, null, false); break;
                    case "sandyShop": Shops.Instance.SandyShop(); break;
                    case "return": DelayedAction.functionAfterDelay(Shops.Instance.Categories, Delay); break;
                }
            };
            combatAndMining = new Response[]
            {
                new Response("adventureShop", helper.Translation.Get("shop.adventurer")),
                new Response("blacksmith", helper.Translation.Get("shop.clint")),
                new Response("toolUpgrades", helper.Translation.Get("shop.upgrades")),
                new Response("crushGeodes", helper.Translation.Get("shop.geodes")),
                new Response("desertTrader", helper.Translation.Get("shop.desertTrader")),
                new Response("return", helper.Translation.Get("option.return"))
            };
            combatAndMiningLogic = (Farmer who, string combatAndMininganswers) =>
            {
                switch (combatAndMininganswers)
                {
                    case "adventureShop": Utility.TryOpenShopMenu(Game1.shop_adventurersGuild, null, false); break;
                    case "blacksmith": Utility.TryOpenShopMenu(Game1.shop_blacksmith, null, false); break;
                    case "toolUpgrades": Utility.TryOpenShopMenu(Game1.shop_blacksmithUpgrades, null, false); break;
                    case "crushGeodes": Game1.activeClickableMenu = new StardewValley.Menus.GeodeMenu(); break;
                    case "desertTrader": Shops.Instance.DesertTrader(); break;
                    case "return": DelayedAction.functionAfterDelay(Shops.Instance.Categories, Delay); break;
                }
            };
            building = new Response[]
            {
                new Response("carpenter", helper.Translation.Get("shop.robin")),
                new Response("buildBuildings", helper.Translation.Get("shop.construct")),
                new Response("wizard", helper.Translation.Get("shop.wizard")),
                new Response("return", helper.Translation.Get("option.return"))
            };
            buildingLogic = (Farmer who, string buildinganswers) =>
            {
                switch (buildinganswers)
                {
                    case "carpenter": Utility.TryOpenShopMenu(Game1.shop_carpenter, null, false); break;
                    case "buildBuildings": Shops.Instance.BuildingMenu("Robin"); break;
                    case "wizard": Shops.Instance.WizardMenu("Wizard"); break;
                    case "return": DelayedAction.functionAfterDelay(Shops.Instance.Categories, Delay); break;
                }
            };
            animals = new Response[]
            {
                new Response("supplies", helper.Translation.Get("shop.marnie")),
                new Response("animalShop", helper.Translation.Get("shop.buyAnimals")),
                new Response("adoptPet", helper.Translation.Get("shop.pets")),
                new Response("return", helper.Translation.Get("option.return"))
            };
            animalsLogic = (Farmer who, string animalsAnswers) =>
            {
                switch (animalsAnswers)
                {
                    case "supplies": Utility.TryOpenShopMenu(Game1.shop_animalSupplies, null, false); break;
                    case "animalShop": Shops.Instance.MarnieMenu(); break;
                    case "adoptPet": Utility.TryOpenShopMenu(Game1.shop_petAdoption, null, false); break;
                    case "return": DelayedAction.functionAfterDelay(Shops.Instance.Categories, Delay); break;
                }
            };
            oth = new Response[]
            {
                new Response("wanderingTrader", helper.Translation.Get("shop.travelingCart")),
                new Response("dwarf", helper.Translation.Get("shop.dwarf")),
                new Response("krobus", helper.Translation.Get("shop.krobus")),
                new Response("qiGem", helper.Translation.Get("shop.qiGem")),
                new Response("hatMouse", helper.Translation.Get("shop.hatMouse")),
                new Response("return", helper.Translation.Get("option.return"))
            };
            othLogic = (Farmer who, string othAnswers) =>
            {
                switch (othAnswers)
                {
                    case "wanderingTrader": Shops.Instance.TravelingCart(); break;
                    case "dwarf": Shops.Instance.DwarfShop(); break;
                    case "krobus": Shops.Instance.KrobusShop(); break;
                    case "qiGem": Shops.Instance.QiGemShop(); break;
                    case "hatMouse": Shops.Instance.HatMouseShop(); break;
                    case "return": DelayedAction.functionAfterDelay(Shops.Instance.Categories, Delay); break;
                }
            };
        }

        public void InitializeConfig(object sender, GameLaunchedEventArgs e)
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

        private void CabinDemolishFix(object sender, SaveLoadedEventArgs e)
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
        }

        private void FlagReset(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null && Shops.Instance.canSkip)
            {
                Shops.Instance.canSkip = false;
            }
        }

        private void OpenMain_Key(object sender, ButtonReleasedEventArgs e)
        {
            if (!Config.Instance.EnableKeybind) { return; }

            if (!Context.IsWorldReady || !Context.IsPlayerFree || Game1.player.IsBusyDoingSomething()) { return; }

            if (e.Button == Config.Instance.Keybind)
            {
                Shops.Instance.Categories();
            }
        }

        public void OnTap(object s, ButtonReleasedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Context.IsWorldReady || Game1.player.IsBusyDoingSomething()) { return; }

            if (Game1.player.CurrentItem?.QualifiedItemId == Shops.KTShop)
            {
                if (e.Button == SButton.MouseLeft && e.Cursor.Tile == Game1.player.getTileLocation())
                {
                    if (Game1.options.weaponControl == 0 || Game1.options.weaponControl == 1)
                    {
                        Shops.Instance.Categories();
                    }
                }
            }
        }
    }
}
