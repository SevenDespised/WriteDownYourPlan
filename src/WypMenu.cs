using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.BellsAndWhistles;

using static WritedownYourPlan.src.MenuEnum;
using static WritedownYourPlan.src.Plan;
using static WritedownYourPlan.src.TimeUtils;

namespace WritedownYourPlan.src;
public class WypMenu : IClickableMenu
{
    public PlanData planData;
    public ModData modData;
    public ModConfig config;
    private readonly Reminder reminder;
    public readonly LoadData loaddata = new();
    public readonly int xPos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - Game1.tileSize * 6 / 2;
    public readonly int yPos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - Game1.tileSize * 6 / 2 - Game1.tileSize;
    public readonly int uiWidth = Game1.tileSize * 6;
    public readonly int uiHeight = Game1.tileSize / 2 + Game1.tileSize * 8;
    public readonly int actionBarHeight = Game1.tileSize;
    public readonly int barHeight = Game1.tileSize + Game1.tileSize / 2;
    public Plan newPlan = new();
    Plan hoverPlan = new();
    Rectangle menuBound;
    List<Rectangle> chooseBoundList = new();
    Dictionary<string, Rectangle> chooseBoundDict = new();
    readonly SearchBox titleBox;
    readonly SearchBox itemBox;
    readonly ActionBar actionBar;
    PageButton updownButton;
    readonly PageButton leftrightButton;
    readonly List<CancelButton> cancelButtons;
    readonly List<ChooseButton> chooseButtons;
    SelectButton selectRepeatButton;
    readonly List<SelectButton> selectSpecialDayButtons = new();
    readonly List<SelectByArrowButton> arrowDateButtons = new();
    readonly HoverFlags hoverFlags = new();
    // pageIndex: 0-mainPage, 1-editPage, 2-choosePage
    int pageIndex = (int)PageEnum.mainPage;
    // planPageIndex: the index of plan page
    int planPageIndex = 0;
    // planNum: the index of the selected plan on the current page
    int planIndexOnPage;
    // choosePageIndex: 0-npc, 1-location, 2-action, 3-time, 4-item
    int choosePageIndex;
    List<string> pririotyLocation = new();
    int locationDisplayIndex = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="WypMenu"/> class with specified model and mod data.
    /// </summary>
    /// <param name="planData">The data model containing plan information.</param>
    /// <param name="modData">The data object containing global mod-related information.</param>
    /// <remarks>
    /// This constructor sets up the layout and components of the menu, including:
    /// - Initializing the menu dimensions and bounding rectangle.
    /// - Creating search boxes for editing titles and selecting items.
    /// - Adding action bars and navigation buttons (left-right and up-down).
    /// - Adding cancel buttons for each plan slot.
    /// - Initializing choose buttons for selecting various attributes such as NPC, location, and actions.
    /// - Setting up additional buttons for the time selection page.
    /// </remarks>
    public WypMenu(PlanData planData, ModData modData, ModConfig config)
    {
        initialize(xPos, yPos, uiWidth, uiHeight);
        this.planData = planData;
        this.modData = modData;
        this.config = config;
        menuBound = new Rectangle(xPos, yPos + actionBarHeight, uiWidth, uiHeight - actionBarHeight);
        titleBox = new SearchBox(null, null, Game1.smallFont, Color.Black, Translations.GetStr("EditPage.EditTitle"));
        itemBox = new SearchBox(null, null, Game1.smallFont, Color.Black, Translations.GetStr("ChooseItem.EditItem"));
        actionBar = new ActionBar(xPos, yPos, uiWidth, actionBarHeight);
        leftrightButton = new PageButton(xPos - 32, yPos, uiWidth, uiHeight, uiWidth + Game1.tileSize);
        updownButton = new PageButton(xPos, yPos, uiWidth, uiHeight, uiWidth + Game1.tileSize);
        cancelButtons = new List<CancelButton>();
        chooseButtons = new List<ChooseButton>
        {
            new(xPos, yPos + actionBarHeight + 30 + Game1.tileSize * 1, uiWidth, Game1.tileSize),
            new(xPos, yPos + actionBarHeight + 30 + Game1.tileSize * 2, uiWidth, Game1.tileSize),
            new(xPos, yPos + actionBarHeight + 30 + Game1.tileSize * 3, uiWidth, Game1.tileSize),
            new(xPos, yPos + actionBarHeight + 30 + Game1.tileSize * 4, uiWidth, Game1.tileSize),
            new(xPos, yPos + actionBarHeight + 30 + Game1.tileSize * 5, uiWidth, Game1.tileSize)
        };
        for (int i = 0; i < 5; i++)
        {
            cancelButtons.Add(new CancelButton(xPos + uiWidth - 52, yPos + actionBarHeight + barHeight * i + 30, 36, 36));
        }
        selectRepeatButton = new SelectButton(0, 0, 0, 0, "", new List<string>());
        InitTimePageButtons();

        reminder = new Reminder(this.planData, this.modData);
        reminder.InitReminder();
    }

    /// <summary>
    /// Draws the UI elements of the custom menu depending on the current page index.
    /// </summary>
    /// <param name="b">The SpriteBatch used to draw textures.</param>
    /// <remarks>
    /// This method handles the rendering of various menu pages, including the main page, edit page, and choose page.
    /// - On the main page, it displays plans and navigation buttons.
    /// - On the edit page, it allows the user to input details for a new plan.
    /// - On the choose page, it lets the user select items, locations, or characters etc. for a plan.
    /// The method also applies a screen fade effect in the background and manages hover and mouse interactions.
    /// </remarks>
    public override void draw(SpriteBatch b)
    {
        //draw screen fade
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

        // draw menu main page
        
        //draw page button
        if (pageIndex == (int)PageEnum.mainPage)
        {
            Game1.DrawBox(xPos, yPos, uiWidth, uiHeight);
            SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("MainPage", "Title"), xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            actionBar.Draw(b, pageIndex);
            if (planPageIndex > 0)
                leftrightButton.DrawLeftButton(b);
            if (planPageIndex < planData.plan.Count / 5 && planPageIndex != (config.MaxPlan - 1) / 5)
                leftrightButton.DrawRightButton(b);                
            
            for (int i = 0; i < 5; i++)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), xPos, yPos + actionBarHeight + barHeight * i, uiWidth, barHeight, Color.White, 4f, false);
                cancelButtons[i].Draw(b);
                if (planPageIndex * 5 + i < planData.plan.Count)
                {
                    DrawLimitWidthString.Draw(b, planData.plan[planPageIndex * 5 + i].title, Game1.smallFont, xPos + 24, yPos + actionBarHeight + barHeight * i + 16, uiWidth - 90, Color.Black);
                    DrawLimitWidthString.Draw(b, TextUtils.GetOrganizedText(planData.plan[planPageIndex * 5 + i]), Game1.smallFont, xPos + 24, yPos + actionBarHeight + barHeight * i + 48, uiWidth - 90, Color.Black);
                }
            }
        }
        else if (pageIndex == (int)PageEnum.editPage)
        {
            Game1.DrawBox(xPos, yPos, uiWidth, uiHeight);
            SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("EditPage", "Title"), xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            actionBar.Draw(b, pageIndex);
            titleBox.Bounds = new Rectangle(xPos, yPos + actionBarHeight, uiWidth, Game1.tileSize);
            if (newPlan.title != "")
            {
                titleBox.SetHoverText(newPlan.title);
            }
            titleBox.Draw(b);
            chooseButtons[0].Draw(b,Translations.GetStr("EditPage.NPC"), Translations.GetNPCName(newPlan.npc));
            chooseButtons[1].Draw(b, Translations.GetStr("EditPage.Location"),  Translations.GetStr("ChooseLocation", newPlan.location));
            chooseButtons[2].Draw(b, Translations.GetStr("EditPage.Action"), Translations.GetStr("ChooseAction", newPlan.action));
            chooseButtons[3].Draw(b, Translations.GetStr("EditPage.Time"), newPlan.time == "" ? "" : Translations.GetStr("EditPage.Time.Description"));
            chooseButtons[4].Draw(b, Translations.GetStr("EditPage.Item"), newPlan.item); 
            reminder.Draw(b, xPos - uiWidth - 16, yPos, uiWidth, uiHeight, planPageIndex * 5 + planIndexOnPage);
        }
        else if (pageIndex == (int)PageEnum.choosePage)
        {
            if (choosePageIndex == (int)ChoosePageEnum.characterPage)
            {   
                chooseBoundList = DrawChoosePage.Draw(b, xPos, yPos, uiWidth, uiHeight, modData.npcData);
            }
            else if (choosePageIndex == (int)ChoosePageEnum.locationPage)
            {
                Game1.DrawBox(xPos, yPos, uiWidth, uiHeight);
                SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("ChoosePage", "Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                actionBar.Draw(b, pageIndex);
                chooseBoundList = DrawChoosePage.Draw(b, xPos, yPos, uiWidth, uiHeight, locationDisplayIndex, newPlan.npc == "" ? modData.locationData : pririotyLocation);
                updownButton = new PageButton(chooseBoundList[0].X + uiWidth + 16, chooseBoundList[0].Y, uiWidth, Game1.tileSize * 6, Game1.tileSize * 6);
                updownButton.DrawUpButton(b);
                updownButton.DrawDownButton(b);
            }
            else if (choosePageIndex == (int)ChoosePageEnum.actionPage)
            {
                Game1.DrawBox(xPos, yPos, uiWidth, uiHeight);
                SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("ChoosePage", "Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                actionBar.Draw(b, pageIndex);
                chooseBoundDict = DrawChoosePage.Draw(b, xPos, yPos, uiWidth, uiHeight, newPlan.npc != "");
            }
            else if(choosePageIndex == (int)ChoosePageEnum.timePage)
            {
                int x = xPos - Game1.tileSize / 2;
                int y = yPos;
                int width = uiWidth + Game1.tileSize;
                int height = uiHeight + Game1.tileSize / 2;
                SpriteFont font = Game1.smallFont;
                int gap = Game1.tileSize * 7 / 8;
                int[] widths = {32, 64, 32, 96};
                Game1.DrawBox(x, y, width, height);
                SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("ChoosePage", "Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                actionBar.Draw(b, pageIndex);
                b.DrawString(font, Translations.GetStr("ChooseDate.StartDate"), new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year")).X) / 2, y + actionBarHeight), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.EndDate"), new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year")).X) / 2, y + actionBarHeight + Game1.tileSize * 5 / 2), Color.Black);
                
                //transform season-day to week-day
                string week = Translations.GetStr("ChooseDate.WhichWeek", new{ week = (arrowDateButtons[2].Index / 7 + 1).ToString() });
                string day = TimeList.Days[arrowDateButtons[2].Index % 7];
                b.DrawString(font, week + " " + day, new Vector2(x + width / 2, y + actionBarHeight), Color.Black);
                week = Translations.GetStr("ChooseDate.WhichWeek", new { week = (arrowDateButtons[6].Index / 7 + 1).ToString() });
                day = TimeList.Days[arrowDateButtons[6].Index % 7];
                b.DrawString(font, week + " " + day, new Vector2(x + width / 2, y + actionBarHeight + Game1.tileSize * 5 / 2), Color.Black);
                
                //draw time selection text
                y += actionBarHeight + Game1.tileSize / 2;
                b.DrawString(font, Translations.GetStr("ChooseDate.Year") + ":", new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Season") + ":", new Vector2(x + widths[0] + gap + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Season") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Day") + ":", new Vector2(x + widths[0] + widths[1] + gap * 2 + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Day") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Hour") + ":", new Vector2(x + widths[0] + widths[1] + widths[2] + gap * 3 + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Hour") + ":").X) * 3 / 4, y), Color.Black);
                y += Game1.tileSize * 5 / 2;
                b.DrawString(font, Translations.GetStr("ChooseDate.Year") + ":", new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Season") + ":", new Vector2(x + widths[0] + gap + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Season") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Day") + ":", new Vector2(x + widths[0] + widths[1] + gap * 2 + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Day") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Hour") + ":", new Vector2(x + widths[0] + widths[1] + widths[2] + gap * 3 + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Hour") + ":").X) * 3 / 4, y), Color.Black);
                widths[0] = 56; widths[1] = 56; widths[2] = 56; widths[3] = 56;
            
                foreach (SelectByArrowButton button in arrowDateButtons)
                {
                    button.Draw(b);
                }
                foreach (SelectButton button in selectSpecialDayButtons)
                {
                    button.Draw(b);
                }
                selectRepeatButton.Draw(b);
            }
            else if (choosePageIndex == (int)ChoosePageEnum.itemPage)
            {
                Game1.DrawBox(xPos, yPos, uiWidth, uiHeight);
                SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("ChoosePage", "Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                actionBar.Draw(b, pageIndex);
                itemBox.Bounds = new Rectangle(xPos, yPos + actionBarHeight, uiWidth, Game1.tileSize);
                if (newPlan.item != "")
                {
                    itemBox.SetHoverText(newPlan.item);
                }
                itemBox.Draw(b);
                // draw four item descriptions
                b.DrawString(Game1.smallFont, Translations.GetStr("ChooseItem.Description1"), new Vector2(xPos + 16, yPos + actionBarHeight + 32 + Game1.tileSize), Color.Black);
                b.DrawString(Game1.smallFont, Translations.GetStr("ChooseItem.Description2"), new Vector2(xPos + 16, yPos + actionBarHeight + 32 + Game1.tileSize * 2), Color.Black);
                b.DrawString(Game1.smallFont, Translations.GetStr("ChooseItem.Description3"), new Vector2(xPos + 16, yPos + actionBarHeight + 32 + Game1.tileSize * 3), Color.Black);
                b.DrawString(Game1.smallFont, Translations.GetStr("ChooseItem.Description4"), new Vector2(xPos + 16, yPos + actionBarHeight + 32 + Game1.tileSize * 4), Color.Black);
            }   
        }
        DrawHoverAction(b);
        drawMouse(b);
    }

    /// <summary>
    /// Handles the player's left-click interaction within the game menu.
    /// Performs different actions based on the current page index and the location of the click.
    /// </summary>
    /// <param name="x">The x-coordinate of the click position.</param>
    /// <param name="y">The y-coordinate of the click position.</param>
    /// <param name="playSound">Determines whether a sound should be played when the click is registered. Default is true.</param>
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (pageIndex == (int)PageEnum.mainPage)
        {
            if (menuBound.Contains(x, y))
            {
                planIndexOnPage = (y - yPos - actionBarHeight) / barHeight;
                if (CancelCheck(x, y))
                {
                    if (planPageIndex * 5 + planIndexOnPage < planData.plan.Count)
                    {
                        planData.plan.RemoveAt(planPageIndex * 5 + planIndexOnPage);
                        planIndexOnPage = 0;
                    }
                }
                else
                {
                    newPlan = new Plan();
                    if (planPageIndex * 5 + planIndexOnPage < planData.plan.Count)
                    {
                        newPlan.npc = planData.plan[planPageIndex * 5 + planIndexOnPage].npc;
                        newPlan.location = planData.plan[planPageIndex * 5 + planIndexOnPage].location;
                        newPlan.action = planData.plan[planPageIndex * 5 + planIndexOnPage].action;
                        newPlan.time = planData.plan[planPageIndex * 5 + planIndexOnPage].time;
                        newPlan.title = planData.plan[planPageIndex * 5 + planIndexOnPage].title;
                        newPlan.repeat = planData.plan[planPageIndex * 5 + planIndexOnPage].repeat;
                        newPlan.special = planData.plan[planPageIndex * 5 + planIndexOnPage].special;
                        newPlan.item = planData.plan[planPageIndex * 5 + planIndexOnPage].item;
                    }
                    reminder.UpdateReminder();
                    pageIndex = (int)PageEnum.editPage;
                }
            }
            else if (actionBar.OKBound.Contains(x, y))
            {
                
                exitThisMenu();
            }
            else if (actionBar.BackBound.Contains(x, y))
            {
                exitThisMenu();
            }
            if (leftrightButton.LeftBound.Contains(x, y) && planPageIndex > 0)
            {
                planPageIndex--;
            }
            if (leftrightButton.RightBound.Contains(x, y) && planPageIndex < planData.plan.Count / 5 && planPageIndex != (config.MaxPlan - 1) / 5)
            {
                planPageIndex++;
            }
        }
        else if (pageIndex == (int)PageEnum.editPage)
        {
            if (titleBox.Bounds.Contains(x, y))
            {
                titleBox.Click();
            }
            else if (actionBar.OKBound.Contains(x, y))
            {
                if (titleBox.Text != "")
                {
                    newPlan.title = titleBox.Text;
                }
                if (planPageIndex * 5 + planIndexOnPage >= planData.plan.Count)
                {
                    //only choosing date is invalid
                    if(newPlan.title != "" || newPlan.npc != "" || newPlan.location != "" || newPlan.action != "" || newPlan.item != "")
                        planData.plan.Add(newPlan);
                }
                else
                {
                    planData.plan[planPageIndex * 5 + planIndexOnPage] = newPlan;
                }
                titleBox.Release();
                pageIndex--;
            }
            else if (actionBar.BackBound.Contains(x, y))
            {
                titleBox.Release();
                pageIndex--;
            }
            else if ((choosePageIndex = GetChoosePageIndex(x, y)) != -1)
            {
                if (choosePageIndex == (int)ChoosePageEnum.locationPage)
                {
                    SetDefaultLocation();
                }
                if (choosePageIndex == (int)ChoosePageEnum.timePage)
                {
                    SetDefaultTime();
                }
                pageIndex++;
            }
        }
        else if (pageIndex == (int)PageEnum.choosePage)
        {
            List<Rectangle> chooseBound = chooseBoundList;
            if (choosePageIndex == (int)ChoosePageEnum.locationPage)
            {
                if (updownButton.UpBound.Contains(x, y) && locationDisplayIndex > 0)
                {
                    locationDisplayIndex--;
                }
                else if (updownButton.DownBound.Contains(x, y) && locationDisplayIndex + 6 < modData.locationData.Count)
                {
                    locationDisplayIndex++;
                }
                else if (chooseBound[0].Contains(x, y))
                {
                    int idx = (y - chooseBound[0].Y) / Game1.tileSize;
                    newPlan.location = pririotyLocation[locationDisplayIndex + idx];
                    locationDisplayIndex = 0;
                    pageIndex--;
                }
                else if (actionBar.OKBound.Contains(x, y) || actionBar.BackBound.Contains(x, y))
                {
                    pageIndex--;
                }
            }
            else if (choosePageIndex == (int)ChoosePageEnum.characterPage)
            {
                // not safe, but it works.Maybe other data structure can be used to store npc data
                List<string> keys = modData.npcData.Keys.ToList();
                int i;
                for (i = 0; i < chooseBoundList.Count; i++)
                {
                    if (chooseBoundList[i].Contains(x, y))
                    {
                        if (newPlan.npc != "" && newPlan.npc == keys[i])
                        {
                            newPlan.npc = "";
                        }
                        else
                        {
                            newPlan.npc = keys[i];
                        }
                        pageIndex--;
                        break;
                    }
                }
                if (i == chooseBoundList.Count)
                {
                    pageIndex--;
                }
            }
            else if (choosePageIndex == (int)ChoosePageEnum.actionPage)
            {
                if (actionBar.OKBound.Contains(x, y) || actionBar.BackBound.Contains(x, y))
                {
                    pageIndex--;
                }
                else
                {
                    foreach (KeyValuePair<string, Rectangle> pair in chooseBoundDict)
                    {
                        if (pair.Value.Contains(x, y))
                        {
                            newPlan.action = pair.Key;
                            pageIndex--;
                        }
                    }
                }
            }
            else if (choosePageIndex == (int)ChoosePageEnum.timePage)
            {
                if (actionBar.BackBound.Contains(x, y))
                {
                    pageIndex--;
                }
                else if (actionBar.OKBound.Contains(x, y))
                {
                    newPlan.time = Index2TimeString(arrowDateButtons);
                    newPlan.repeat = EncodeRepeatButton(selectRepeatButton);
                    newPlan.special = EncodeSpecialDateButton(selectSpecialDayButtons, 3);
                    pageIndex--;
                }
                else
                {
                    DateButtonCheck(x, y);
                }
            }
            else if (choosePageIndex == (int)ChoosePageEnum.itemPage)
            {
                if (itemBox.Bounds.Contains(x, y))
                {
                    itemBox.Click();
                }
                else if (actionBar.OKBound.Contains(x, y))
                {
                    newPlan.item = itemBox.Text;
                    itemBox.Release();
                    pageIndex--;
                }
                else if (actionBar.BackBound.Contains(x, y))
                {
                    itemBox.Release();
                    pageIndex--;
                }
            }
        }
    }
    
    private void SetDefaultLocation()
    {
        pririotyLocation = new();
        locationDisplayIndex = 0;
        if (newPlan.npc != "")
        {
            foreach (string location in modData.locationData)
            {
                if (modData.npcData[newPlan.npc][0].Contains(location))
                    pririotyLocation.Add(location);
            }
            foreach (string location in modData.locationData)
            {
                if (!modData.npcData[newPlan.npc][0].Contains(location))
                    pririotyLocation.Add(location);
            }
        }
        else
        {
            foreach (string location in modData.locationData)
            {
                pririotyLocation.Add(location);
            }
        }
    }

    public override void performHoverAction(int x, int y)
    {
        //base.performHoverAction(x, y);
        hoverFlags.SetDefaultFlags();
        if (pageIndex == (int)PageEnum.mainPage)
        {
            if (menuBound.Contains(x, y))
            {
                hoverFlags.MainPage_HoverPlan = true;
                planIndexOnPage = (y - yPos - actionBarHeight) / barHeight;
                hoverPlan = planPageIndex * 5 + planIndexOnPage < planData.plan.Count ? planData.plan[planPageIndex * 5 + planIndexOnPage] : new();
            }
            else
            {
                hoverFlags.MainPage_HoverPlan = false;
                hoverPlan = new();
            }
        }
        else if (pageIndex == (int)PageEnum.editPage)
        {
            if (chooseButtons[3].TextBound.Contains(x, y))
            {
                hoverFlags.TimePage_HoverTime = true;
            }
            if (actionBar.OKBound.Contains(x, y))
            {
                hoverFlags.EditPage_HoverOK = true;
            }
            else if (actionBar.BackBound.Contains(x, y))
            {
                hoverFlags.EditPage_HoverBack = true;
            }
        }
        else if (pageIndex == (int)PageEnum.choosePage)
        {
            if (choosePageIndex == (int)ChoosePageEnum.locationPage)
            {

            }
            else if (choosePageIndex == (int)ChoosePageEnum.characterPage)
            {

            }
            else if (choosePageIndex == (int)ChoosePageEnum.actionPage)
            {

            }
            else if (choosePageIndex == (int)ChoosePageEnum.timePage)
            {

            }
        }
    }
    private void DrawHoverAction(SpriteBatch b)
    {
        if (pageIndex == (int)PageEnum.mainPage)
        {
            if (hoverFlags.MainPage_HoverPlan)
            {
                string? tmp = TimeString2DisplayText(hoverPlan.time);
                string organizedText = TextUtils.GetOrganizedText(hoverPlan); 
                if (tmp is not null)
                {
                    drawToolTip(b, organizedText, tmp, null);
                }
                else
                {
                    drawToolTip(b, organizedText, "" , null);
                }
            }
        }
        else if (pageIndex == (int)PageEnum.editPage)
        {
            if (hoverFlags.TimePage_HoverTime)
            {
                string? tmp = TimeString2DisplayText(newPlan.time);
                if (tmp is not null)
                {
                    drawToolTip(b, tmp, "", null);
                }
            }
            if (hoverFlags.EditPage_HoverOK)
            {
                drawToolTip(b, Translations.GetStr("EditPage.OK"), "", null);
            }
            if (hoverFlags.EditPage_HoverBack)
            {
                drawToolTip(b, Translations.GetStr("EditPage.Back"), "", null);
            }
        }
        else if (pageIndex == (int)PageEnum.choosePage)
        {
            if (choosePageIndex == (int)ChoosePageEnum.locationPage)
            {

            }
            else if (choosePageIndex == (int)ChoosePageEnum.characterPage)
            {

            }
            else if (choosePageIndex == (int)ChoosePageEnum.actionPage)
            {

            }
            else if (choosePageIndex == (int)ChoosePageEnum.timePage)
            {

            }
        }
    }
    private void DateButtonCheck(int x, int y)
    {
        int interval = arrowDateButtons.Count / 2;
        List<SDate>? dateList = DateParse(Index2TimeString(arrowDateButtons), out int[] time);
        for (int i = 0; i < interval; i++)
        {
            SelectByArrowButton button = arrowDateButtons[i];
            if (button.PageButtonBound[0].Contains(x, y))
            {
                button.DecreasePage();
            }
            else if (button.PageButtonBound[1].Contains(x, y))
            {
                if (dateList is not null && i != interval - 1)
                {
                    if (dateList[0] >= dateList[1])
                    {
                        arrowDateButtons[i + interval].IncreasePage();
                    }
                }
                button.IncreasePage();
            }
        }
        for (int i = interval; i < arrowDateButtons.Count; i++)
        {
            SelectByArrowButton button = arrowDateButtons[i];
            if (button.PageButtonBound[0].Contains(x, y))
            {
                if (dateList is not null && i != interval - 1)
                {
                    if (dateList[0] >= dateList[1])
                    {
                        arrowDateButtons[i - interval].DecreasePage();
                    }
                }
                button.DecreasePage();
            }
            else if (button.PageButtonBound[1].Contains(x, y))
            {
                button.IncreasePage();
            }
        }
        for (int i = 0; i < selectSpecialDayButtons.Count; i++)
        {
            SelectButton button = selectSpecialDayButtons[i];
            button.ToggleOption(button.GetClickedOptionIndex(x, y));
        }
        selectRepeatButton.ToggleOption(selectRepeatButton.GetClickedOptionIndex(x, y));
    }
    //checkpoint
    private void InitTimePageButtons()
    {
        int x = xPos - Game1.tileSize / 2;
        int y = yPos + actionBarHeight + Game1.tileSize;
        int gap = Game1.tileSize * 7 / 8;
        int[] widths = {32, 64, 32, 72};
        //start date
        arrowDateButtons.Add(new SelectByArrowButton(x + gap, y, widths[0], 32, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + gap * 2, y, widths[1], 32, TimeList.Seasons, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + widths[1] + gap * 3, y, widths[2], 32, MaxIndex: 27, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + widths[1] + widths[2] + gap * 4, y, widths[3], 32, TimeList.Hours, Leftright: false));
        //end date
        y += Game1.tileSize * 5 / 2;
        arrowDateButtons.Add(new SelectByArrowButton(x + gap, y, widths[0], 32, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + gap * 2, y, widths[1], 32, TimeList.Seasons, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + widths[1] + gap * 3, y, widths[2], 32, MaxIndex: 27, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + widths[1] + widths[2] + gap * 4, y, widths[3], 32, TimeList.Hours, Leftright: false));
        
        //repeat and special day
        y += Game1.tileSize;
        x += 16;
        selectRepeatButton = new(x, y + Game1.tileSize / 2, 32, 32, Translations.GetStr("ChooseDate.Repeat") + ":", new List<string> { Translations.GetStr("ChooseDate.Year"), Translations.GetStr("ChooseDate.Season"), Translations.GetStr("ChooseDate.Week"), Translations.GetStr("ChooseDate.Day")});
        selectSpecialDayButtons.Add(new SelectButton(x, y + Game1.tileSize + Game1.tileSize / 8, 32, 32, Translations.GetStr("ChooseDate.Weather") + ":", new List<string> { Translations.GetStr("ChooseDate.Weather.Sunny"), Translations.GetStr("ChooseDate.Weather.Rainy") }));
        selectSpecialDayButtons.Add(new SelectButton(x, y + Game1.tileSize * 3 / 2 + Game1.tileSize / 8, 32, 32, Translations.GetStr("ChooseDate.Luck") + ":", new List<string> {Translations.GetStr("ChooseDate.Luck.Lucky"), Translations.GetStr("ChooseDate.Luck.Unlucky")}));
    }
    private void SetDefaultTime()
    {
        //if time is not set, set current time as default
        if (newPlan.time == "")
        { 
            arrowDateButtons[0].SetDefaultIndex(SDate.Now().Year - 1);
            arrowDateButtons[1].SetDefaultIndex(SDate.Now().SeasonIndex);
            arrowDateButtons[2].SetDefaultIndex(SDate.Now().Day - 1);
            arrowDateButtons[3].Release();
            arrowDateButtons[4].SetDefaultIndex(SDate.Now().Year - 1);
            arrowDateButtons[5].SetDefaultIndex(SDate.Now().SeasonIndex);
            arrowDateButtons[6].SetDefaultIndex(SDate.Now().Day - 1);
            arrowDateButtons[7].Release();
        }
        else
        {
            List<string[]> startendTime = DeadlineSplit(newPlan.time);
            for (int i = 0; i < 8; i++)
            {
                if (i < 4)
                {
                    arrowDateButtons[i].SetDefaultIndex(int.Parse(startendTime[0][i]));
                }
                else
                {
                    arrowDateButtons[i].SetDefaultIndex(int.Parse(startendTime[1][i - 4]));
                }
            }
        }
        DecodeRepeatButton(selectRepeatButton, newPlan.repeat);
        DecodeSpecialDateButton(newPlan.special, selectSpecialDayButtons, 3);
    }
    private void CheckEnableRepeat()
    {
        int startYear = int.Parse(arrowDateButtons[0].GetSelectedValue());
        int startSeason = int.Parse(arrowDateButtons[1].GetSelectedValue());
        int startTmp = int.Parse(arrowDateButtons[2].GetSelectedValue());
        int startWeek = startTmp / 7 + 1;
        int startDay = startTmp % 7 + 1;

        int endYear = int.Parse(arrowDateButtons[4].GetSelectedValue());
        int endSeason = int.Parse(arrowDateButtons[5].GetSelectedValue());
        int endTmp = int.Parse(arrowDateButtons[6].GetSelectedValue());
        int endWeek = endTmp / 7 + 1;
        int endDay = endTmp % 7 + 1;

        for (int i = 0; i < 4; i++)
        {
            arrowDateButtons[8 + i].SetIsRepeat(true);
        }
        if (startYear >= endYear)
        {
            arrowDateButtons[8].SetIsRepeat(false);
        }
        else
        {
            return;
        }
        if (startSeason >= endSeason)
        {
            arrowDateButtons[9].SetIsRepeat(false);
        }
        else
        {
            return;
        }
        if (startWeek >= endWeek)
        {
            arrowDateButtons[10].SetIsRepeat(false);
        }
        else
        {
            return;
        }
        if (startDay >= endDay)
        {
            arrowDateButtons[11].SetIsRepeat(false);
        }
        else
        {
            return;
        }
    }
    private int GetChoosePageIndex(int x, int y)
    {
        for (int i = 0; i < chooseButtons.Count; i++)
        {
            if (chooseButtons[i].EditBound.Contains(x, y))
            {
                return i;
            }
        }
        return -1;
    }
    private bool CancelCheck(int x, int y)
    {
        for (int i = 0; i < 5; i++)
        {
            if (cancelButtons[i].CancelBound.Contains(x, y))
            {
                return true;
            }
        }
        return false;
    }
    public override void receiveKeyPress(Keys key)
    {
        // deliberately avoid calling base, which may let another key close the menu
        if (key.Equals(Keys.Escape))
            exitThisMenu(); 
    }
}