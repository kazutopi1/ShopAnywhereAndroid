using StardewValley;
using StardewValley.Menus;
using StardewValley.Mobile;
using StardewModdingAPI;
using HarmonyLib;
using System;

namespace ShopAnywhereAndroid
{
    public static class ModHarmonyPatches
    {
        public static void ApplyPatch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.warpFarmer),
                new Type[]
                {
                    typeof(LocationRequest),
                    typeof(int),
                    typeof(int),
                    typeof(int)
                }),
                prefix: new HarmonyMethod(typeof(ModHarmonyPatches), nameof(ModHarmonyPatches.WarpFarmer_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.pressActionButton)),
                postfix: new HarmonyMethod(typeof(ModHarmonyPatches), nameof(ModHarmonyPatches.pressActionButton_postfix))
            );
        }
        public static void WarpFarmer_prefix(ref LocationRequest locationRequest, ref int tileX, ref int tileY, ref int facingDirectionAfterWarp)
        {
            try
            {
                if (Shops.Instance.canSkip)
                {
                    var req = Game1.getLocationRequest(Shops.Instance.savedLocationName);

                    var onWarpDelegate = (LocationRequest.Callback)AccessTools.Field(typeof(LocationRequest), nameof(LocationRequest.OnWarp))
                        .GetValue(locationRequest);

                    if (onWarpDelegate != null)
                    {
                        req.OnWarp += onWarpDelegate;
                    }
                    req.OnWarp += () =>
                    {
                        ModEntry.Instance.Monitor.Log($"Warping to {Shops.Instance.savedLocationName}", LogLevel.Trace);
                        ModEntry.Instance.Monitor.Log($"Warping to Tile position: {Shops.Instance.savedTilePosition}", LogLevel.Trace);
                    };
                    locationRequest = req;
                    tileX = (int)Shops.Instance.savedTilePosition.Value.X;
                    tileY = (int)Shops.Instance.savedTilePosition.Value.Y;
                    facingDirectionAfterWarp = Game1.player.FacingDirection;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.LogOnce($"{ex}", LogLevel.Error);
            }
        }
        public static void pressActionButton_postfix(ref bool __result)
        {
            try
            {
                if (__result)
                {
                    if (Game1.player.CurrentItem?.QualifiedItemId == Shops.KTShop)
                    {
                        if (!Game1.player.IsBusyDoingSomething())
                        {
                            Game1.activeClickableMenu = new ShopAnywhereMenu();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance.Monitor.LogOnce($"{ex}", LogLevel.Error);
            }
        }
    }
}