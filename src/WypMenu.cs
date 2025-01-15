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
    private Reminder reminder;
    public readonly LoadData loaddata = new();
    public readonly int x_pos = (int)(Game1.viewport.Width * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - Game1.tileSize * 6 / 2;
    public readonly int y_pos = (int)(Game1.viewport.Height * Game1.options.zoomLevel * (1 / Game1.options.uiScale)) / 2 - Game1.tileSize * 6 / 2 - Game1.tileSize;
    public readonly int ui_width = Game1.tileSize * 6;
    public readonly int ui_height = Game1.tileSize / 2 + Game1.tileSize * 8;
    public readonly int action_bar_height = Game1.tileSize;
    public readonly int bar_height = Game1.tileSize + Game1.tileSize / 2;
    public Plan new_plan = new();
    Plan hover_plan = new();
    Rectangle menu_bound;
    List<Rectangle> choose_bound_list = new();
    Dictionary<string, Rectangle> choose_bound_dict = new();
    readonly SearchBox titleBox;
    readonly SearchBox item_box;
    readonly ActionBar action_bar;
    PageButton updown_button;
    readonly PageButton leftright_button;
    readonly List<CancelButton> cancel_buttons;
    readonly List<ChooseButton> choose_buttons;
    SelectButton selectRepeatButton;
    readonly List<SelectButton> selectSpecialDayButtons = new();
    readonly List<SelectByArrowButton> arrowDateButtons = new();
    readonly HoverFlags hover_flags = new();
    readonly List<string> time_labels = new()
    {
        Translations.GetStr("ChooseDate.Year"),
        Translations.GetStr("ChooseDate.Season"),
        Translations.GetStr("ChooseDate.Day"),
        Translations.GetStr("ChooseDate.Hour")
    };
    readonly List<string> repeat_labels = new()
    {
        Translations.GetStr("ChooseDate.Year"),
        Translations.GetStr("ChooseDate.Season"),
        Translations.GetStr("ChooseDate.Week"),
        Translations.GetStr("ChooseDate.Day")
    };
    // pageIndex: 0-mainPage, 1-editPage, 2-choosePage
    int pageIndex = (int)PageEnum.mainPage;
    // planPageIndex: the index of plan page
    int planPageIndex = 0;
    // planNum: the index of the selected plan on the current page
    int planIndexOnPage;
    // choosePageIndex: 0-npc, 1-location, 2-action, 3-time, 4-item
    int choosePageIndex;
    private List<string> pririoty_location = new();
    int location_display_index = 0;
    //bool is_canceling = false;

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
        initialize(x_pos, y_pos, ui_width, ui_height);
        this.planData = planData;
        this.modData = modData;
        this.config = config;
        menu_bound = new Rectangle(x_pos, y_pos + action_bar_height, ui_width, ui_height - action_bar_height);
        titleBox = new SearchBox(null, null, Game1.smallFont, Color.Black, Translations.GetStr("EditPage.EditTitle"));
        item_box = new SearchBox(null, null, Game1.smallFont, Color.Black, Translations.GetStr("ChooseItem.EditItem"));
        action_bar = new ActionBar(x_pos, y_pos, ui_width, action_bar_height);
        leftright_button = new PageButton(x_pos - 32, y_pos, ui_width, ui_height, ui_width + Game1.tileSize);
        updown_button = new PageButton(x_pos, y_pos, ui_width, ui_height, ui_width + Game1.tileSize);
        cancel_buttons = new List<CancelButton>();
        choose_buttons = new List<ChooseButton>
        {
            new(x_pos, y_pos + action_bar_height + 30 + Game1.tileSize * 1, ui_width, Game1.tileSize),
            new(x_pos, y_pos + action_bar_height + 30 + Game1.tileSize * 2, ui_width, Game1.tileSize),
            new(x_pos, y_pos + action_bar_height + 30 + Game1.tileSize * 3, ui_width, Game1.tileSize),
            new(x_pos, y_pos + action_bar_height + 30 + Game1.tileSize * 4, ui_width, Game1.tileSize),
            new(x_pos, y_pos + action_bar_height + 30 + Game1.tileSize * 5, ui_width, Game1.tileSize)
        };
        for (int i = 0; i < 5; i++)
        {
            cancel_buttons.Add(new CancelButton(x_pos + ui_width - 52, y_pos + action_bar_height + bar_height * i + 30, 36, 36));
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
            Game1.DrawBox(x_pos, y_pos, ui_width, ui_height);
            SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("MainPage", "Title"), xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            action_bar.Draw(b, pageIndex);
            if (planPageIndex > 0)
                leftright_button.DrawLeftButton(b);
            if (planPageIndex < planData.plan.Count / 5 && planPageIndex != (config.MaxPlan - 1) / 5)
                leftright_button.DrawRightButton(b);                
            
            for (int i = 0; i < 5; i++)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), x_pos, y_pos + action_bar_height + bar_height * i, ui_width, bar_height, Color.White, 4f, false);
                cancel_buttons[i].Draw(b);
                if (planPageIndex * 5 + i < planData.plan.Count)
                {
                    DrawLimitWidthString.Draw(b, planData.plan[planPageIndex * 5 + i].title, Game1.smallFont, x_pos + 24, y_pos + action_bar_height + bar_height * i + 16, ui_width - 90, Color.Black);
                    DrawLimitWidthString.Draw(b, TextUtils.GetOrganizedText(planData.plan[planPageIndex * 5 + i]), Game1.smallFont, x_pos + 24, y_pos + action_bar_height + bar_height * i + 48, ui_width - 90, Color.Black);
                }
            }
        }
        else if (pageIndex == (int)PageEnum.editPage)
        {
            Game1.DrawBox(x_pos, y_pos, ui_width, ui_height);
            SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("EditPage", "Title"), xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
            action_bar.Draw(b, pageIndex);
            titleBox.Bounds = new Rectangle(x_pos, y_pos + action_bar_height, ui_width, Game1.tileSize);
            if (new_plan.title != "")
            {
                titleBox.SetHoverText(new_plan.title);
            }
            titleBox.Draw(b);
            choose_buttons[0].Draw(b,Translations.GetStr("EditPage.NPC"), Translations.GetNPCName(new_plan.npc));
            choose_buttons[1].Draw(b, Translations.GetStr("EditPage.Location"),  Translations.GetStr("ChooseLocation", new_plan.location));
            choose_buttons[2].Draw(b, Translations.GetStr("EditPage.Action"), Translations.GetStr("ChooseAction", new_plan.action));
            choose_buttons[3].Draw(b, Translations.GetStr("EditPage.Time"), new_plan.time == "" ? "" : Translations.GetStr("EditPage.Time.Description"));
            choose_buttons[4].Draw(b, Translations.GetStr("EditPage.Item"), new_plan.item); 
            reminder.Draw(b, x_pos - ui_width - 16, y_pos, ui_width, ui_height, planPageIndex * 5 + planIndexOnPage);
        }
        else if (pageIndex == (int)PageEnum.choosePage)
        {
            if (choosePageIndex == (int)ChoosePageEnum.characterPage)
            {   
                choose_bound_list = DrawChoosePage.Draw(b, x_pos, y_pos, ui_width, ui_height, modData.npc_data);
            }
            else if (choosePageIndex == (int)ChoosePageEnum.locationPage)
            {
                Game1.DrawBox(x_pos, y_pos, ui_width, ui_height);
                SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("ChoosePage", "Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                action_bar.Draw(b, pageIndex);
                choose_bound_list = DrawChoosePage.Draw(b, x_pos, y_pos, ui_width, ui_height, location_display_index, new_plan.npc == "" ? modData.location_data : pririoty_location);
                updown_button = new PageButton(choose_bound_list[0].X + ui_width + 16, choose_bound_list[0].Y, ui_width, Game1.tileSize * 6, Game1.tileSize * 6);
                updown_button.DrawUpButton(b);
                updown_button.DrawDownButton(b);
            }
            else if (choosePageIndex == (int)ChoosePageEnum.actionPage)
            {
                Game1.DrawBox(x_pos, y_pos, ui_width, ui_height);
                SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("ChoosePage", "Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                action_bar.Draw(b, pageIndex);
                choose_bound_dict = DrawChoosePage.Draw(b, x_pos, y_pos, ui_width, ui_height, new_plan.npc != "");
            }
            else if(choosePageIndex == (int)ChoosePageEnum.timePage)
            {
                int x = x_pos - Game1.tileSize / 2;
                int y = y_pos;
                int width = ui_width + Game1.tileSize;
                int height = ui_height + Game1.tileSize / 2;
                SpriteFont font = Game1.smallFont;
                int gap = Game1.tileSize * 3 / 4;
                int[] widths = {32, 64, 32, 96};
                Game1.DrawBox(x, y, width, height);
                SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("ChoosePage", "Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                action_bar.Draw(b, pageIndex);
                b.DrawString(font, Translations.GetStr("ChooseDate.StartDate"), new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year")).X) * 3 / 4, y + action_bar_height), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.EndDate"), new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year")).X) * 3 / 4, y + action_bar_height + Game1.tileSize * 2), Color.Black);
                //b.DrawString(font, Translations.GetStr("ChooseDate.Repeat"), new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year")).X) * 3 / 4, y + action_bar_height + Game1.tileSize * 4), Color.Black);
                
                y += action_bar_height + Game1.tileSize;
                b.DrawString(font, Translations.GetStr("ChooseDate.Year") + ":", new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Season") + ":", new Vector2(x + widths[0] + gap + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Season") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Day") + ":", new Vector2(x + widths[0] + widths[1] + gap * 2 + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Day") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Hour") + ":", new Vector2(x + widths[0] + widths[1] + widths[2] + gap * 3 + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Hour") + ":").X) * 3 / 4, y), Color.Black);
                y += Game1.tileSize * 2;
                b.DrawString(font, Translations.GetStr("ChooseDate.Year") + ":", new Vector2(x + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Year") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Season") + ":", new Vector2(x + widths[0] + gap + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Season") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Day") + ":", new Vector2(x + widths[0] + widths[1] + gap * 2 + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Day") + ":").X) * 3 / 4, y), Color.Black);
                b.DrawString(font, Translations.GetStr("ChooseDate.Hour") + ":", new Vector2(x + widths[0] + widths[1] + widths[2] + gap * 3 + (gap - font.MeasureString(Translations.GetStr("ChooseDate.Hour") + ":").X) * 3 / 4, y), Color.Black);
                y += Game1.tileSize * 2;
                widths[0] = 56; widths[1] = 56; widths[2] = 56; widths[3] = 56;
                x += 16;
                gap = Game1.tileSize;
            
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
                Game1.DrawBox(x_pos, y_pos, ui_width, ui_height);
                SpriteText.drawStringWithScrollCenteredAt(b, Translations.GetStr("ChoosePage", "Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                action_bar.Draw(b, pageIndex);
                item_box.Bounds = new Rectangle(x_pos, y_pos + action_bar_height, ui_width, Game1.tileSize);
                if (new_plan.item != "")
                {
                    item_box.SetHoverText(new_plan.item);
                }
                item_box.Draw(b);
                // draw four item descriptions
                b.DrawString(Game1.smallFont, Translations.GetStr("ChooseItem.Description1"), new Vector2(x_pos + 16, y_pos + action_bar_height + 32 + Game1.tileSize), Color.Black);
                b.DrawString(Game1.smallFont, Translations.GetStr("ChooseItem.Description2"), new Vector2(x_pos + 16, y_pos + action_bar_height + 32 + Game1.tileSize * 2), Color.Black);
                b.DrawString(Game1.smallFont, Translations.GetStr("ChooseItem.Description3"), new Vector2(x_pos + 16, y_pos + action_bar_height + 32 + Game1.tileSize * 3), Color.Black);
                b.DrawString(Game1.smallFont, Translations.GetStr("ChooseItem.Description4"), new Vector2(x_pos + 16, y_pos + action_bar_height + 32 + Game1.tileSize * 4), Color.Black);
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
            if (menu_bound.Contains(x, y))
            {
                planIndexOnPage = (y - y_pos - action_bar_height) / bar_height;
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
                    new_plan = new Plan();
                    if (planPageIndex * 5 + planIndexOnPage < planData.plan.Count)
                    {
                        new_plan.npc = planData.plan[planPageIndex * 5 + planIndexOnPage].npc;
                        new_plan.location = planData.plan[planPageIndex * 5 + planIndexOnPage].location;
                        new_plan.action = planData.plan[planPageIndex * 5 + planIndexOnPage].action;
                        new_plan.time = planData.plan[planPageIndex * 5 + planIndexOnPage].time;
                        new_plan.title = planData.plan[planPageIndex * 5 + planIndexOnPage].title;
                        new_plan.repeat = planData.plan[planPageIndex * 5 + planIndexOnPage].repeat;
                        new_plan.special = planData.plan[planPageIndex * 5 + planIndexOnPage].special;
                        new_plan.item = planData.plan[planPageIndex * 5 + planIndexOnPage].item;
                    }
                    reminder.UpdateReminder();
                    pageIndex = (int)PageEnum.editPage;
                }
            }
            else if (action_bar.OKBound.Contains(x, y))
            {
                
                exitThisMenu();
            }
            else if (action_bar.BackBound.Contains(x, y))
            {
                exitThisMenu();
            }
            if (leftright_button.LeftBound.Contains(x, y) && planPageIndex > 0)
            {
                planPageIndex--;
            }
            if (leftright_button.RightBound.Contains(x, y) && planPageIndex < planData.plan.Count / 5 && planPageIndex != (config.MaxPlan - 1) / 5)
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
            else if (action_bar.OKBound.Contains(x, y))
            {
                if (titleBox.Text != "")
                {
                    new_plan.title = titleBox.Text;
                }
                if (planPageIndex * 5 + planIndexOnPage >= planData.plan.Count)
                {
                    //only choosing date is invalid
                    if(new_plan.title != "" || new_plan.npc != "" || new_plan.location != "" || new_plan.action != "" || new_plan.item != "")
                        planData.plan.Add(new_plan);
                }
                else
                {
                    planData.plan[planPageIndex * 5 + planIndexOnPage] = new_plan;
                }
                titleBox.Release();
                pageIndex--;
            }
            else if (action_bar.BackBound.Contains(x, y))
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
            List<Rectangle> choose_bound = choose_bound_list;
            if (choosePageIndex == (int)ChoosePageEnum.locationPage)
            {
                if (updown_button.UpBound.Contains(x, y) && location_display_index > 0)
                {
                    location_display_index--;
                }
                else if (updown_button.DownBound.Contains(x, y) && location_display_index + 6 < modData.location_data.Count)
                {
                    location_display_index++;
                }
                else if (choose_bound[0].Contains(x, y))
                {
                    int idx = (y - choose_bound[0].Y) / Game1.tileSize;
                    new_plan.location = pririoty_location[location_display_index + idx];
                    location_display_index = 0;
                    pageIndex--;
                }
                else if (action_bar.OKBound.Contains(x, y) || action_bar.BackBound.Contains(x, y))
                {
                    pageIndex--;
                }
            }
            else if (choosePageIndex == (int)ChoosePageEnum.characterPage)
            {
                // not safe, but it works.Maybe other data structure can be used to store npc data
                List<string> keys = modData.npc_data.Keys.ToList();
                int i;
                for (i = 0; i < choose_bound_list.Count; i++)
                {
                    if (choose_bound_list[i].Contains(x, y))
                    {
                        if (new_plan.npc != "" && new_plan.npc == keys[i])
                        {
                            new_plan.npc = "";
                        }
                        else
                        {
                            new_plan.npc = keys[i];
                        }
                        pageIndex--;
                        break;
                    }
                }
                if (i == choose_bound_list.Count)
                {
                    pageIndex--;
                }
            }
            else if (choosePageIndex == (int)ChoosePageEnum.actionPage)
            {
                if (action_bar.OKBound.Contains(x, y) || action_bar.BackBound.Contains(x, y))
                {
                    pageIndex--;
                }
                else
                {
                    foreach (KeyValuePair<string, Rectangle> pair in choose_bound_dict)
                    {
                        if (pair.Value.Contains(x, y))
                        {
                            new_plan.action = pair.Key;
                            pageIndex--;
                        }
                    }
                }
            }
            else if (choosePageIndex == (int)ChoosePageEnum.timePage)
            {
                if (action_bar.BackBound.Contains(x, y))
                {
                    pageIndex--;
                }
                else if (action_bar.OKBound.Contains(x, y))
                {
                    new_plan.time = Index2TimeString(arrowDateButtons);
                    new_plan.repeat = EncodeRepeatButton(selectRepeatButton);
                    new_plan.special = EncodeSpecialDateButton(selectSpecialDayButtons, 3);
                    pageIndex--;
                }
                else
                {
                    DateButtonCheck(x, y);
                }
            }
            else if (choosePageIndex == (int)ChoosePageEnum.itemPage)
            {
                if (item_box.Bounds.Contains(x, y))
                {
                    item_box.Click();
                }
                else if (action_bar.OKBound.Contains(x, y))
                {
                    new_plan.item = item_box.Text;
                    item_box.Release();
                    pageIndex--;
                }
                else if (action_bar.BackBound.Contains(x, y))
                {
                    item_box.Release();
                    pageIndex--;
                }
            }
        }
    }
    
    private void SetDefaultLocation()
    {
        pririoty_location = new();
        location_display_index = 0;
        if (new_plan.npc != "")
        {
            foreach (string location in modData.location_data)
            {
                if (modData.npc_data[new_plan.npc][0].Contains(location))
                    pririoty_location.Add(location);
            }
            foreach (string location in modData.location_data)
            {
                if (!modData.npc_data[new_plan.npc][0].Contains(location))
                    pririoty_location.Add(location);
            }
        }
        else
        {
            foreach (string location in modData.location_data)
            {
                pririoty_location.Add(location);
            }
        }
    }

    public override void performHoverAction(int x, int y)
    {
        //base.performHoverAction(x, y);
        hover_flags.SetDefaultFlags();
        if (pageIndex == (int)PageEnum.mainPage)
        {
            if (menu_bound.Contains(x, y))
            {
                hover_flags.MainPage_HoverPlan = true;
                planIndexOnPage = (y - y_pos - action_bar_height) / bar_height;
                hover_plan = planPageIndex * 5 + planIndexOnPage < planData.plan.Count ? planData.plan[planPageIndex * 5 + planIndexOnPage] : new();
            }
            else
            {
                hover_flags.MainPage_HoverPlan = false;
                hover_plan = new();
            }
        }
        else if (pageIndex == (int)PageEnum.editPage)
        {
            if (choose_buttons[3].TextBound.Contains(x, y))
            {
                hover_flags.TimePage_HoverTime = true;
                //drawToolTip(b, Translations.GetStr("EditPage.Time.Description"), "", null);
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
            if (hover_flags.MainPage_HoverPlan)
            {
                string? tmp = TimeString2DisplayText(hover_plan.time);
                string organized_text = TextUtils.GetOrganizedText(hover_plan); 
                if (tmp is not null)
                {
                    drawToolTip(b, organized_text, tmp, null);
                }
                else
                {
                    drawToolTip(b, organized_text, "" , null);
                }
            }
        }
        else if (pageIndex == (int)PageEnum.editPage)
        {
            if (hover_flags.TimePage_HoverTime)
            {
                string? tmp = TimeString2DisplayText(new_plan.time);
                if (tmp is not null)
                {
                    drawToolTip(b, tmp, "", null);
                }
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
        for (int i = 0; i < arrowDateButtons.Count; i++)
        {
            SelectByArrowButton button = arrowDateButtons[i];
            if (button.PageButtonBound[0].Contains(x, y))
            {
                button.DecreasePage();
                //CheckEnableRepeat();
            }
            else if (button.PageButtonBound[1].Contains(x, y))
            {
                button.IncreasePage();
                //CheckEnableRepeat();
            }
        }
        for (int i = 0; i < selectSpecialDayButtons.Count; i++)
        {
            SelectButton button = selectSpecialDayButtons[i];
            button.ToggleOption(button.GetClickedOptionIndex(x, y));
        }
        selectRepeatButton.ToggleOption(selectRepeatButton.GetClickedOptionIndex(x, y));
    }

    private void InitTimePageButtons()
    {   
        int x = x_pos - Game1.tileSize / 2;
        int y = y_pos + action_bar_height + Game1.tileSize;
        int gap = Game1.tileSize * 3 / 4;
        int[] widths = {32, 64, 32, 96};
        //start date
        arrowDateButtons.Add(new SelectByArrowButton(x + gap, y, widths[0], 32, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + gap * 2, y, widths[1], 32, TimeList.Seasons, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + widths[1] + gap * 3, y, widths[2], 32, MaxIndex: 27, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + widths[1] + widths[2] + gap * 4, y, widths[3], 32, TimeList.Hours, Leftright: false));
        //end date
        y += Game1.tileSize * 2;
        arrowDateButtons.Add(new SelectByArrowButton(x + gap, y, widths[0], 32, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + gap * 2, y, widths[1], 32, TimeList.Seasons, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + widths[1] + gap * 3, y, widths[2], 32, MaxIndex: 27, Leftright: false));
        arrowDateButtons.Add(new SelectByArrowButton(x + widths[0] + widths[1] + widths[2] + gap * 4, y, widths[3], 32, TimeList.Hours, Leftright: false));
        //repeat
        y += Game1.tileSize;
        widths[0] = 56; widths[1] = 56; widths[2] = 56; widths[3] = 56;
        x += 16;

        //delete repeat arrow button to add select button
        selectRepeatButton = new(x, y, 32, 32, Translations.GetStr("ChooseDate.Repeat"), new List<string> { Translations.GetStr("ChooseDate.Year"), Translations.GetStr("ChooseDate.Season"), Translations.GetStr("ChooseDate.Week"), Translations.GetStr("ChooseDate.Day")});
        selectSpecialDayButtons.Add(new SelectButton(x, y + Game1.tileSize + Game1.tileSize / 8, 32, 32, Translations.GetStr("ChooseDate.Weather") + ":", new List<string> { Translations.GetStr("ChooseDate.Weather.Sunny"), Translations.GetStr("ChooseDate.Weather.Rainy") }));
        selectSpecialDayButtons.Add(new SelectButton(x, y + Game1.tileSize * 3 / 2 + Game1.tileSize / 8, 32, 32, Translations.GetStr("ChooseDate.Luck") + ":", new List<string> {Translations.GetStr("ChooseDate.Luck.Lucky"), Translations.GetStr("ChooseDate.Luck.Unlucky")}));
    }
    private void SetDefaultTime()
    {
        //if time is not set, set current time as default
        if (new_plan.time == "")
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
            List<string[]> startend_time = DeadlineSplit(new_plan.time);
            for (int i = 0; i < 8; i++)
            {
                if (i < 4)
                {
                    arrowDateButtons[i].SetDefaultIndex(int.Parse(startend_time[0][i]));
                }
                else
                {
                    arrowDateButtons[i].SetDefaultIndex(int.Parse(startend_time[1][i - 4]));
                }
            }
        }
        DecodeRepeatButton(selectRepeatButton, new_plan.repeat);
        //ModEntry.Monitor1.Log($"special state = {new_plan.special}", LogLevel.Debug);
        DecodeSpecialDateButton(new_plan.special, selectSpecialDayButtons, 3);
    }
    private void CheckEnableRepeat()
    {
        int start_year = int.Parse(arrowDateButtons[0].GetSelectedValue());
        int start_season = int.Parse(arrowDateButtons[1].GetSelectedValue());
        int start_tmp = int.Parse(arrowDateButtons[2].GetSelectedValue());
        int start_week = start_tmp / 7 + 1;
        int start_day = start_tmp % 7 + 1;

        int end_year = int.Parse(arrowDateButtons[4].GetSelectedValue());
        int end_season = int.Parse(arrowDateButtons[5].GetSelectedValue());
        int end_tmp = int.Parse(arrowDateButtons[6].GetSelectedValue());
        int end_week = end_tmp / 7 + 1;
        int end_day = end_tmp % 7 + 1;

        for (int i = 0; i < 4; i++)
        {
            arrowDateButtons[8 + i].SetIsRepeat(true);
        }
        if (start_year >= end_year)
        {
            arrowDateButtons[8].SetIsRepeat(false);
        }
        else
        {
            return;
        }
        if (start_season >= end_season)
        {
            arrowDateButtons[9].SetIsRepeat(false);
        }
        else
        {
            return;
        }
        if (start_week >= end_week)
        {
            arrowDateButtons[10].SetIsRepeat(false);
        }
        else
        {
            return;
        }
        if (start_day >= end_day)
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
        for (int i = 0; i < choose_buttons.Count; i++)
        {
            if (choose_buttons[i].EditBound.Contains(x, y))
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
            if (cancel_buttons[i].CancelBound.Contains(x, y))
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