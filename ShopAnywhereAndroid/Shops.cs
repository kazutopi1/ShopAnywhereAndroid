using StardewValley;
using StardewValley.Tools;
using StardewValley.Menus;
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
        public string savedLocationName;
        public Vector2? savedTilePosition;
        public bool canSkip = false;
        public bool canSkipDialogue = false;
        public const string KTShop = "(O)kt.shop";

        public Shops(IModHelper helper, IMonitor monitor)
        {
            Instance = this;
            this.Helper = helper;
            this.Monitor = monitor;
        }

        public void SavePosition()
        {
            canSkip = true;
            canSkipDialogue = true;
            savedLocationName = Game1.currentLocation.NameOrUniqueName;
            savedTilePosition = Game1.player.Tile;
        }

        public void StopDialogue(DialogueBox d)
        {
            d.exitThisMenu();
            Game1.dialogueUp = false;
            Game1.currentSpeaker = null;
            Game1.player.forceCanMove();
            canSkipDialogue = false;
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
            SavePosition();
            Game1.currentLocation.ShowConstructOptions(npc);
        }

        public void ItemRecoveryShop()
        {
            Utility.TryOpenShopMenu(Game1.shop_adventurersGuildItemRecovery, null, false);
            canSkipDialogue = true;
        }
        public void ToolUpgradeShop()
        {
            var tool = Game1.player.toolBeingUpgraded;
            var timeLeft = Game1.player.daysLeftForToolUpgrade;

            if (tool.Value == null)
            {
                Utility.TryOpenShopMenu(Game1.shop_blacksmithUpgrades, null, false);
                canSkipDialogue = true;
            }
            else if (tool.Value != null && timeLeft.Value <= 0)
            {
                if (!Game1.player.isInventoryFull())
                {
                    Tool value = tool.Value;
                    tool.Value = null;
                    Game1.player.hasReceivedToolUpgradeMessageYet = false;
                    Game1.player.holdUpItemThenMessage(value);

                    if (value is GenericTool)
                    {
                        value.actionWhenClaimed();
                    }
                    else
                    {
                        Game1.player.addItemToInventoryBool(value);
                    }
                }

            }
            else if (tool.Value != null && timeLeft.Value >= 0)
            {
                Game1.drawObjectDialogue($"Your {tool.Value.DisplayName} is currently being upgraded.");
            }
        }
    }

    public class Config
    {
        public static Config Instance { get; set; }
        public SButton Keybind { get; set; } = SButton.K;
        public bool EnableKeybind { get; set; } = false;
        public bool AllowMultipleBuild { get; set; } = true;
    }
}