using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewModdingAPI;
using HarmonyLib;
using System;
using Microsoft.Xna.Framework;

namespace ShopAnywhere
{
    internal class HarmonyPatches
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;

        public HarmonyPatches(Harmony harmony, IMonitor Monitor, IModHelper helper)
        {
            _monitor = Monitor;
            _helper = helper;

            var skipCallback = new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.SkipCallback));
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
                Monitor.Log("Methods succesfully patched", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to patch methods: {ex.ToString()}", LogLevel.Error);
            }
        }
        public static bool SkipCallback(object __instance)
        {
            if (!Shop.Instance.canSkip) { return true; }

            try
            {
                Shop.Instance.canSkip = false;
                foreach (GameLocation location in Game1.locations)
                {
                    foreach (var building in location.buildings)
                    {
                        building.color = Color.White;
                    }
                }
                LocationRequest req = Game1.getLocationRequest(Shop.Instance.lastLocationName);
                req.OnWarp += delegate
                {
                    Game1.exitActiveMenu();
                    Game1.viewportFreeze = false;
                    Game1.displayHUD = true;
                    Game1.displayFarmer = true;
                    Game1.player.viewingLocation.Value = null;

                    if (Shop.Instance.lastTilePos != null && Shop.Instance.lastLocationName != null)
                    {
                        Shop.Instance.lastLocationName = null;
                        Shop.Instance.lastTilePos = null;
                        _monitor.Log("Saved position cleared", LogLevel.Trace);
                    }

                    if (__instance is CarpenterMenu carpenter)
                    {
                        var reflection = _helper.Reflection;
                        reflection.GetField<bool>(carpenter, "onFarm").SetValue(false);
                        reflection.GetField<bool>(carpenter, "upgrading").SetValue(false);
                        reflection.GetField<bool>(carpenter, "demolishing").SetValue(false);
                        reflection.GetField<bool>(carpenter, "moving").SetValue(false);
                        reflection.GetField<bool>(carpenter, "painting").SetValue(false);
                        reflection.GetField<bool>(carpenter, "freeze").SetValue(false);
                        reflection.GetField<bool>(carpenter, "paintButtonHeld").SetValue(false);
                        reflection.GetField<bool>(carpenter, "buildButtonHeld").SetValue(false);
                        reflection.GetField<bool>(carpenter, "moveButtonHeld").SetValue(false);
                        reflection.GetField<bool>(carpenter, "drawBG").SetValue(true);
                        reflection.GetField<Building>(carpenter, "buildingToMove").SetValue(null);
                        if (Game1.options.SnappyMenus)
                        {
                            carpenter.populateClickableComponentList();
                            carpenter.snapToDefaultClickableComponent();
                        }
                        reflection.GetMethod(carpenter, "resetBounds").Invoke();
                    }

                    if (__instance is PurchaseAnimalsMenu animal)
                    {
                        var reflection = _helper.Reflection;
                        reflection.GetField<bool>(animal, "freeze").SetValue(false);
                        reflection.GetField<bool>(animal, "namingAnimal").SetValue(false);
                        reflection.GetField<bool>(animal, "onFarm").SetValue(false);
                    }
                };

                Game1.warpFarmer(
                    req,
                    (int)Shop.Instance.lastTilePos.Value.X,
                    (int)Shop.Instance.lastTilePos.Value.Y,
                    Game1.player.FacingDirection
                );

                _monitor.Log("Method skipped", LogLevel.Trace);
                return false;
            }
            catch (Exception ex)
            {
                _monitor.LogOnce($"Failed to skip Method: {ex.ToString()}", LogLevel.Error);
                return true;
            }
        }
    }
}
