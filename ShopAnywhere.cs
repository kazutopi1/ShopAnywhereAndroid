using StardewValley;
using StardewValley.Menus;
using StardewValley.Mobile;
using StardewModdingAPI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System;

namespace ShopAnywhere
{
    public class Shop : Mod
    {
        private static Response[] categories;

        private static StardewValley.GameLocation.afterQuestionBehavior categoriesOptionsLogic;

        private static bool wasBTapped = false;

        private static string lastLocationName;

        private static Vector2 lastTilePos;

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(VirtualJoypad), nameof(VirtualJoypad.ButtonBPressed)),
                postfix: new HarmonyMethod(typeof(Shop), nameof(Shop.Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenu)),
                postfix: new HarmonyMethod(typeof(Shop), nameof(Shop.Postfix2))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild)),
                postfix: new HarmonyMethod(typeof(Shop), nameof(Shop.Postfix3))
            );
        }
        private static void Postfix(ref bool __result)
        {
            if (Context.IsPlayerFree && !wasBTapped && __result)
            {
                if (Game1.player.CurrentItem is StardewValley.Item item)
                {
                    if (item.QualifiedItemId.Equals("(O)kt.shop"))
                    {
                        categories = new Response[]
                        {
                            new Response("category1", "General Goods"),
                            new Response("category2", "Combat and Mining"),
                            new Response("category3", "Building"),
                            new Response("others", "Others"),
                            new Response("doNothing", "Close")
                        };
                        categoriesOptionsLogic = (Farmer who, string whichAnswer) =>
                        {
                            if (whichAnswer == "category1")
                            {
                                DelayedAction.functionAfterDelay(() =>
                                {
                                    category1();
                                }, 34);
                            }
                            else if (whichAnswer == "category2")
                            {
                                DelayedAction.functionAfterDelay(() =>
                                {
                                    category2();
                                }, 34);
                            }
                            else if (whichAnswer == "category3")
                            {
                                DelayedAction.functionAfterDelay(() =>
                                {
                                    category3();
                                }, 34);
                            }
                            else if (whichAnswer == "others")
                            {
                                DelayedAction.functionAfterDelay(() =>
                                {
                                    others();
                                }, 34);
                            }
                        };

                        Game1.currentLocation.createQuestionDialogue(
                            question: "Categories",
                            answerChoices: categories,
                            afterDialogueBehavior: categoriesOptionsLogic,
                            speaker: null
                        );
                    }
                }
            }
            wasBTapped = __result;
        }
        private static void Postfix2()
        {
            DelayedAction.functionAfterDelay(() =>
            {
                Game1.warpFarmer(
                    lastLocationName,
                    (int)lastTilePos.X,
                    (int)lastTilePos.Y,
                    Game1.player.FacingDirection,
                    doFade: false
                );
                Game1.player.viewingLocation.Value = null;
                Game1.displayHUD = true;
                Game1.currentLocation.resetForPlayerEntry();
                Game1.player.forceCanMove();
                Game1.exitActiveMenu();

            }, 50);
        }
        private static void Postfix3()
        {
            DelayedAction.functionAfterDelay(() =>
            {
                Game1.warpFarmer(
                    lastLocationName,
                    (int)lastTilePos.X,
                    (int)lastTilePos.Y,
                    Game1.player.FacingDirection,
                    doFade: false
                );
                Game1.player.viewingLocation.Value = null;
                Game1.displayHUD = true;
                Game1.currentLocation.resetForPlayerEntry();
                Game1.player.forceCanMove();
                Game1.exitActiveMenu();

            }, 50);
        }
        private static void category1()
        {
            Response[] cat1 = new Response[]
            {
                new Response("seedShop", "Pierre's General Store"),
                new Response("fishShop", "Willy's Shop"),
                new Response("saloon", "Saloon"),
                new Response("return", "Return")
            };
            StardewValley.GameLocation.afterQuestionBehavior cat1Logic = (Farmer who, string cat1answers) =>
            {
                if (cat1answers == "seedShop")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_generalStore,
                        ownerName: null
                    );
                }
                else if (cat1answers == "fishShop")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_fish,
                        ownerName: null
                    );
                }
                else if (cat1answers == "saloon")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_saloon,
                        ownerName: null
                    );
                }
                else if (cat1answers == "return")
                {
                    DelayedAction.functionAfterDelay(() =>
                    {
                        Game1.currentLocation.createQuestionDialogue(
                            question: "Categories",
                            answerChoices: categories,
                            afterDialogueBehavior: categoriesOptionsLogic,
                            speaker: null
                        );
                    }, 34);
                }
            };
            Game1.currentLocation.createQuestionDialogue(
                question: "General Goods",
                answerChoices: cat1,
                afterDialogueBehavior: cat1Logic,
                speaker: null
            );
        }
        private static void category2()
        {
            Response[] cat2 = new Response[]
            {
                new Response("adventureShop", "Adventurer's Guild Shop"),
                new Response("blacksmith", "Clint's Shop"),
                new Response("toolUpgrades", "Tool Upgrades"),
                new Response("desertTrader", "Desert Trader"),
                new Response("return2", "Return")
            };
            StardewValley.GameLocation.afterQuestionBehavior cat2Logic = (Farmer who, string cat2answers) =>
            {
                if (cat2answers == "adventureShop")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_adventurersGuild,
                        ownerName: null
                    );
                }
                else if (cat2answers == "blacksmith")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_blacksmith,
                        ownerName: null
                    );
                }
                else if (cat2answers == "toolUpgrades")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_blacksmithUpgrades,
                        ownerName: null
                    );
                }
                else if (cat2answers == "desertTrader")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_desertTrader,
                        ownerName: null
                    );
                }
                else if (cat2answers == "return2")
                {
                    DelayedAction.functionAfterDelay(() =>
                    {
                        Game1.currentLocation.createQuestionDialogue(
                            question: "Categories",
                            answerChoices: categories,
                            afterDialogueBehavior: categoriesOptionsLogic,
                            speaker: null
                        );
                    }, 34);
                }
            };
            Game1.currentLocation.createQuestionDialogue(
                question: "Combat and Mining",
                answerChoices: cat2,
                afterDialogueBehavior: cat2Logic,
                speaker: null
            );
        }
        private static void category3()
        {
            Response[] cat3 = new Response[]
            {
                new Response("carpenter", "Robin's Shop"),
                new Response("buildBuildings", "Build Buildings"),
                new Response("return3", "Return")
            };
            StardewValley.GameLocation.afterQuestionBehavior cat3Logic = (Farmer who, string cat3answers) =>
            {
                if (cat3answers == "carpenter")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_carpenter,
                        ownerName: null
                    );
                }
                else if (cat3answers == "buildBuildings")
                {
                    DelayedAction.functionAfterDelay(() =>
                    {
                        lastLocationName = Game1.currentLocation.Name;
                        lastTilePos = Game1.player.Tile;

                        Game1.activeClickableMenu = new StardewValley.Menus.CarpenterMenu("Robin");
                    }, 34);
                }
                else if (cat3answers == "return3")
                {
                    DelayedAction.functionAfterDelay(() =>
                    {
                        Game1.currentLocation.createQuestionDialogue(
                            question: "Categories",
                            answerChoices: categories,
                            afterDialogueBehavior: categoriesOptionsLogic,
                            speaker: null
                        );
                    }, 34);
                }
            };
            Game1.currentLocation.createQuestionDialogue(
                question: "Building",
                answerChoices: cat3,
                afterDialogueBehavior: cat3Logic,
                speaker: null
            );
        }
        private static void others()
        {
            Response[] oth = new Response[]
            {
                new Response("wanderingTrader", "Traveling Cart"),
                new Response("dwarf", "Dwarf's Shop"),
                new Response("krobus", "Krobus's Shop"),
                new Response("othReturn", "Return")
            };
            StardewValley.GameLocation.afterQuestionBehavior othLogic = (Farmer who, string othAnswers) =>
            {
                if (othAnswers == "wanderingTrader")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_travelingCart,
                        ownerName: null
                    );
                }
                else if (othAnswers == "dwarf")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_dwarf,
                        ownerName: null
                    );
                }
                else if (othAnswers == "krobus")
                {
                    Utility.TryOpenShopMenu(
                        shopId: Game1.shop_krobus,
                        ownerName: null
                    );
                }
                else if (othAnswers == "othReturn")
                {
                    DelayedAction.functionAfterDelay(() =>
                    {
                        Game1.currentLocation.createQuestionDialogue(
                            question: "Categories",
                            answerChoices: categories,
                            afterDialogueBehavior: categoriesOptionsLogic,
                            speaker: null
                        );
                    }, 34);
                }
            };
            Game1.currentLocation.createQuestionDialogue(
                question: "Other Shops",
                answerChoices: oth,
                afterDialogueBehavior: othLogic,
                speaker: null
            );
        }
    }
}
