using StardewValley;
using StardewValley.Menus;
using StardewValley.Mobile;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ShopAnywhere
{
    internal sealed class Shop : Mod
    {
        private static IMonitor M;
        public static Shop SA;
        private Response[] categories, general, combatAndMining, building, animals, oth;
        private StardewValley.GameLocation.afterQuestionBehavior categoriesOptionsLogic, generalLogic, combatAndMiningLogic, buildingLogic, animalsLogic, othLogic;
        private bool wasBTapped = false;
        private string lastLocationName;
        private Vector2? lastTilePos;
        private bool canSkip = false;
        private const int Delay = 50;
        private const string KTShop = "(O)kt.shop";
        private Config config;

        public override void Entry(IModHelper helper)
        {
            SA = this;
            M = this.Monitor;

            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                var ex = new Exception();
                Monitor.Log($"This mod only supports Android. {ex.ToString()}", LogLevel.Error);
                return;
            }

            var harmony = new Harmony(ModManifest.UniqueID);

            try
            {
                harmony.Patch(
                    original: AccessTools.PropertyGetter(typeof(VirtualJoypad), nameof(VirtualJoypad.ButtonBPressed)),
                    postfix: new HarmonyMethod(typeof(Shop), nameof(Shop.OpenMain_ButtonB))
                );
                Monitor.Log("Succesfully patched VirtualJoypad.ButtonBPressed", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to patch VirtualJoypad.ButtonBPressed: {ex.ToString()}", LogLevel.Error);
            }

            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(TapToMove), nameof(TapToMove.OnTap)),
                    postfix: new HarmonyMethod(typeof(Shop), nameof(Shop.OpenMain_TapToMove))
                );
                Monitor.Log("Succesfully patched TapToMove.OnTap", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to patch TapToMove.OnTap: {ex.ToString()}", LogLevel.Error);
            }

            var skipCallback = new HarmonyMethod(typeof(Shop), nameof(Shop.SkipCallback));
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenu)),
                    prefix: skipCallback
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild)),
                    prefix: skipCallback
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal)),
                    prefix: skipCallback
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnToShopMenu)),
                    prefix: skipCallback
                );
                Monitor.Log("Callback methods succesfully patched", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to patch Callback methods: {ex.ToString()}", LogLevel.Error);
            }

            this.config = helper.ReadConfig<Config>();

            if (config.EnableKeybind)
            {
                helper.Events.Input.ButtonPressed += OpenMain_Key;
                Monitor.Log($"Keybind set to {config.Keybind}", LogLevel.Trace);
            }

            helper.Events.GameLoop.GameLaunched += QuestionDialogueCache;
            helper.Events.Display.MenuChanged += FlagReset;
        }
        private void QuestionDialogue(
            string question,
            Response[] answerChoices,
            StardewValley.GameLocation.afterQuestionBehavior afterDialogueBehavior
        )
        {
            Game1.currentLocation.createQuestionDialogue(
                question: question,
                answerChoices: answerChoices,
                afterDialogueBehavior: afterDialogueBehavior
            );
        }
        private void Categories()
        {
            wasBTapped = false;
            QuestionDialogue(
                Helper.Translation.Get("option.categories"),
                categories,
                categoriesOptionsLogic
            );
        }
        private void General()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.general"),
                general,
                generalLogic
            );
        }
        private void CombatAndMining()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.combat"),
                combatAndMining,
                combatAndMiningLogic
            );
        }
        private void Building()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.building"),
                building,
                buildingLogic
            );
        }
        private void Animals()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.animals"),
                animals,
                animalsLogic
            );
        }
        private void Others()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.others"),
                oth,
                othLogic
            );
        }
        private void QuestionDialogueCache(object sender, GameLaunchedEventArgs e)
        {
            categories = new Response[]
            {
                new Response("General", Helper.Translation.Get("option.general")),
                new Response("CombatAndMining", Helper.Translation.Get("option.combat")),
                new Response("Building", Helper.Translation.Get("option.building")),
                new Response("Animals", Helper.Translation.Get("option.animals")),
                new Response("Others", Helper.Translation.Get("option.others")),
                new Response("doNothing", Helper.Translation.Get("option.close"))
            };
            categoriesOptionsLogic = (Farmer who, string whichAnswer) =>
            {
                switch (whichAnswer)
                {
                    case "General": DelayedAction.functionAfterDelay(General, Delay); break;
                    case "CombatAndMining": DelayedAction.functionAfterDelay(CombatAndMining, Delay); break;
                    case "Building": DelayedAction.functionAfterDelay(Building, Delay); break;
                    case "Animals": DelayedAction.functionAfterDelay(Animals, Delay); break;
                    case "Others": DelayedAction.functionAfterDelay(Others, Delay); break;
                }
            };
            general = new Response[]
            {
                new Response("seedShop", Helper.Translation.Get("shop.pierre")),
                new Response("fishShop", Helper.Translation.Get("shop.willy")),
                new Response("saloon", Helper.Translation.Get("shop.saloon")),
                new Response("sandyShop", Helper.Translation.Get("shop.oasis")),
                new Response("return", Helper.Translation.Get("option.return"))
            };
            generalLogic = (Farmer who, string generalanswers) =>
            {
                switch (generalanswers)
                {
                    case "seedShop": Utility.TryOpenShopMenu(Game1.shop_generalStore, null, false); break;
                    case "fishShop": Utility.TryOpenShopMenu(Game1.shop_fish, null, false); break;
                    case "saloon": Utility.TryOpenShopMenu(Game1.shop_saloon, null, false); break;
                    case "sandyShop": SandyShop(); break;
                    case "return": DelayedAction.functionAfterDelay(Categories, Delay); break;
                }
            };
            combatAndMining = new Response[]
            {
                new Response("adventureShop", Helper.Translation.Get("shop.adventurer")),
                new Response("blacksmith", Helper.Translation.Get("shop.clint")),
                new Response("toolUpgrades", Helper.Translation.Get("shop.upgrades")),
                new Response("crushGeodes", Helper.Translation.Get("shop.geodes")),
                new Response("desertTrader", Helper.Translation.Get("shop.desertTrader")),
                new Response("return", Helper.Translation.Get("option.return"))
            };
            combatAndMiningLogic = (Farmer who, string combatAndMininganswers) =>
            {
                switch (combatAndMininganswers)
                {
                    case "adventureShop": Utility.TryOpenShopMenu(Game1.shop_adventurersGuild, null, false); break;
                    case "blacksmith": Utility.TryOpenShopMenu(Game1.shop_blacksmith, null, false); break;
                    case "toolUpgrades": Utility.TryOpenShopMenu(Game1.shop_blacksmithUpgrades, null, false); break;
                    case "crushGeodes": Game1.activeClickableMenu = new StardewValley.Menus.GeodeMenu(); break;
                    case "desertTrader": DesertTrader(); break;
                    case "return": DelayedAction.functionAfterDelay(Categories, Delay); break;
                }
            };
            building = new Response[]
            {
                new Response("carpenter", Helper.Translation.Get("shop.robin")),
                new Response("buildBuildings", Helper.Translation.Get("shop.construct")),
                new Response("wizard", Helper.Translation.Get("shop.wizard")),
                new Response("return", Helper.Translation.Get("option.return"))
            };
            buildingLogic = (Farmer who, string buildinganswers) =>
            {
                switch (buildinganswers)
                {
                    case "carpenter": Utility.TryOpenShopMenu(Game1.shop_carpenter, null, false); break;
                    case "buildBuildings": BuildingMenu("Robin"); break;
                    case "wizard": WizardMenu("Wizard"); break;
                    case "return": DelayedAction.functionAfterDelay(Categories, Delay); break;
                }
            };
            animals = new Response[]
            {
                new Response("supplies", Helper.Translation.Get("shop.marnie")),
                new Response("animalShop", Helper.Translation.Get("shop.buyAnimals")),
                new Response("adoptPet", Helper.Translation.Get("shop.pets")),
                new Response("return", Helper.Translation.Get("option.return"))
            };
            animalsLogic = (Farmer who, string animalsAnswers) =>
            {
                switch (animalsAnswers)
                {
                    case "supplies": Utility.TryOpenShopMenu(Game1.shop_animalSupplies, null, false); break;
                    case "animalShop": MarnieMenu(); break;
                    case "adoptPet": Utility.TryOpenShopMenu(Game1.shop_petAdoption, null, false); break;
                    case "return": DelayedAction.functionAfterDelay(Categories, Delay); break;
                }
            };
            oth = new Response[]
            {
                new Response("wanderingTrader", Helper.Translation.Get("shop.travelingCart")),
                new Response("dwarf", Helper.Translation.Get("shop.dwarf")),
                new Response("krobus", Helper.Translation.Get("shop.krobus")),
                new Response("return", Helper.Translation.Get("option.return"))
            };
            othLogic = (Farmer who, string othAnswers) =>
            {
                switch (othAnswers)
                {
                    case "wanderingTrader": TravelingCart(); break;
                    case "dwarf": DwarfShop(); break;
                    case "krobus": KrobusShop(); break;
                    case "return": DelayedAction.functionAfterDelay(Categories, Delay); break;
                }
            };
        }
        private void BuildingMenu(string npc)
        {
            SavePosition();
            Game1.currentLocation.ShowConstructOptions(npc);
        }
        private void MarnieMenu()
        {
            SavePosition();
            Game1.currentLocation.ShowAnimalShopMenu();
        }
        private void WizardMenu(string npc)
        {
            if (Game1.player.hasMagicInk)
            {
                SavePosition();
                Game1.currentLocation.ShowConstructOptions(npc);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.wizard")); }
        }
        private void KrobusShop()
        {
            if (Game1.player.hasRustyKey)
            {
                Utility.TryOpenShopMenu(Game1.shop_krobus, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.krobus")); }
        }
        private void DesertTrader()
        {
            if (Game1.player.hasOrWillReceiveMail("ccVault") || Game1.player.hasOrWillReceiveMail("JojaVault"))
            {
                Utility.TryOpenShopMenu(Game1.shop_desertTrader, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.bus")); }
        }
        private void DwarfShop()
        {
            if (Game1.player.canUnderstandDwarves)
            {
                Utility.TryOpenShopMenu(Game1.shop_dwarf, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.dwarf")); }
        }
        private void SandyShop()
        {
            if (Game1.player.hasOrWillReceiveMail("ccVault") || Game1.player.hasOrWillReceiveMail("JojaVault"))
            {
                Utility.TryOpenShopMenu(Game1.shop_sandy, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.bus")); }
        }
        private void TravelingCart()
        {
            if (Game1.dayOfMonth % 7 == 5 || Game1.dayOfMonth % 7 == 0)
            {
                Utility.TryOpenShopMenu(Game1.shop_travelingCart, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.travelingCart")); }
        }
        private void OpenMain_Key(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == config.Keybind && Context.IsWorldReady && Context.IsPlayerFree)
            {
                Categories();
            }
        }
        public static void OpenMain_ButtonB(ref bool __result)
        {
            try
            {
                if (Context.IsPlayerFree && !SA.wasBTapped && __result)
                {
                    if (Game1.player.CurrentItem?.QualifiedItemId == KTShop)
                    {
                        SA.Categories();
                    }
                }
                SA.wasBTapped = __result;
            }
            catch (Exception ex)
            {
                M.Log($"An error has occured while trying to open 'Categories': {ex.ToString()}", LogLevel.Error);
            }
        }
        public static void OpenMain_TapToMove(int mouseX, int mouseY, int viewportX, int viewportY)
        {
            try
            {
                int tappedTileX = (mouseX + viewportX) / Game1.tileSize;
                int tappedTileY = (mouseY + viewportY) / Game1.tileSize;

                int playerTileX = Game1.player.TilePoint.X;
                int playerTileY = Game1.player.TilePoint.Y;

                if (tappedTileX == playerTileX && tappedTileY == playerTileY)
                {
                    if (Game1.options.weaponControl == 1 || Game1.options.weaponControl == 0)
                    {
                        if (Game1.player.CurrentItem?.QualifiedItemId == KTShop && Context.IsPlayerFree)
                        {
                            SA.Categories();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                M.Log($"An error has occurred while trying to open 'Categories': {ex.ToString()}", LogLevel.Error);
            }
        }
        private void SavePosition()
        {
            canSkip = true;
            lastLocationName = Game1.currentLocation.NameOrUniqueName;
            lastTilePos = Game1.player.Tile;
            Monitor.Log($"Position saved: {lastLocationName} {lastTilePos}", LogLevel.Trace);
        }
        public static bool SkipCallback(object __instance)
        {
            if (!SA.canSkip) { return true; }

            try
            {
                SA.canSkip = false;
                foreach (GameLocation location in Game1.locations)
                {
                    foreach (var building in location.buildings)
                    {
                        building.color = Color.White;
                    }
                }
                LocationRequest req = Game1.getLocationRequest(SA.lastLocationName);
                req.OnWarp += delegate
                {
                    Game1.exitActiveMenu();
                    Game1.viewportFreeze = false;
                    Game1.displayHUD = true;
                    Game1.displayFarmer = true;
                    Game1.player.viewingLocation.Value = null;

                    if (SA.lastTilePos != null && SA.lastLocationName != null)
                    {
                        SA.lastLocationName = null;
                        SA.lastTilePos = null;
                        M.Log("Saved position cleared", LogLevel.Trace);
                    }

                    if (__instance is CarpenterMenu carpenter)
                    {
                        var reflection = SA.Helper.Reflection;
                        reflection.GetField<bool>(carpenter, "upgrading").SetValue(false);
                        reflection.GetField<bool>(carpenter, "demolishing").SetValue(false);
                        reflection.GetField<bool>(carpenter, "moving").SetValue(false);
                        reflection.GetField<bool>(carpenter, "painting").SetValue(false);
                        reflection.GetMethod(carpenter, "resetBounds").Invoke();
                    }

                    if (__instance is PurchaseAnimalsMenu animal)
                    {
                        var reflection = SA.Helper.Reflection;
                        reflection.GetField<bool>(animal, "freeze").SetValue(false);
                    }
                };
                Game1.warpFarmer(
                    req,
                    (int)SA.lastTilePos.Value.X,
                    (int)SA.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                M.Log("Callback method skipped", LogLevel.Trace);
                return false;
            }
            catch (Exception ex)
            {
                M.Log($"Failed to skip Callback method: {ex.ToString()}", LogLevel.Error);
                return true;
            }
        }
        private void FlagReset(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null && canSkip)
            {
                canSkip = false;
            }
        }
    }
    internal class Config
    {
        public SButton Keybind { get; set; } = SButton.Q;
        public bool EnableKeybind { get; set; } = false;
    }
}
