using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ShopAnywhere
{
    internal class EventHandler
    {
        private readonly Shop Shop;
        private readonly IModHelper helper;
        private Config config;

        public Response[] categories, general, combatAndMining, building, animals, oth;
        public StardewValley.GameLocation.afterQuestionBehavior categoriesOptionsLogic, generalLogic, combatAndMiningLogic, buildingLogic, animalsLogic, othLogic;
        public const int Delay = 50;
        public const string KTShop = "(O)kt.shop";
        private bool wasBTapped = false;

        public EventHandler(Shop shop, IModHelper helper, IMonitor Monitor, Config config)
        {
            this.Shop = shop;
            this.helper = helper;
            this.config = config;

            helper.Events.GameLoop.SaveLoaded += this.InitializeQuestionDialogue;
            helper.Events.GameLoop.GameLaunched += this.InitializeConfig;
            helper.Events.GameLoop.SaveLoaded += this.CabinDemolishFix;
            helper.Events.Display.MenuChanged += this.FlagReset;
            helper.Events.Input.ButtonReleased += this.OpenMain_Key;
            helper.Events.Input.ButtonReleased += this.OnTap;
            helper.Events.GameLoop.UpdateTicking += this.ButtonBPressed;

            Monitor.Log($"Keybind set to {config.Keybind}", LogLevel.Trace);
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
                    case "General": DelayedAction.functionAfterDelay(Shop.General, Delay); break;
                    case "CombatAndMining": DelayedAction.functionAfterDelay(Shop.CombatAndMining, Delay); break;
                    case "Building": DelayedAction.functionAfterDelay(Shop.Building, Delay); break;
                    case "Animals": DelayedAction.functionAfterDelay(Shop.Animals, Delay); break;
                    case "Others": DelayedAction.functionAfterDelay(Shop.Others, Delay); break;
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
                    case "sandyShop": Shop.SandyShop(); break;
                    case "return": DelayedAction.functionAfterDelay(Shop.Categories, Delay); break;
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
                    case "desertTrader": Shop.DesertTrader(); break;
                    case "return": DelayedAction.functionAfterDelay(Shop.Categories, Delay); break;
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
                    case "buildBuildings": Shop.BuildingMenu("Robin"); break;
                    case "wizard": Shop.WizardMenu("Wizard"); break;
                    case "return": DelayedAction.functionAfterDelay(Shop.Categories, Delay); break;
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
                    case "animalShop": Shop.MarnieMenu(); break;
                    case "adoptPet": Utility.TryOpenShopMenu(Game1.shop_petAdoption, null, false); break;
                    case "return": DelayedAction.functionAfterDelay(Shop.Categories, Delay); break;
                }
            };
            oth = new Response[]
            {
                new Response("wanderingTrader", helper.Translation.Get("shop.travelingCart")),
                new Response("dwarf", helper.Translation.Get("shop.dwarf")),
                new Response("krobus", helper.Translation.Get("shop.krobus")),
                new Response("qiGem", helper.Translation.Get("shop.qiGem")),
                new Response("return", helper.Translation.Get("option.return"))
            };
            othLogic = (Farmer who, string othAnswers) =>
            {
                switch (othAnswers)
                {
                    case "wanderingTrader": Shop.TravelingCart(); break;
                    case "dwarf": Shop.DwarfShop(); break;
                    case "krobus": Shop.KrobusShop(); break;
                    case "qiGem": Shop.QiGemShop(); break;
                    case "return": DelayedAction.functionAfterDelay(Shop.Categories, Delay); break;
                }
            };
        }
        public void InitializeConfig(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) { return; }

            configMenu.Register(
                mod: Shop.ModManifest,
                reset: () => config = new Config(),
                save: () => helper.WriteConfig(config)
            );

            configMenu.AddBoolOption(
                mod: Shop.ModManifest,
                name: () => "Enable Keybind",
                getValue: () => config.EnableKeybind,
                setValue: value => config.EnableKeybind = value
            );

            configMenu.AddBoolOption(
                mod: Shop.ModManifest,
                name: () => "Allow Multiple Build",
                getValue: () => config.AllowMultipleBuild,
                setValue: value => config.AllowMultipleBuild = value
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
            if (e.NewMenu == null && Shop.canSkip)
            {
                Shop.canSkip = false;
            }
        }
        private void OpenMain_Key(object sender, ButtonReleasedEventArgs e)
        {
            if (!config.EnableKeybind) { return; }

            if (!Context.IsWorldReady || !Context.IsPlayerFree) { return; }

            if (e.Button == config.Keybind)
            {
                Shop.Categories();
            }
        }
        public void OnTap(object s, ButtonReleasedEventArgs e)
        {
            if (!Context.IsPlayerFree || !Context.IsWorldReady) { return; }

            if (Game1.player.CurrentItem?.QualifiedItemId == KTShop)
            {
                if (e.Button == SButton.MouseLeft && e.Cursor.Tile == Game1.player.getTileLocation())
                {
                    if (Game1.options.weaponControl == 0 || Game1.options.weaponControl == 1)
                    {
                        Shop.Categories();
                    }
                }
            }
        }
        private void ButtonBPressed(object s, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) { return; }

            var keyState = Game1.currentLocation.tapToMove.mobileKeyStates;

            if (Game1.player.CurrentItem?.QualifiedItemId == KTShop)
            {
                if (!wasBTapped && keyState.actionButtonPressed)
                {
                    Shop.Categories();
                    wasBTapped = true;
                }
                else if (!keyState.actionButtonPressed)
                {
                    wasBTapped = false;
                }
            }
        }
    }
}
