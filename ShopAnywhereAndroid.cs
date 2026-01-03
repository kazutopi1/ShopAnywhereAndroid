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
        private Response[] categories, cat1, cat2, cat3, cat4, oth;
        private StardewValley.GameLocation.afterQuestionBehavior categoriesOptionsLogic, cat1Logic, cat2Logic, cat3Logic, cat4Logic, othLogic;
        private bool wasBTapped = false;
        private string lastLocationName;
        private Vector2? lastTilePos;
        private bool canSkip = false;
        private const int Delay = 50;
        private const string KTShop = "(O)kt.shop";

        public override void Entry(IModHelper helper)
        {
            SA = this;
            M = this.Monitor;

            if (Constants.TargetPlatform != GamePlatform.Android)
                return;

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
            helper.Events.Input.ButtonPressed += OpenMain_Key;
            helper.Events.GameLoop.GameLaunched += QuestionDialogueCache;
            helper.Events.Display.MenuChanged += FlagReset;
        }
        private void MainCategory() { wasBTapped = false; QuestionDialogue("Categories", categories, categoriesOptionsLogic); }
        private void category1() { QuestionDialogue("General Goods", cat1, cat1Logic); }
        private void category2() { QuestionDialogue("Combat and Mining", cat2, cat2Logic); }
        private void category3() { QuestionDialogue("Building", cat3, cat3Logic); }
        private void category4() { QuestionDialogue("Animals", cat4, cat4Logic); }
        private void others() { QuestionDialogue("Other Shops", oth, othLogic); }

        private void QuestionDialogueCache(object sender, GameLaunchedEventArgs e)
        {
            categories = new Response[]
            {
                new Response("category1", "General Goods"),
                new Response("category2", "Combat and Mining"),
                new Response("category3", "Building"),
                new Response("category4", "Animal Supplies"),
                new Response("others", "Others"),
                new Response("doNothing", "Close")
            };
            categoriesOptionsLogic = (Farmer who, string whichAnswer) =>
            {
                switch (whichAnswer)
                {
                    case "category1": DelayedAction.functionAfterDelay(category1, Delay); break;
                    case "category2": DelayedAction.functionAfterDelay(category2, Delay); break;
                    case "category3": DelayedAction.functionAfterDelay(category3, Delay); break;
                    case "category4": DelayedAction.functionAfterDelay(category4, Delay); break;
                    case "others": DelayedAction.functionAfterDelay(others, Delay); break;
                }
            };
            cat1 = new Response[]
            {
                new Response("seedShop", "Pierre's General Store"),
                new Response("fishShop", "Willy's Shop"),
                new Response("saloon", "Saloon"),
                new Response("sandyShop", "Oasis"),
                new Response("return", "Return")
            };
            cat1Logic = (Farmer who, string cat1answers) =>
            {
                switch (cat1answers)
                {
                    case "seedShop": Utility.TryOpenShopMenu(Game1.shop_generalStore, null, false); break;
                    case "fishShop": Utility.TryOpenShopMenu(Game1.shop_fish, null, false); break;
                    case "saloon": Utility.TryOpenShopMenu(Game1.shop_saloon, null, false); break;
                    case "sandyShop": SandyShop(); break;
                    case "return": DelayedAction.functionAfterDelay(MainCategory, Delay); break;
                }
            };
            cat2 = new Response[]
            {
                new Response("adventureShop", "Adventurer's Guild Shop"),
                new Response("blacksmith", "Clint's Shop"),
                new Response("toolUpgrades", "Tool Upgrades"),
                new Response("crushGeodes", "Crush Geodes"),
                new Response("desertTrader", "Desert Trader"),
                new Response("return2", "Return")
            };
            cat2Logic = (Farmer who, string cat2answers) =>
            {
                switch (cat2answers)
                {
                    case "adventureShop": Utility.TryOpenShopMenu(Game1.shop_adventurersGuild, null, false); break;
                    case "blacksmith": Utility.TryOpenShopMenu(Game1.shop_blacksmith, null, false); break;
                    case "toolUpgrades": Utility.TryOpenShopMenu(Game1.shop_blacksmithUpgrades, null, false); break;
                    case "crushGeodes": Game1.activeClickableMenu = new StardewValley.Menus.GeodeMenu(); break;
                    case "desertTrader": DesertTrader(); break;
                    case "return2": DelayedAction.functionAfterDelay(MainCategory, Delay); break;
                }
            };
            cat3 = new Response[]
            {
                new Response("carpenter", "Robin's Shop"),
                new Response("buildBuildings", "Construct Farm Buildings"),
                new Response("wizard", "Construct Wizard Buildings"),
                new Response("return3", "Return")
            };
            cat3Logic = (Farmer who, string cat3answers) =>
            {
                switch (cat3answers)
                {
                    case "carpenter": Utility.TryOpenShopMenu(Game1.shop_carpenter, null, false); break;
                    case "buildBuildings": BuildingMenu("Robin"); break;
                    case "wizard": WizardMenu("Wizard"); break;
                    case "return3": DelayedAction.functionAfterDelay(MainCategory, Delay); break;
                }
            };
            cat4 = new Response[]
            {
                new Response("supplies", "Marnie's Shop"),
                new Response("animalShop", "Purchase Animals"),
                new Response("adoptPet", "Adopt Pets"),
                new Response("return4", "Return")
            };
            cat4Logic = (Farmer who, string cat4Answers) =>
            {
                switch (cat4Answers)
                {
                    case "supplies": Utility.TryOpenShopMenu(Game1.shop_animalSupplies, null, false); break;
                    case "animalShop": MarnieMenu(); break;
                    case "adoptPet": Utility.TryOpenShopMenu(Game1.shop_petAdoption, null, false); break;
                    case "return4": DelayedAction.functionAfterDelay(MainCategory, Delay); break;
                }
            };
            oth = new Response[]
            {
                new Response("wanderingTrader", "Traveling Cart"),
                new Response("dwarf", "Dwarf's Shop"),
                new Response("krobus", "Krobus's Shop"),
                new Response("othReturn", "Return")
            };
            othLogic = (Farmer who, string othAnswers) =>
            {
                switch (othAnswers)
                {
                    case "wanderingTrader": TravelingCart(); break;
                    case "dwarf": DwarfShop(); break;
                    case "krobus": KrobusShop(); break;
                    case "othReturn": DelayedAction.functionAfterDelay(MainCategory, Delay); break;
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
            else { Game1.drawObjectDialogue("Return the Magic Ink to the Wizard to access this shop."); }
        }
        private void KrobusShop()
        {
            if (Game1.player.hasRustyKey)
            {
                Utility.TryOpenShopMenu(Game1.shop_krobus, null, false);
            }
            else { Game1.drawObjectDialogue("Acquire the Rusty Key first to access this Shop."); }
        }
        private void DesertTrader()
        {
            if (Game1.player.hasOrWillReceiveMail("ccVault") || Game1.player.hasOrWillReceiveMail("JojaVault"))
            {
                Utility.TryOpenShopMenu(Game1.shop_desertTrader, null, false);
            }
            else { Game1.drawObjectDialogue("Fix the Bus first to access this Shop."); }
        }
        private void DwarfShop()
        {
            if (Game1.player.canUnderstandDwarves)
            {
                Utility.TryOpenShopMenu(Game1.shop_dwarf, null, false);
            }
            else { Game1.drawObjectDialogue("Donate all 4 Dwarf Scrolls to access this shop."); }
        }
        private void SandyShop()
        {
            if (Game1.player.hasOrWillReceiveMail("ccVault") || Game1.player.hasOrWillReceiveMail("JojaVault"))
            {
                Utility.TryOpenShopMenu(Game1.shop_sandy, null, false);
            }
            else { Game1.drawObjectDialogue("Fix the Bus first to access this Shop."); }
        }
        private void TravelingCart()
        {
            if (Game1.dayOfMonth % 7 == 5 || Game1.dayOfMonth % 7 == 0)
            {
                Utility.TryOpenShopMenu(Game1.shop_travelingCart, null, false);
            }
            else { Game1.drawObjectDialogue("Only available on Fridays and Sundays"); }
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
                afterDialogueBehavior: afterDialogueBehavior,
                speaker: null
            );
        }
        private void OpenMain_Key(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.F1 && Context.IsPlayerFree && Context.IsWorldReady)
            {
                if (Helper.Input.IsDown(SButton.LeftShift) && Helper.Input.IsDown(SButton.LeftControl))
                {
                    MainCategory();
                }
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
                        SA.MainCategory();
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
                            SA.MainCategory();
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
            Monitor.Log($"Position saved: {SA.lastLocationName} {SA.lastTilePos}", LogLevel.Trace);
        }
        public static bool SkipCallback(object __instance)
        {
            if (!SA.canSkip) { return true; }

            try
            {
                SA.canSkip = false;
                if (__instance is CarpenterMenu)
                {
                    Farm farm = Game1.getLocationFromName("Farm") as Farm;
                    if (farm != null)
                    {
                        foreach (var building in farm.buildings)
                        {
                            building.color = Color.White;
                        }
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
            if (e.NewMenu == null && canSkip) { canSkip = false; }
        }
    }
}
