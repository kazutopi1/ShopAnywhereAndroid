using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using Microsoft.Xna.Framework;

namespace ShopAnywhereAndroid
{
    public class Shops
    {
        public static Shops Instance { get; private set; }

        readonly IMonitor Monitor;

        readonly IModHelper Helper;

        public string lastLocationName;

        public Vector2? lastTilePos;

        public bool canSkip = false;

        public const string KTShop = "(O)kt.shop";

        public Shops(IModHelper helper, IMonitor monitor)
        {
            Instance = this;

            this.Helper = helper;

            this.Monitor = monitor;
        }

        public void Categories()
        {
            Game1.currentLocation.createQuestionDialogue(
                Helper.Translation.Get("option.categories"),
                S_Events.Instance.categories,
                S_Events.Instance.categoriesOptionsLogic
            );
        }

        public void General()
        {
            Game1.currentLocation.createQuestionDialogue(
                Helper.Translation.Get("option.general"),
                S_Events.Instance.general,
                S_Events.Instance.generalLogic
            );
        }

        public void CombatAndMining()
        {
            Game1.currentLocation.createQuestionDialogue(
                Helper.Translation.Get("option.combat"),
                S_Events.Instance.combatAndMining,
                S_Events.Instance.combatAndMiningLogic
            );
        }

        public void Building()
        {
            Game1.currentLocation.createQuestionDialogue(
                Helper.Translation.Get("option.building"),
                S_Events.Instance.building,
                S_Events.Instance.buildingLogic
            );
        }

        public void Animals()
        {
            Game1.currentLocation.createQuestionDialogue(
                Helper.Translation.Get("option.animals"),
                S_Events.Instance.animals,
                S_Events.Instance.animalsLogic
            );
        }

        public void Others()
        {
            Game1.currentLocation.createQuestionDialogue(
                Helper.Translation.Get("option.others"),
                S_Events.Instance.oth,
                S_Events.Instance.othLogic
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
            if (!Config.Instance.AllowMultipleBuild && Game1.netWorldState.Value.Builders.ContainsKey(npc))
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

    public class Config
    {
        public static Config Instance { get; set; }

        public SButton Keybind { get; set; } = SButton.Q;

        public bool EnableKeybind { get; set; } = false;

        public bool AllowMultipleBuild { get; set; } = true;
    }
}
