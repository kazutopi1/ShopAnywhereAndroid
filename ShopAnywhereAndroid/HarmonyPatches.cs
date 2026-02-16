using StardewValley;
using StardewValley.Mobile;
using StardewValley.Menus;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewModdingAPI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using xTile.Dimensions;
using System;

namespace ShopAnywhere
{
    internal class HarmonyPatches
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;

        private static bool wasBTapped = false;
        public const string KTShop = "(O)kt.shop";

        public HarmonyPatches(Harmony harmony, IMonitor Monitor, IModHelper helper)
        {
            _monitor = Monitor;
            _helper = helper;

            harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenu)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Skip_returnToCarpentryMenu))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Skip_returnToCarpentryMenuAfterSuccessfulBuild))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnToShopMenu)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Skip_setUpForReturnToShopMenu))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Skip_setUpForReturnAfterPurchasingAnimal))
            );

            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(VirtualJoypad), nameof(VirtualJoypad.ButtonBPressed)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.Postfix_ButtonBPressed))
            );
        }
        public static bool Skip_returnToCarpentryMenu(CarpenterMenu __instance)
        {
            if (!Shop.Instance.canSkip) { return true; }

            try
            {
                _helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(true);
                _helper.Reflection.GetField<Building>(__instance, "_selectedBuilding").SetValue(null);

                foreach (var loc in Game1.locations)
                {
                    foreach (var building in loc.buildings)
                    {
                        building.color = Color.White;
                    }
                }
                _helper.Reflection.GetField<bool>(__instance, "demolishing").SetValue(false);
                _helper.Reflection.GetField<bool>(__instance, "moveButtonHeld").SetValue(false);
                _helper.Reflection.GetField<bool>(__instance, "demolishButtonHeld").SetValue(false);
                _helper.Reflection.GetField<bool>(__instance, "paintButtonHeld").SetValue(false);
                LocationRequest req = Game1.getLocationRequest(Shop.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    _helper.Reflection.GetField<bool>(__instance, "onFarm").SetValue(false);
                    Game1.player.viewingLocation.Value = null;
                    _helper.Reflection.GetField<bool>(__instance, "upgrading").SetValue(false);
                    _helper.Reflection.GetField<bool>(__instance, "moving").SetValue(false);
                    _helper.Reflection.GetField<bool>(__instance, "painting").SetValue(false);
                    _helper.Reflection.GetField<Building>(__instance, "buildingToMove").SetValue(null);
                    _helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(false);
                    _helper.Reflection.GetField<bool>(__instance, "demolishing").SetValue(false);
                    _helper.Reflection.GetField<bool>(__instance, "buildButtonHeld").SetValue(false);
                    Game1.displayHUD = true;
                    Game1.viewportFreeze = false;
                    Game1.viewport.Location = new Location((int)Shop.Instance.lastTilePos.Value.X, (int)Shop.Instance.lastTilePos.Value.Y);
                    _helper.Reflection.GetField<bool>(__instance, "drawBG").SetValue(true);
                    Game1.displayFarmer = true;
                    if (Game1.options.SnappyMenus)
                    {
                        __instance.populateClickableComponentList();
                        __instance.snapToDefaultClickableComponent();
                    }
                    _helper.Reflection.GetMethod(__instance, "resetBounds").Invoke();
                };
                Game1.warpFarmer(
                    req,
                    (int)Shop.Instance.lastTilePos.Value.X,
                    (int)Shop.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                return false;
            }
            catch (Exception ex)
            {
                _monitor.LogOnce($"{ex}", LogLevel.Error);
                return true;
            }
        }
        public static bool Skip_returnToCarpentryMenuAfterSuccessfulBuild(CarpenterMenu __instance)
        {
            if (!Shop.Instance.canSkip) { return true; }

            try
            {
                LocationRequest req = Game1.getLocationRequest(Shop.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    Game1.displayHUD = true;
                    Game1.player.viewingLocation.Value = null;
                    Game1.viewportFreeze = false;
                    Game1.viewport.Location = new Location((int)Shop.Instance.lastTilePos.Value.X, (int)Shop.Instance.lastTilePos.Value.Y);
                    _helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(false);
                    Game1.displayFarmer = true;
                    Game1.exitActiveMenu();
                    _helper.Reflection.GetMethod(__instance, "resetBounds").Invoke();
                };
                Game1.warpFarmer(
                    req,
                    (int)Shop.Instance.lastTilePos.Value.X,
                    (int)Shop.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                _helper.Reflection.GetField<bool>(__instance, "demolishing").SetValue(false);
                _helper.Reflection.GetField<bool>(__instance, "moveButtonHeld").SetValue(false);
                _helper.Reflection.GetField<bool>(__instance, "demolishButtonHeld").SetValue(false);
                _helper.Reflection.GetField<bool>(__instance, "paintButtonHeld").SetValue(false);
                return false;
            }
            catch (Exception ex)
            {
                _monitor.LogOnce($"{ex}", LogLevel.Error);
                return true;
            }
        }
        public static bool Skip_setUpForReturnToShopMenu(PurchaseAnimalsMenu __instance)
        {
            if (!Shop.Instance.canSkip) { return true; }

            try
            {
                _helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(false);
                Game1.displayFarmer = true;
                LocationRequest req = Game1.getLocationRequest(Shop.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    _helper.Reflection.GetField<bool>(__instance, "onFarm").SetValue(false);
                    Game1.player.viewingLocation.Value = null;
                    Game1.displayHUD = true;
                    Game1.viewportFreeze = false;
                    _helper.Reflection.GetField<bool>(__instance, "namingAnimal").SetValue(false);
                    var textBox = _helper.Reflection.GetField<TextBox>(__instance, "textBox").GetValue();
                    var e = _helper.Reflection.GetField<TextBoxEvent>(__instance, "e").GetValue();
                    textBox.OnEnterPressed -= e;
                    textBox.Selected = false;
                    if (Game1.options.SnappyMenus)
                    {
                        __instance.snapToDefaultClickableComponent();
                    }
                };
                Game1.warpFarmer(
                    req,
                    (int)Shop.Instance.lastTilePos.Value.X,
                    (int)Shop.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                return false;
            }
            catch (Exception ex)
            {
                _monitor.LogOnce($"{ex}", LogLevel.Error);
                return true;
            }
        }
        public static bool Skip_setUpForReturnAfterPurchasingAnimal(PurchaseAnimalsMenu __instance)
        {
            if (!Shop.Instance.canSkip) { return true; }

            try
            {
                LocationRequest req = Game1.getLocationRequest(Shop.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    _helper.Reflection.GetField<bool>(__instance, "onFarm").SetValue(false);
                    Game1.player.viewingLocation.Value = null;
                    if (__instance.okButton != null)
                    {
                        __instance.okButton.bounds.X = __instance.xPositionOnScreen + __instance.width + 4;
                    }
                    Game1.displayHUD = true;
                    Game1.displayFarmer = true;
                    _helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(false);
                    var textBox = _helper.Reflection.GetField<TextBox>(__instance, "textBox").GetValue();
                    var e = _helper.Reflection.GetField<TextBoxEvent>(__instance, "e").GetValue();
                    textBox.OnEnterPressed -= e;
                    textBox.Selected = false;
                    Game1.viewportFreeze = false;
                    Game1.exitActiveMenu();
                };
                Game1.warpFarmer(
                    req,
                    (int)Shop.Instance.lastTilePos.Value.X,
                    (int)Shop.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                return false;
            }
            catch (Exception ex)
            {
                _monitor.LogOnce($"{ex}", LogLevel.Error);
                return true;
            }
        }
        public static void Postfix_ButtonBPressed(ref bool __result)
        {
            try
            {
                if (__result && !wasBTapped && Game1.player.CurrentItem?.QualifiedItemId == KTShop)
                {
                    Shop.Instance.Categories();
                }
                wasBTapped = __result;
            }
            catch (Exception ex)
            {
                _monitor.LogOnce($"{ex}", LogLevel.Error);
            }
        }
    }
}
