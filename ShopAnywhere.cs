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
        private static Response[] categories;
        private static StardewValley.GameLocation.afterQuestionBehavior categoriesOptionsLogic;
        private static Response[] cat1;
        private static StardewValley.GameLocation.afterQuestionBehavior cat1Logic;
        private static Response[] cat2;
        private static StardewValley.GameLocation.afterQuestionBehavior cat2Logic;
        private static Response[] cat3;
        private static StardewValley.GameLocation.afterQuestionBehavior cat3Logic;
        private static Response[] cat4;
        private static StardewValley.GameLocation.afterQuestionBehavior cat4Logic;
        private static Response[] oth;
        private static StardewValley.GameLocation.afterQuestionBehavior othLogic;
        private static bool wasBTapped = false;
        private static string lastLocationName;
        private static Vector2 lastTilePos;
        private const int Delay = 50;
        private const string KTShop = "(O)kt.shop";
        private static bool warpBack = false;

        public override void Entry(IModHelper helper)
        {
            if (Constants.TargetPlatform != GamePlatform.Android)
                return;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(VirtualJoypad), nameof(VirtualJoypad.ButtonBPressed)),
                postfix: new HarmonyMethod(typeof(Shop), nameof(Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.warpFarmer), new[]
                {
                    typeof(LocationRequest), typeof(int), typeof(int), typeof(int), typeof(bool)
                }),
                postfix: new HarmonyMethod(typeof(Shop), nameof(Shop.WarpPlayer))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.exitActiveMenu)),
                postfix: new HarmonyMethod(typeof(Shop), nameof(FlagReset))
            );
            helper.Events.Input.ButtonPressed += Key;
            helper.Events.GameLoop.ReturnedToTitle += CleanUp;
            helper.Events.GameLoop.GameLaunched += QuestionDialogueCache;
        }
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
                        CrushGeodeMenu();
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
        private void CleanUp(object sender, ReturnedToTitleEventArgs e)
        {
            warpBack = false;
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
        private static void Postfix(ref bool __result)
        {
            if (Context.IsPlayerFree && !wasBTapped && __result)
            {
                if (Game1.player.CurrentItem is StardewValley.Item item)
                {
                    if (item.QualifiedItemId == KTShop)
                    {
                        MainCategory();
                    }
                }
            }
            wasBTapped = __result;
        }
        private static void FlagReset() { warpBack = false; }
        private static void MainCategory() { wasBTapped = false; QuestionDialogue("Categories", categories, categoriesOptionsLogic); }
        private static void category1() { QuestionDialogue("General Goods", cat1, cat1Logic); }
        private static void category2() { QuestionDialogue("Combat and Mining", cat2, cat2Logic); }
        private static void category3() { QuestionDialogue("Building", cat3, cat3Logic); }
        private static void category4() { QuestionDialogue("Animals", cat4, cat4Logic); }
        private static void others() { QuestionDialogue("Other Shops", oth, othLogic); }

        private static void WarpPlayer()
        {
            if (!warpBack)
            {
                return;
            }
            warpBack = false;
            Game1.warpFarmer(
                lastLocationName,
                (int)lastTilePos.X,
                (int)lastTilePos.Y,
                Game1.player.FacingDirection,
                doFade: false
            );
            Game1.viewportHold = 0;
            Game1.player.viewingLocation.Value = null;
            Game1.displayHUD = true;
            Game1.currentLocation.resetForPlayerEntry();
            Game1.player.forceCanMove();
            Game1.exitActiveMenu();
        }
        private static void QuestionDialogue(
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
        private static void BuildingMenu(string npc)
        {
            warpBack = true;
            lastLocationName = Game1.currentLocation.NameOrUniqueName;
            lastTilePos = Game1.player.Tile;
            Game1.activeClickableMenu = new StardewValley.Menus.CarpenterMenu(npc);
        }
        private static void MarnieMenu()
        {
            warpBack = true;
            var location = Game1.getFarm();
            lastLocationName = Game1.currentLocation.NameOrUniqueName;
            lastTilePos = Game1.player.Tile;
            List<StardewValley.Object> stock = Utility.getPurchaseAnimalStock(location);
            Game1.activeClickableMenu = new StardewValley.Menus.PurchaseAnimalsMenu(stock);
        }
        private static void KrobusShop()
        {
            if (Game1.player.hasRustyKey)
            {
                Utility.TryOpenShopMenu(Game1.shop_krobus, null, false);
            }
            else { Game1.drawObjectDialogue("Acquire the Rusty Key first to access this Shop."); }
        }
        private static void DesertTrader()
        {
            if (Game1.player.hasOrWillReceiveMail("ccVault") || Game1.player.hasOrWillReceiveMail("JojaVault"))
            {
                Utility.TryOpenShopMenu(Game1.shop_desertTrader, null, false);
            }
            else { Game1.drawObjectDialogue("Fix the Bus first to access this Shop."); }
        }
        private static void DwarfShop()
        {
            if (Game1.player.canUnderstandDwarves)
            {
                Utility.TryOpenShopMenu(Game1.shop_dwarf, null, false);
            }
            else { Game1.drawObjectDialogue("Donate all 4 Dwarf Scrolls to access this shop."); }
        }
        private static void SandyShop()
        {
            if (Game1.player.hasOrWillReceiveMail("ccVault") || Game1.player.hasOrWillReceiveMail("JojaVault"))
            {
                Utility.TryOpenShopMenu(Game1.shop_sandy, null, false);
            }
            else { Game1.drawObjectDialogue("Fix the Bus first to access this Shop."); }
        }
        private static void WizardMenu(string npc)
        {
            if (Game1.player.hasMagicInk)
            {
                warpBack = true;
                lastLocationName = Game1.currentLocation.NameOrUniqueName;
                lastTilePos = Game1.player.Tile;
                Game1.activeClickableMenu = new StardewValley.Menus.CarpenterMenu(npc);
            }
            else { Game1.drawObjectDialogue("Return the Magic Ink to the Wizard to access this shop."); }
        }
        private static void CrushGeodeMenu()
        {
            Game1.activeClickableMenu = new StardewValley.Menus.GeodeMenu();
        }
    }
}
