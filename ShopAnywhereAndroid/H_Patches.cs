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

namespace ShopAnywhereAndroid
{
    public class H_Patches
    {
        static IMonitor Monitor;

        static IModHelper Helper;

        static bool wasBTapped = false;

        public H_Patches(Harmony harmony, IModHelper helper, IMonitor monitor)
        {
            Helper = helper;

            Monitor = monitor;

            harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenu)),
                prefix: new HarmonyMethod(typeof(H_Patches), nameof(H_Patches.Skip_returnToCarpentryMenu))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild)),
                prefix: new HarmonyMethod(typeof(H_Patches), nameof(H_Patches.Skip_returnToCarpentryMenuAfterSuccessfulBuild))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnToShopMenu)),
                prefix: new HarmonyMethod(typeof(H_Patches), nameof(H_Patches.Skip_setUpForReturnToShopMenu))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal)),
                prefix: new HarmonyMethod(typeof(H_Patches), nameof(H_Patches.Skip_setUpForReturnAfterPurchasingAnimal))
            );

            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(VirtualJoypad), nameof(VirtualJoypad.ButtonBPressed)),
                postfix: new HarmonyMethod(typeof(H_Patches), nameof(H_Patches.Postfix_ButtonBPressed))
            );
        }
        public static bool Skip_returnToCarpentryMenu(CarpenterMenu __instance)
        {
            if (!Shops.Instance.canSkip) { return true; }

            try
            {
                Helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(true);
                Helper.Reflection.GetField<Building>(__instance, "_selectedBuilding").SetValue(null);

                foreach (var loc in Game1.locations)
                {
                    foreach (var building in loc.buildings)
                    {
                        building.color = Color.White;
                    }
                }
                Helper.Reflection.GetField<bool>(__instance, "demolishing").SetValue(false);
                Helper.Reflection.GetField<bool>(__instance, "moveButtonHeld").SetValue(false);
                Helper.Reflection.GetField<bool>(__instance, "demolishButtonHeld").SetValue(false);
                Helper.Reflection.GetField<bool>(__instance, "paintButtonHeld").SetValue(false);
                LocationRequest req = Game1.getLocationRequest(Shops.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    Helper.Reflection.GetField<bool>(__instance, "onFarm").SetValue(false);
                    Game1.player.viewingLocation.Value = null;
                    Helper.Reflection.GetField<bool>(__instance, "upgrading").SetValue(false);
                    Helper.Reflection.GetField<bool>(__instance, "moving").SetValue(false);
                    Helper.Reflection.GetField<bool>(__instance, "painting").SetValue(false);
                    Helper.Reflection.GetField<Building>(__instance, "buildingToMove").SetValue(null);
                    Helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(false);
                    Helper.Reflection.GetField<bool>(__instance, "demolishing").SetValue(false);
                    Helper.Reflection.GetField<bool>(__instance, "buildButtonHeld").SetValue(false);
                    Game1.displayHUD = true;
                    Game1.viewportFreeze = false;
                    Game1.viewport.Location = new Location((int)Shops.Instance.lastTilePos.Value.X, (int)Shops.Instance.lastTilePos.Value.Y);
                    Helper.Reflection.GetField<bool>(__instance, "drawBG").SetValue(true);
                    Game1.displayFarmer = true;
                    if (Game1.options.SnappyMenus)
                    {
                        __instance.populateClickableComponentList();
                        __instance.snapToDefaultClickableComponent();
                    }
                    Helper.Reflection.GetMethod(__instance, "resetBounds").Invoke();
                };
                Game1.warpFarmer(
                    req,
                    (int)Shops.Instance.lastTilePos.Value.X,
                    (int)Shops.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                return false;
            }
            catch (Exception ex)
            {
                Monitor.LogOnce($"{ex}", LogLevel.Error);
                return true;
            }
        }
        public static bool Skip_returnToCarpentryMenuAfterSuccessfulBuild(CarpenterMenu __instance)
        {
            if (!Shops.Instance.canSkip) { return true; }

            try
            {
                LocationRequest req = Game1.getLocationRequest(Shops.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    Game1.displayHUD = true;
                    Game1.player.viewingLocation.Value = null;
                    Game1.viewportFreeze = false;
                    Game1.viewport.Location = new Location((int)Shops.Instance.lastTilePos.Value.X, (int)Shops.Instance.lastTilePos.Value.Y);
                    Helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(false);
                    Game1.displayFarmer = true;
                    Game1.exitActiveMenu();
                    Helper.Reflection.GetMethod(__instance, "resetBounds").Invoke();
                };
                Game1.warpFarmer(
                    req,
                    (int)Shops.Instance.lastTilePos.Value.X,
                    (int)Shops.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                Helper.Reflection.GetField<bool>(__instance, "demolishing").SetValue(false);
                Helper.Reflection.GetField<bool>(__instance, "moveButtonHeld").SetValue(false);
                Helper.Reflection.GetField<bool>(__instance, "demolishButtonHeld").SetValue(false);
                Helper.Reflection.GetField<bool>(__instance, "paintButtonHeld").SetValue(false);
                return false;
            }
            catch (Exception ex)
            {
                Monitor.LogOnce($"{ex}", LogLevel.Error);
                return true;
            }
        }
        public static bool Skip_setUpForReturnToShopMenu(PurchaseAnimalsMenu __instance)
        {
            if (!Shops.Instance.canSkip) { return true; }

            try
            {
                Helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(false);
                Game1.displayFarmer = true;
                LocationRequest req = Game1.getLocationRequest(Shops.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    Helper.Reflection.GetField<bool>(__instance, "onFarm").SetValue(false);
                    Game1.player.viewingLocation.Value = null;
                    Game1.displayHUD = true;
                    Game1.viewportFreeze = false;
                    Helper.Reflection.GetField<bool>(__instance, "namingAnimal").SetValue(false);
                    var textBox = Helper.Reflection.GetField<TextBox>(__instance, "textBox").GetValue();
                    var e = Helper.Reflection.GetField<TextBoxEvent>(__instance, "e").GetValue();
                    textBox.OnEnterPressed -= e;
                    textBox.Selected = false;
                    if (Game1.options.SnappyMenus)
                    {
                        __instance.snapToDefaultClickableComponent();
                    }
                };
                Game1.warpFarmer(
                    req,
                    (int)Shops.Instance.lastTilePos.Value.X,
                    (int)Shops.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                return false;
            }
            catch (Exception ex)
            {
                Monitor.LogOnce($"{ex}", LogLevel.Error);
                return true;
            }
        }
        public static bool Skip_setUpForReturnAfterPurchasingAnimal(PurchaseAnimalsMenu __instance)
        {
            if (!Shops.Instance.canSkip) { return true; }

            try
            {
                LocationRequest req = Game1.getLocationRequest(Shops.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    Helper.Reflection.GetField<bool>(__instance, "onFarm").SetValue(false);
                    Game1.player.viewingLocation.Value = null;
                    if (__instance.okButton != null)
                    {
                        __instance.okButton.bounds.X = __instance.xPositionOnScreen + __instance.width + 4;
                    }
                    Game1.displayHUD = true;
                    Game1.displayFarmer = true;
                    Helper.Reflection.GetField<bool>(__instance, "freeze").SetValue(false);
                    var textBox = Helper.Reflection.GetField<TextBox>(__instance, "textBox").GetValue();
                    var e = Helper.Reflection.GetField<TextBoxEvent>(__instance, "e").GetValue();
                    textBox.OnEnterPressed -= e;
                    textBox.Selected = false;
                    Game1.viewportFreeze = false;
                    Game1.exitActiveMenu();
                };
                Game1.warpFarmer(
                    req,
                    (int)Shops.Instance.lastTilePos.Value.X,
                    (int)Shops.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );
                return false;
            }
            catch (Exception ex)
            {
                Monitor.LogOnce($"{ex}", LogLevel.Error);
                return true;
            }
        }
        public static void Postfix_ButtonBPressed(ref bool __result)
        {
            try
            {
                if (!Context.IsPlayerFree || Game1.player.IsBusyDoingSomething()) { return; }

                if (__result && !wasBTapped && Game1.player.CurrentItem?.QualifiedItemId == Shops.KTShop)
                {
                    Shops.Instance.Categories();
                }
                wasBTapped = __result;
            }
            catch (Exception ex)
            {
                Monitor.LogOnce($"{ex}", LogLevel.Error);
            }
        }
    }
}
