using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using System;
using HarmonyLib;
using StardewModdingAPI.Utilities;

namespace ShopAnywhere
{
    internal sealed class Shop : Mod
    {
        public static Shop Instance { get; private set; }
        public Config config;
        public HarmonyPatches p;
        public EventHandler e;

        public string lastLocationName;
        public Vector2? lastTilePos;
        public bool canSkip = false;

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            this.config = helper.ReadConfig<Config>();

            if (Constants.TargetPlatform != GamePlatform.Android)
            {
                var ex = new Exception();
                Monitor.Log($"This mod only supports Android. {ex.ToString()}", LogLevel.Error);
                return;
            }

            var harmony = new Harmony(ModManifest.UniqueID);
            e = new EventHandler(this, helper, Monitor, config);
            p = new HarmonyPatches(harmony, Monitor, helper);
        }
        public void QuestionDialogue(
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
        public void Categories()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.categories"),
                e.categories,
                e.categoriesOptionsLogic
            );
        }
        public void General()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.general"),
                e.general,
                e.generalLogic
            );
        }
        public void CombatAndMining()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.combat"),
                e.combatAndMining,
                e.combatAndMiningLogic
            );
        }
        public void Building()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.building"),
                e.building,
                e.buildingLogic
            );
        }
        public void Animals()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.animals"),
                e.animals,
                e.animalsLogic
            );
        }
        public void Others()
        {
            QuestionDialogue(
                Helper.Translation.Get("option.others"),
                e.oth,
                e.othLogic
            );
        }
        public void SavePosition()
        {
            canSkip = true;
            lastLocationName = Game1.currentLocation.NameOrUniqueName;
            lastTilePos = Game1.player.Tile;
            Monitor.Log($"Position saved: {lastLocationName} {lastTilePos}", LogLevel.Trace);
        }
        public void BuildingMenu(string npc)
        {
            if (!config.AllowMultipleBuild && Game1.netWorldState.Value.Builders.ContainsKey(npc))
            {
                Game1.drawObjectDialogue(Helper.Translation.Get("condition.robin"));
                return;
            }
            SavePosition();
            Game1.currentLocation.ShowConstructOptions(npc);
        }
        public void MarnieMenu()
        {
            SavePosition();
            Game1.currentLocation.ShowAnimalShopMenu();
        }
        public void WizardMenu(string npc)
        {
            if (Game1.player.hasMagicInk)
            {
                SavePosition();
                Game1.currentLocation.ShowConstructOptions(npc);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.wizard")); }
        }
        public void KrobusShop()
        {
            if (Game1.player.hasRustyKey)
            {
                Utility.TryOpenShopMenu(Game1.shop_krobus, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.krobus")); }
        }
        public void DesertTrader()
        {
            if (Game1.player.hasOrWillReceiveMail("ccVault") || Game1.player.hasOrWillReceiveMail("JojaVault"))
            {
                Utility.TryOpenShopMenu(Game1.shop_desertTrader, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.bus")); }
        }
        public void DwarfShop()
        {
            if (Game1.player.canUnderstandDwarves)
            {
                Utility.TryOpenShopMenu(Game1.shop_dwarf, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.dwarf")); }
        }
        public void SandyShop()
        {
            if (Game1.player.hasOrWillReceiveMail("ccVault") || Game1.player.hasOrWillReceiveMail("JojaVault"))
            {
                Utility.TryOpenShopMenu(Game1.shop_sandy, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.bus")); }
        }
        public void TravelingCart()
        {
            if (Game1.dayOfMonth % 7 == 5 || Game1.dayOfMonth % 7 == 0)
            {
                Utility.TryOpenShopMenu(Game1.shop_travelingCart, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.travelingCart")); }
        }
        public void QiGemShop()
        {
            if (IslandWest.IsQiWalnutRoomDoorUnlocked(out int walnutsFound))
            {
                Utility.TryOpenShopMenu(Game1.shop_qiGemShop, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.qiGemShop")); }
        }
        public void HatMouseShop()
        {
            if (Game1.player.hasOrWillReceiveMail("hatter"))
            {
                Utility.TryOpenShopMenu(Game1.shop_hatMouse, null, false);
            }
            else { Game1.drawObjectDialogue(Helper.Translation.Get("condition.hatMouse")); }
        }
    }
    internal class Config
    {
        public SButton Keybind { get; set; } = SButton.Q;
        public bool EnableKeybind { get; set; } = false;
        public bool AllowMultipleBuild { get; set; } = true;
    }
}
