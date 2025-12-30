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
    public class Shop : Mod
    {
        private static IMonitor M;
        public static Shop SA;
        private Response[] categories, cat1, cat2, cat3, cat4, oth;
        private StardewValley.GameLocation.afterQuestionBehavior categoriesOptionsLogic, cat1Logic, cat2Logic, cat3Logic, cat4Logic, othLogic;
        private bool wasBTapped = false;
        private string lastLocationName;
        private Vector2 lastTilePos;
        private bool canSkip = false;
        private const int Delay = 50;
        private const string KTShop = "(O)kt.shop";
        private Harmony harmony;

        public override void Entry(IModHelper helper)
        {
            SA = this;
            M = this.Monitor;

            if (Constants.TargetPlatform != GamePlatform.Android)
                return;

            harmony = new Harmony(ModManifest.UniqueID);
            var prefix = new HarmonyMethod(typeof(Shop), nameof(Shop.Skip));

            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(VirtualJoypad), nameof(VirtualJoypad.ButtonBPressed)),
                postfix: new HarmonyMethod(typeof(Shop), nameof(Shop.OpenMain))
            );

            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenu)),
                    prefix: prefix
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild)),
                    prefix: prefix
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal)),
                    prefix: prefix
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnToShopMenu)),
                    prefix: prefix
                );
            }
            catch (Exception ex)
            {
                Monitor.Log($"Patch failed: {ex.Message}");
            }
            helper.Events.Input.ButtonPressed += Key;
            helper.Events.GameLoop.ReturnedToTitle += CleanUp;
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
                    case "category1":
                        DelayedAction.functionAfterDelay(category1, Delay);
                        break;
                    case "category2":
                        DelayedAction.functionAfterDelay(category2, Delay);
                        break;
                    case "category3":
                        DelayedAction.functionAfterDelay(category3, Delay);
                        break;
                    case "category4":
                        DelayedAction.functionAfterDelay(category4, Delay);
                        break;
                    case "others":
                        DelayedAction.functionAfterDelay(others, Delay);
                        break;
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
                    case "seedShop":
                        Utility.TryOpenShopMenu(Game1.shop_generalStore, null, false);
                        break;
                    case "fishShop":
                        Utility.TryOpenShopMenu(Game1.shop_fish, null, false);
                        break;
                    case "saloon":
                        Utility.TryOpenShopMenu(Game1.shop_saloon, null, false);
                        break;
                    case "sandyShop":
                        SandyShop();
                        break;
                    case "return":
                        DelayedAction.functionAfterDelay(MainCategory, Delay);
                        break;
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
                    case "adventureShop":
                        Utility.TryOpenShopMenu(Game1.shop_adventurersGuild, null, false);
                        break;
                    case "blacksmith":
                        Utility.TryOpenShopMenu(Game1.shop_blacksmith, null, false);
                        break;
                    case "toolUpgrades":
                        Utility.TryOpenShopMenu(Game1.shop_blacksmithUpgrades, null, false);
                        break;
                    case "crushGeodes":
                        Game1.activeClickableMenu = new StardewValley.Menus.GeodeMenu();
                        break;
                    case "desertTrader":
                        DesertTrader();
                        break;
                    case "return2":
                        DelayedAction.functionAfterDelay(MainCategory, Delay);
                        break;
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
                    case "carpenter":
                        Utility.TryOpenShopMenu(Game1.shop_carpenter, null, false);
                        break;
                    case "buildBuildings":
                        BuildingMenu("Robin");
                        break;
                    case "wizard":
                        WizardMenu("Wizard");
                        break;
                    case "return3":
                        DelayedAction.functionAfterDelay(MainCategory, Delay);
                        break;
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
                    case "supplies":
                        Utility.TryOpenShopMenu(Game1.shop_animalSupplies, null, false);
                        break;
                    case "animalShop":
                        MarnieMenu();
                        break;
                    case "adoptPet":
                        Utility.TryOpenShopMenu(Game1.shop_petAdoption, null, false);
                        break;
                    case "return4":
                        DelayedAction.functionAfterDelay(MainCategory, Delay);
                        break;
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
                    case "wanderingTrader":
                        Utility.TryOpenShopMenu(Game1.shop_travelingCart, null, false);
                        break;
                    case "dwarf":
                        DwarfShop();
                        break;
                    case "krobus":
                        KrobusShop();
                        break;
                    case "othReturn":
                        DelayedAction.functionAfterDelay(MainCategory, Delay);
                        break;
                }
            };
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
        private void CleanUp(object sender, ReturnedToTitleEventArgs e)
        {
            canSkip = false;
            lastLocationName = null;
            wasBTapped = false;
            lastTilePos = Vector2.Zero;
        }
        private void Key(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.F1 && Context.IsPlayerFree && Context.IsWorldReady)
            {
                if (Helper.Input.IsDown(SButton.LeftShift) && Helper.Input.IsDown(SButton.LeftControl))
                {
                    MainCategory();
                }
            }
        }
        private static void OpenMain(ref bool __result)
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
        private void Save()
        {
            canSkip = true;
            lastLocationName = Game1.currentLocation.NameOrUniqueName;
            lastTilePos = Game1.player.Tile;
        }
        private static bool Skip()
        {
            if (!SA.canSkip) { return true; }

            try
            {
                SA.canSkip = false;
                LocationRequest req = Game1.getLocationRequest(SA.lastLocationName);
                req.OnWarp += delegate
                {
                    Game1.exitActiveMenu();
                    Game1.viewportHold = 0;
                    Game1.dialogueUp = false;
                    Game1.viewportFreeze = false;
                    Game1.player.viewingLocation.Value = null;
                    Game1.displayFarmer = true;
                    Game1.displayHUD = true;
                    Game1.currentLocation.resetForPlayerEntry();
                    Game1.player.forceCanMove();
                };
                Game1.warpFarmer(
                    req,
                    (int)SA.lastTilePos.X,
                    (int)SA.lastTilePos.Y,
                    Game1.player.FacingDirection
                );
                return false;
            }
            catch (Exception ex)
            {
                M.Log($"Failed to skip method: {ex.Message}", LogLevel.Error);
                return true;
            }
        }
        private void FlagReset(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu == null && canSkip) { canSkip = false; }
        }
        private void BuildingMenu(string npc)
        {
            Save();
            Game1.activeClickableMenu = new StardewValley.Menus.CarpenterMenu(npc);
        }
        private void MarnieMenu()
        {
            Save();
            var location = Game1.getFarm();
            List<StardewValley.Object> stock = Utility.getPurchaseAnimalStock(location);
            Game1.activeClickableMenu = new StardewValley.Menus.PurchaseAnimalsMenu(stock);
        }
        private void WizardMenu(string npc)
        {
            if (Game1.player.hasMagicInk)
            {
                Save();
                Game1.activeClickableMenu = new StardewValley.Menus.CarpenterMenu(npc);
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
    }
}
