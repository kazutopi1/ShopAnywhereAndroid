using StardewValley;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ShopAnywhereAndroid
{
    public class ShopAnywhereMenu : IClickableMenu
    {
        List<(string key, string shopName)> shopList = new List<(string key, string shopName)>();
        readonly List<ClickableComponent> shopButtons = new List<ClickableComponent>();
        ClickableTextureComponent backButton;
        ClickableTextureComponent forwardButton;

        public static int currentPage = 0;
        int shopsPerPage = 12;
        int totalPages = 1;

        public ShopAnywhereMenu() : base(0, 0, 0, 0, true)
        {
            Game1.playSound("bigSelect");

            // dont change, its easier to calculate with this
            width = Game1.uiViewport.Width;
            height = Game1.uiViewport.Height;
            xPositionOnScreen = 0;
            yPositionOnScreen = 0;

            ShopList();
            Buttons();
            NavButtons();
        }
        void ShopList()
        {
            var T = ModEntry.Instance.Helper.Translation;
            var f = Game1.player;
            var s = shopList;

            s.Clear();
            s.AddRange(new (string key, string shopName)[]
            {
                ("seedShop", T.Get("shop.pierre")),
                ("fishShop", T.Get("shop.willy")),
                ("saloon", T.Get("shop.saloon")),
                ("blacksmith", T.Get("shop.clint")),
                ("toolUpgrades", T.Get("shop.upgrades")),
                ("crushGeodes", T.Get("shop.geodes")),
                ("carpenter", T.Get("shop.robin")),
                ("buildBuildings", T.Get("shop.construct")),
                ("supplies", T.Get("shop.marnie")),
                ("animalShop", T.Get("shop.buyAnimals")),
                ("adoptPet", T.Get("shop.pets")),
                ("hospital", T.Get("shop.hospital")),
                ("jojaMart", T.Get("shop.jojaMart"))
            });
            if (f.hasOrWillReceiveMail("guildMember"))
            {
                s.Add(("adventureShop", T.Get("shop.adventurer")));
                s.Add(("itemRecovery", T.Get("shop.itemRecovery")));
            }
            if (Game1.dayOfMonth % 7 == 5 || Game1.dayOfMonth % 7 == 0)
            {
                s.Add(("wanderingTrader", T.Get("shop.travelingCart")));
            }
            if (f.hasOrWillReceiveMail("ccVault") || f.hasOrWillReceiveMail("JojaVault"))
            {
                s.Add(("sandyShop", T.Get("shop.oasis")));
                s.Add(("desertTrader", T.Get("shop.desertTrader")));
            }
            if (f.canUnderstandDwarves)
            {
                s.Add(("dwarf", T.Get("shop.dwarf")));
            }
            if (f.hasRustyKey)
            {
                s.Add(("krobus", T.Get("shop.krobus")));
            }
            if (f.hasOrWillReceiveMail("hatter"))
            {
                s.Add(("hatMouse", T.Get("shop.hatMouse")));
            }
            if (f.hasClubCard)
            {
                s.Add(("casino", T.Get("shop.casino")));
            }
            if (f.hasMagicInk)
            {
                s.Add(("wizard", T.Get("shop.wizard")));
            }
            if (Game1.netWorldState.Value.GoldenWalnutsFound >= 100)
            {
                s.Add(("qiGem", T.Get("shop.qiGem")));
            }
            if (f.hasOrWillReceiveMail("Island_Resort"))
            {
                s.Add(("resortBar", T.Get("shop.resortBar")));
            }
            if (f.hasOrWillReceiveMail("Island_UpgradeTrader"))
            {
                s.Add(("islandTrader", T.Get("shop.islandTrader")));
            }
            if (f.hasCompletedCommunityCenter() && !f.hasOrWillReceiveMail("JojaMember"))
            {
                shopList.Remove(("jojaMart", T.Get("shop.jojaMart")));
            }
            if (Utility.getDaysOfBooksellerThisSeason().Contains(Game1.dayOfMonth))
            {
                s.Add(("bookSeller", T.Get("shop.bookSeller")));
                s.Add(("tradeBooks", T.Get("shop.tradeBooks")));
            }
            if (f.hasOrWillReceiveMail("Island_VolcanoShortcutOut"))
            {
                s.Add(("volcanoShop", T.Get("shop.volcanoShop")));
            }
            totalPages = (int)Math.Ceiling((double)shopList.Count / shopsPerPage);
            if (totalPages < 1) { totalPages = 1; }
        }
        void Buttons() // if the values inside are hardcoded then dont touch
        {
            shopButtons.Clear();

            int buttonWidth = (int)(width * 0.26); // do not
            int buttonHeight = (int)(height * 0.15); // this too
            int spaceBetweenButtons = 12; // this too
            int startIndex = currentPage * shopsPerPage;
            int endIndex = Math.Min(startIndex + shopsPerPage, shopList.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                int index = i - startIndex;
                int columns = index % 3;
                int rows = index / 3;
                int buttonStartX = (xPositionOnScreen + (int)(width * 0.105)); // dont touch
                int buttonStartY = (yPositionOnScreen + (int)(height * 0.195)); // this too
                int x = buttonStartX + columns * (buttonWidth + spaceBetweenButtons);
                int y = buttonStartY + rows * (buttonHeight + spaceBetweenButtons);

                var shop = shopList[i];
                shopButtons.Add(new ClickableComponent(
                    new Rectangle(x, y, buttonWidth, buttonHeight),
                    shop.key, // this is name
                    shop.shopName // this is label dont forget
                ));
            }
        }
        void NavButtons()
        {
            int leftMenuBorder = xPositionOnScreen;
            int rightMenuBorder = xPositionOnScreen + width;

            backButton = new ClickableTextureComponent( // do not touch any x pos
                new Rectangle(leftMenuBorder + (int)(width * 0.05), (height / 2) - 36, 66, 72),
                Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44),
                1.5f
            );

            forwardButton = new ClickableTextureComponent( // this too dont touch
                new Rectangle(rightMenuBorder - (int)(width * 0.065) - 66, (height / 2) - 36, 66, 72),
                Game1.mouseCursors,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33),
                1.5f
            );
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (backButton.containsPoint(x, y) && currentPage > 0)
            {
                currentPage--;
                Buttons();
                return;
            }
            if (forwardButton.containsPoint(x, y) && currentPage < totalPages - 1)
            {
                currentPage++;
                Buttons();
                return;
            }
            foreach (var button in shopButtons)
            {
                if (button.containsPoint(x, y))
                {
                    OpenShop(button.name);
                    return;
                }
            }
            base.receiveLeftClick(x, y, playSound);
            Game1.playSound("smallSelect");
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);

            width = Game1.uiViewport.Width;
            height = Game1.uiViewport.Height;
            xPositionOnScreen = 0;
            yPositionOnScreen = 0;
            Buttons();
            NavButtons();
        }
        private void OpenShop(string key)
        {
            Game1.exitActiveMenu();

            switch (key)
            {
                case "seedShop": Utility.TryOpenShopMenu(Game1.shop_generalStore, null, false); break;
                case "fishShop": Utility.TryOpenShopMenu(Game1.shop_fish, null, false); break;
                case "saloon": Utility.TryOpenShopMenu(Game1.shop_saloon, null, false); break;
                case "sandyShop": Utility.TryOpenShopMenu(Game1.shop_sandy, null, false); break;
                case "adventureShop": Utility.TryOpenShopMenu(Game1.shop_adventurersGuild, null, false); break;
                case "itemRecovery": Shops.Instance.ItemRecoveryShop(); break;
                case "blacksmith": Utility.TryOpenShopMenu(Game1.shop_blacksmith, null, false); break;
                case "toolUpgrades": Shops.Instance.ToolUpgradeShop(); break;
                case "crushGeodes": Game1.activeClickableMenu = new GeodeMenu(); break;
                case "desertTrader": Utility.TryOpenShopMenu(Game1.shop_desertTrader, null, false); break;
                case "carpenter": Utility.TryOpenShopMenu(Game1.shop_carpenter, null, false); break;
                case "buildBuildings": Shops.Instance.BuildingMenu("Robin"); break;
                case "wizard": Shops.Instance.WizardMenu("Wizard"); break;
                case "supplies": Utility.TryOpenShopMenu(Game1.shop_animalSupplies, null, false); break;
                case "animalShop": Shops.Instance.MarnieMenu(); break;
                case "adoptPet": Utility.TryOpenShopMenu(Game1.shop_petAdoption, null, false); break;
                case "wanderingTrader": Utility.TryOpenShopMenu(Game1.shop_travelingCart, null, false); break;
                case "dwarf": Utility.TryOpenShopMenu(Game1.shop_dwarf, null, false); break;
                case "krobus": Utility.TryOpenShopMenu(Game1.shop_krobus, null, false); break;
                case "hatMouse": Utility.TryOpenShopMenu(Game1.shop_hatMouse, null, false); break;
                case "casino": Utility.TryOpenShopMenu(Game1.shop_casino, null, false); break;
                case "hospital": Utility.TryOpenShopMenu(Game1.shop_hospital, null, false); break;
                case "qiGem": Utility.TryOpenShopMenu(Game1.shop_qiGemShop, null, false); break;
                case "islandTrader": Utility.TryOpenShopMenu(Game1.shop_islandTrader, null, false); break;
                case "resortBar": Utility.TryOpenShopMenu(Game1.shop_resortBar, null, false); break;
                case "jojaMart": Utility.TryOpenShopMenu(Game1.shop_jojaMart, null, false); break;
                case "bookSeller": Utility.TryOpenShopMenu(Game1.shop_bookseller, null, false); break;
                case "tradeBooks": Utility.TryOpenShopMenu(Game1.shop_bookseller_trade, null, false); break;
                case "volcanoShop": Utility.TryOpenShopMenu(Game1.shop_volcanoShop, null, false); break;
            }
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

            foreach (var button in shopButtons)
            {
                IClickableMenu.drawTextureBox(
                    b,
                    button.bounds.X,
                    button.bounds.Y,
                    button.bounds.Width,
                    button.bounds.Height,
                    Color.White
                );
                // this should be always button.label
                Vector2 textSize = Game1.dialogueFont.MeasureString(button.label); // do not change into button.name
                                                                                   // do not change do not change
                var scale = 1f;
                if (textSize.X > button.bounds.Width * 0.90)
                {
                    scale = (float)(button.bounds.Width * 0.90) / textSize.X;
                }

                Vector2 textPosition = new Vector2(
                    button.bounds.Center.X - ((textSize.X * scale) / 2f),
                    button.bounds.Center.Y - ((textSize.Y * scale) / 2f) + 4f
                );

                Utility.drawBoldText(
                    b,
                    button.label,
                    Game1.dialogueFont,
                    textPosition,
                    Color.Black,
                    scale
                );
            }
            backButton.draw(b);
            forwardButton.draw(b);
            base.draw(b);
            drawMouse(b);
        }
    }
}