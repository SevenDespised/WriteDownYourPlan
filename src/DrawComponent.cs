using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using static WritedownYourPlan.src.MenuEnum;

namespace WritedownYourPlan.src;
public class SearchBox : TextBox
{
    private Rectangle BoundsImpl;
    private bool DisplayHoverText = true;
    private string hoverText = "";
    private readonly string defaultHoverText = "";
    public SearchBox(Texture2D? textBoxTexture, Texture2D? caretTexture, SpriteFont font, Color textColor, string hoverText = "") : base(textBoxTexture, caretTexture, font, textColor)
    {
        this.hoverText = " " + hoverText;
        defaultHoverText = " " + hoverText;
    }

    public Rectangle Bounds
        {
            get => this.BoundsImpl;
            set
            {
                this.BoundsImpl = value;
                base.X = value.X;
                base.Y = value.Y;
                base.Width = value.Width;
                base.Height = value.Height;
            }
        }
    public override void Draw(SpriteBatch b, bool withShadow = false)
    {
        bool flag = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
        string text = Text;
        Vector2 vector = _font.MeasureString(text);

        if (_textBoxTexture != null)
        {
            b.Draw(_textBoxTexture, new Rectangle(X, Y, Width, Height), new Rectangle(0, 0, Width, Height), Color.White);
        }
        else
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), X, Y, Width, Height, Color.White, 4f, false);
        }
        while (vector.X > (float)Width)
        {
            text = text.Substring(1);
            vector = _font.MeasureString(text);
        }
        if (flag && Selected)
        {
            b.Draw(Game1.staminaRect, new Rectangle(X + 16 + (int)vector.X + 2, Y + 16, 4, 32), _textColor);
        }
        if (DisplayHoverText)
        {
            b.DrawString(_font, hoverText, new Vector2(X + 16, Y + 16), _textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
        }
        else
        {
            b.DrawString(_font, text, new Vector2(X + 16, Y + 16), _textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
        }
    }
    public void SetHoverText(string hoverText)
    {
        this.hoverText = " " + hoverText;
    }
    public void Release()
    {
        Selected = false;
        DisplayHoverText = true;
        hoverText = defaultHoverText;
        Text = "";
    }
    public void Click()
    {
        Selected = true;
        DisplayHoverText = false;
    }
}

public class ActionBar
{
    int x;
    int y;
    int width;
    int height;
    public Rectangle OKBound{ get;set; }
    public Rectangle BackBound{ get;set; }
    public Rectangle RightBound{ get;set; }
    public Rectangle LeftBound{ get;set; }
    int buttonSide;
    public ActionBar(int X, int Y, int W, int H)
    {
        x = X;
        y = Y;
        width = W;
        height = H;
        buttonSide = height / (Game1.tileSize / 2) * Game1.tileSize / 2;
        OKBound = new Rectangle(x + width - Game1.tileSize, y + (height - buttonSide) / 2, buttonSide, buttonSide);
        BackBound = new Rectangle(x, y + (height - buttonSide) / 2, buttonSide, buttonSide);
        RightBound = new Rectangle(x + width + Game1.tileSize / 2, y + Game1.tileSize / 2 + Game1.tileSize * 8 - 64, 48, 64);
        LeftBound = new Rectangle(x - Game1.tileSize / 2, y + Game1.tileSize / 2 + Game1.tileSize * 8 - 64, 48, 64);
    }
    public void Draw(SpriteBatch b, int page)
    {
        DrawOKButton(b);
        if (page == (int)PageEnum.mainPage)
        {
            DrawShutButton(b);
        }
        else
        {
            DrawBackButton(b);
        }
    }
    public void DrawOKButton(SpriteBatch b)
    {
        Rectangle okbutton;
        okbutton = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46);
        b.Draw(Game1.mouseCursors, new Vector2(OKBound.X, OKBound.Y), okbutton, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
    }
    public void DrawBackButton(SpriteBatch b)
    {
        Rectangle backbutton;
        backbutton = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44);
        b.Draw(Game1.mouseCursors, new Vector2(BackBound.X, BackBound.Y), backbutton, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
    }

    public void DrawShutButton(SpriteBatch b)
    {
        Rectangle shutbutton;
        shutbutton = new Rectangle(338, 494, 11, 11);
        b.Draw(Game1.mouseCursors, new Vector2(BackBound.X, BackBound.Y), shutbutton, Color.White, 0f, Vector2.Zero, 5.5f, SpriteEffects.None, 0.9999999f);
    }
}

public class PageButton
{
    int x;
    int y;
    int width;
    int height;
    public Rectangle RightBound{ get;set; }
    public Rectangle LeftBound{ get;set; }
    public Rectangle UpBound{ get;set; }
    public Rectangle DownBound{ get;set; }
    public PageButton(int X, int Y, int W, int H, int button_gap)
    {
        x = X;
        y = Y;
        width = W;
        height = H;
        RightBound = new Rectangle(x + button_gap, y + Game1.tileSize / 2 + Game1.tileSize * 8 - 64, 48, 64);
        LeftBound = new Rectangle(x - 48, y + Game1.tileSize / 2 + Game1.tileSize * 8 - 64, 48, 64);
        UpBound = new Rectangle(x, y, 32, 24);
        DownBound = new Rectangle(x, y + button_gap - 24, 32, 24);
    }
    public void DrawLeftButton(SpriteBatch b)
    {
        Rectangle leftbutton;
        leftbutton = new Rectangle(480, 96, 24, 32);
        b.Draw(Game1.mouseCursors, new Vector2(LeftBound.X, LeftBound.Y), leftbutton, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.9999999f);
    }
    public void DrawRightButton(SpriteBatch b)
    {
        Rectangle rightbutton;
        rightbutton = new Rectangle(448, 96, 24, 32);
        b.Draw(Game1.mouseCursors, new Vector2(RightBound.X, RightBound.Y), rightbutton, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.9999999f);
    }

    public void DrawUpButton(SpriteBatch b)
    {
        Rectangle leftbutton;
        leftbutton = new Rectangle(480, 96, 24, 32);
        b.Draw(Game1.mouseCursors, new Vector2(UpBound.X, UpBound.Y), leftbutton, Color.White, MathHelper.ToRadians(90f), new Vector2(0, 32), 1f, SpriteEffects.None, 0.9999999f);
    }
    public void DrawDownButton(SpriteBatch b)
    {
        Rectangle rightbutton;
        rightbutton = new Rectangle(448, 96, 24, 32);
        b.Draw(Game1.mouseCursors, new Vector2(DownBound.X, DownBound.Y), rightbutton, Color.White,  MathHelper.ToRadians(90f), new Vector2(0, 32), 1f, SpriteEffects.None, 0.9999999f);
    }
    public void Draw(SpriteBatch b)
    {
    }
}
//todo: delete all code about repeat
public class SelectByArrowButton
{
    readonly int x;
    readonly int y;
    readonly int width;
    readonly int height;
    int max_index;
    int index = 0;
    readonly List<string>? content = null;
    string text;
    private readonly bool leftright;
    private bool is_repeat = false;
    public List<Rectangle> PageButtonBound{ get;set; }
    public Rectangle TextBound{ get;set; } 
    public SelectByArrowButton(int X, int Y, int W, int H, List<string> Content, bool Leftright = false)
    {
        x = X;
        y = Y;
        width = W;
        height = H;
        max_index = Content.Count - 1;
        text = Content[index];
        content = Content;
        leftright = Leftright;
        if (leftright)
        {
            PageButtonBound = new List<Rectangle>() 
            {   
                new(x, y, height * 24 / 32, height), 
                new(x + width + height * 24 / 32, y, height * 24 / 32, height)
            };
        }
        else
        {
            PageButtonBound = new List<Rectangle>() 
            {   
                new(x + width / 2 - height / 2, y - height * 24 / 32, height, height * 24 / 32), 
                new(x + width / 2 - height / 2, y + height, height, height * 24 / 32)
            };
        }
        TextBound = leftright ? new Rectangle(x + PageButtonBound[0].Width, y, width, height) : new Rectangle(x, y, width, height);
    }
    public SelectByArrowButton(int X, int Y, int W, int H, int MaxIndex = 99, bool Leftright = false)
    {
        x = X;
        y = Y;
        width = W;
        height = H;
        
        max_index = MaxIndex;
        text = (index + 1).ToString();
        leftright = Leftright;
        // left right button
        if (leftright)
        {
            PageButtonBound = new List<Rectangle>() 
            {   
                new(x - height * 24 / 32, y, height * 24 / 32, height), 
                new(x + width, y, height * 24 / 32, height)
            };
        }
        // up down button
        else
        {
            PageButtonBound = new List<Rectangle>() 
            {   
                new(x + width / 2 - height / 2, y - height * 24 / 32, height, height * 24 / 32), 
                new(x + width / 2 - height / 2, y + height, height, height * 24 / 32)
            };
        }
        TextBound = new Rectangle(x, y, width, height);
    }
    public void Draw(SpriteBatch b)
    {
        Rectangle leftbutton, rightbutton;
        leftbutton = new Rectangle(480, 96, 24, 32);
        rightbutton = new Rectangle(448, 96, 24, 32);
        // left right button
        if (leftright)
        {
            b.Draw(Game1.menuTexture, TextBound, new Rectangle(0, 320, 60, 60), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9999999f);
            b.Draw(Game1.mouseCursors, PageButtonBound[0], leftbutton, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9999999f);
            b.Draw(Game1.mouseCursors, PageButtonBound[1], rightbutton, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9999999f);
            Vector2 size = Game1.smallFont.MeasureString(text);
            b.DrawString(Game1.smallFont, text, new Vector2(x + (width - size.X) / 2, y + (height - size.Y * 3 / 4) / 2), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
        }
        // up down button
        else
        {
            b.Draw(Game1.menuTexture, TextBound, new Rectangle(0, 320, 60, 60), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9999999f);
            b.Draw(Game1.mouseCursors, new Rectangle(PageButtonBound[0].X, PageButtonBound[0].Y, PageButtonBound[0].Height, PageButtonBound[0].Width), leftbutton, Color.White, MathHelper.ToRadians(90f), new Vector2(0, 32), SpriteEffects.None, 0.9999999f);
            b.Draw(Game1.mouseCursors, new Rectangle(PageButtonBound[1].X, PageButtonBound[1].Y, PageButtonBound[1].Height, PageButtonBound[1].Width), rightbutton, Color.White, MathHelper.ToRadians(90f), new Vector2(0, 32), SpriteEffects.None, 0.9999999f);
            Vector2 size = Game1.smallFont.MeasureString(text);
            b.DrawString(Game1.smallFont, text, new Vector2(x + (width - size.X) / 2, y + (height - size.Y * 3 / 4) / 2), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
        }
    }
    public void IncreasePage(bool add5 = false)
    {
        if (add5)
        {
            index += 5;
        }
        else
        {
            index++;
        }
        if (index > max_index)
        {
            index = 0;
        }
        if (is_repeat && index == max_index)
            text = ModEntry.Helper1.Translation.Get("ChooseDate.Per");
        else        
            text = content == null ? (index + 1).ToString() : content[index];
    }
    public void DecreasePage(bool add5 = false)
    {
        if (add5)
        {
            index -= 5;
        }
        else
        {
            index--;
        }    
        if (index < 0)
        {
            index = max_index;
        }
        if (is_repeat && index == max_index)
            text = ModEntry.Helper1.Translation.Get("ChooseDate.Per");
        else
            text = content == null ? (index + 1).ToString() : content[index];
    }
    public void SetDefaultText(string default_text)
    {
        text = default_text;
    }
    public void SetDefaultIndex(int index)
    {
        this.index = index;
        CheckIfRepeat();
        if (is_repeat && index == max_index)
            text = ModEntry.Helper1.Translation.Get("ChooseDate.Per");
        else
            text = content == null ? (index + 1).ToString() : content[index];
    }
    public void SetIsRepeat(bool is_repeat)
    {
        
        if (is_repeat  == true && this.is_repeat == false)
            max_index ++;
        else if (is_repeat == false && this.is_repeat == true)
            max_index --;
        this.is_repeat = is_repeat;
    }
    public void CheckIfRepeat()
    {
        if (index > max_index && is_repeat == false)
        {
            SetIsRepeat(true);
        }
    }
    public string GetSelectedValue()
    {
        return index.ToString();
    }
    public void Release()
    {
        SetIsRepeat(false);
        index = 0;
        text = content == null ? (index + 1).ToString() : content[index];
    }
}
public class SelectButton
{
    private readonly int x, y, h, gap;
    readonly float scale;
    readonly Vector2 title_size;
    readonly string title;
    readonly List<string> options;
    readonly List<bool> select_option = new();
    readonly List<Rectangle> select_bounds = new();
    public SelectButton(int X, int Y, int Height, int Gap, string title, List<string> options)
    {
        x = X;
        y = Y;
        h = Height;
        gap = Gap;
        this.title = title;
        this.options = options;
        title_size = Game1.smallFont.MeasureString(title);
        scale = h / title_size.Y;
        foreach (var _ in options)
        {
            select_option.Add(false);
        }
        int x1 = (int)(x + title_size.X * scale + h / 4);
        int y1 = y + h / 4;
        int h1 = h / 2;
        int w1 = h / 2;
        for (int i = 0; i < options.Count; i++)
        {
            string option = options[i];
            x1 += (int)(Game1.smallFont.MeasureString(option).X * scale + w1);
            select_bounds.Add(new Rectangle(x1, y1, w1, h1));
            x1 += w1 + gap;
        }
    }
    public void Draw(SpriteBatch b)
    {
        SpriteFont font = Game1.smallFont;

        int x1 = (int)(x + title_size.X * scale + h / 4);
        int y1 = y + h / 4;
        int h1 = h / 2;
        int w1 = h / 2;

        b.DrawString(font, title, new Vector2(x, y), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.9999999f);
        for (int i = 0; i < options.Count; i++)
        {
            string option = options[i];
            b.DrawString(font, option, new Vector2(x1, y), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.9999999f);
            x1 += (int)(font.MeasureString(option).X * scale + w1);
            DrawSelectBox(b, x1, y1, w1, h1, select_option[i]);
            x1 += w1 + gap;
        }
    }
    public static void DrawSelectBox(SpriteBatch b, int x, int y, int w, int h, bool isSelected)
    {
        if (isSelected)
        {
            b.Draw(Game1.menuTexture, new Rectangle(x, y, w, h), new Rectangle(192, 768, 36, 36), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9999999f);
        }
        else
        {
            b.Draw(Game1.menuTexture, new Rectangle(x, y, w, h), new Rectangle(128, 768, 36, 36), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9999999f);
        }
    }
    public string GetSelectedOption()
    {
        int n = select_option.Count;
        for (int i = 0; i < n; i++)
        {
            if (select_option[i])
            {
                return options[i];
            }
        }
        return "";
    }
    // index start from -1
    public int GetSelectedOptionIndex()
    {
        int n = select_option.Count;
        for (int i = 0; i < n; i++)
        {
            if (select_option[i])
            {
                return i;
            }
        }
        return -1;
    }
    public void ToggleOption(int index)
    {
        if (index == -1)
        {
            return;
        }
        int n = select_option.Count;
        for (int i = 0; i < n; i++)
        {
            select_option[i] = i == index && (!select_option[i]);
        }
    }
    public int GetClickedOptionIndex(int x, int y)
    {
        for (int i = 0; i < select_bounds.Count; i++)
        {
            if (select_bounds[i].Contains(x, y))
                return i;
        }
        return -1;
    }

    internal void SetSelectedOptionIndex(int index)
    {
        int n = select_option.Count;
        for (int i = 0; i < n; i++)
        {
            select_option[i] = i == index;
        }
    }
}
public class CancelButton
{
    readonly int x;
    readonly int y;
    readonly int width;
    readonly int height;
    public Rectangle CancelBound{ get;set; }
    public CancelButton(int X, int Y, int W, int H)
    {
        x = X;
        y = Y;
        width = W;
        height = H;
        CancelBound = new Rectangle(x, y, width, height);
    }
    public void Draw(SpriteBatch b)
    {
        Rectangle cancelbutton;
        cancelbutton = new Rectangle(322, 498, 12, 12);
        b.Draw(Game1.mouseCursors, new Vector2(CancelBound.X, CancelBound.Y), cancelbutton, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9999999f);
    }
}

public class DrawLimitWidthString
{
    public static string LimitWidth(string str, SpriteFont font, int width)
    {
        Vector2 size = font.MeasureString(str);
        string limited;
        if (size.X > width)
        {
            limited = string.Concat(str.AsSpan(0, (int)Math.Floor(str.Length * (float)width / size.X) - 1), "...");
        }
        else
        {
            limited = str;
        }
        return limited;
    }
    public static void Draw(SpriteBatch b, string text, SpriteFont font, int x, int y, int width, Color color)
    {
        string limited = LimitWidth(text, font, width);
        b.DrawString(font, limited, new Vector2(x, y), color);
    }
    public static void Draw(SpriteBatch b, SpriteFont font, string text, int width, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        string limited = LimitWidth(text, font, width);
        b.DrawString(font, limited, position, color, rotation, origin, scale, effects, layerDepth);
    }
}
public class ChooseButton
{
    public Rectangle Bound{ get;set; }
    public Rectangle EditBound{ get;set; }
    public Rectangle TextBound{ get;set; }
    public ChooseButton(int X, int Y, int W, int H)
    {
        Bound = new Rectangle(X, Y, W, H);
        EditBound = new Rectangle(Bound.X + Bound.Width - Game1.tileSize, Bound.Y, 48, 48);
        TextBound = new Rectangle(Bound.X + (int)Game1.dialogueFont.MeasureString(Translations.GetStr("EditPage.Time") + " ").X, Bound.Y, (int)Game1.dialogueFont.MeasureString(Translations.GetStr("EditPage.Time.Description")).X, (int)Game1.dialogueFont.MeasureString(Translations.GetStr("EditPage.Time.Description")).Y);
    }
    public void Draw(SpriteBatch b, string text)
    {
        Rectangle Editbutton = new(274, 284, 16, 16);
        if (text.Contains('-'))
        {
            b.DrawString(Game1.smallFont, text.Split('-')[0], new Vector2(Bound.X + 12, Bound.Y), Color.Black);
        }
        else
        {
            b.DrawString(Game1.dialogueFont, text, new Vector2(Bound.X + 12, Bound.Y), Color.Black);
        }
        b.Draw(Game1.mouseCursors, new Vector2(EditBound.X, EditBound.Y), Editbutton, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9999999f);
    }
    public void Draw(SpriteBatch b, string label, string text)
    {
        Rectangle Editbutton = new(274, 284, 16, 16);
        b.DrawString(Game1.dialogueFont, label + ' ', new Vector2(Bound.X + 12, Bound.Y), Color.Black);
        Vector2 size = Game1.dialogueFont.MeasureString(label + ' ');
        float scale = Game1.dialogueFont.MeasureString(label + ' ' + text).X + 12 > Bound.Width - Game1.tileSize - 5 ? (Bound.Width - size.X - 12 - Game1.tileSize - 5 ) / Game1.dialogueFont.MeasureString(text).X : 1f;
        b.DrawString(Game1.dialogueFont, text, new Vector2(Bound.X + 12 + size.X, Bound.Y + (1 - scale) * size.Y * 2 / 3), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0.9999999f);
        b.Draw(Game1.mouseCursors, new Vector2(EditBound.X, EditBound.Y), Editbutton, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9999999f);
    }
    public void Draw(SpriteBatch b, string text, List<string>? contents)
    {
        Rectangle Editbutton = new(274, 284, 16, 16);
        b.DrawString(Game1.dialogueFont, text, new Vector2(Bound.X + 12, Bound.Y), Color.Black);
        b.Draw(Game1.mouseCursors, new Vector2(EditBound.X, EditBound.Y), Editbutton, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9999999f);
        if (contents is null)
        {
            return;
        }
        int y  = (int)(Bound.Y + Game1.dialogueFont.MeasureString(text).Y + 4);
        b.DrawString(Game1.smallFont, contents[0].Split('-')[0], new Vector2(Bound.X + 12, y), Color.Black);
        b.DrawString(Game1.smallFont, contents[0].Split('-')[1], new Vector2(Bound.X + 12, 4 + y + Game1.smallFont.MeasureString(contents[0].Split('-')[0]).Y), Color.Black);
    } 
}
public class ActionButton
{
    private readonly int x;
    private readonly int y;
    private readonly int w;
    private readonly int h;
    public Rectangle bound;
    public string text;
    Rectangle src_bound, dest_bound;
    public ActionButton(int X, int Y, int W, int H, string action)
    {
        x = X;
        y = Y;
        w = W;
        h = H;
        text = action;
        bound = new Rectangle(x, y, w, h);
        Vector2 center = new(x + w / 2, y + h / 2);
        src_bound = ChooseActionTexture(action, out int scale);
        dest_bound = new Rectangle((int)center.X - src_bound.Width * scale / 2, (int)center.Y - src_bound.Height * scale / 2, src_bound.Width * scale, src_bound.Height * scale);
    }
    public void Draw(SpriteBatch b)
    {
        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, w, h, Color.White);
        b.Draw(Game1.mouseCursors, dest_bound, src_bound, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.9999999f);
        string action_name = Translations.GetStr("ChooseAction", text);
        Vector2 action_name_size = Game1.dialogueFont.MeasureString(action_name);
        b.DrawString(Game1.dialogueFont, action_name, new Vector2(x + w / 2 - action_name_size.X / 2, y + h + action_name_size.Y / 4), Color.Black);
        //b.Draw(Game1.mouseCursors, bound, action_texture, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
    }
    private Rectangle ChooseActionTexture(string action, out int scale)
    {
        Rectangle action_texture = new();
        scale = w / 2 / 10;
        switch (action)
        {
            case "Forage":
                action_texture = new Rectangle(60, 428, 10, 10);
                break;
            case "Combat":
                action_texture = new Rectangle(40, 428, 10, 10);
                break;
            case "Fish":
                action_texture = new Rectangle(20, 428, 10, 10);
                break;
            case "Mine":
                action_texture = new Rectangle(30, 428, 10, 10);
                break;
            case "Farm":
                action_texture = new Rectangle(10, 428, 10, 10);
                break;
            case "Donate":
                action_texture = new Rectangle(294, 392, 16, 16);
                scale = w / 2 / 16;
                break;
            case "Talk":
                action_texture = new Rectangle(66, 4, 14, 12);
                break;
            case "Gift":
                action_texture = new Rectangle(229, 410, 14, 14);
                break;
            case "Buy":
                action_texture = new Rectangle(281, 412, 14, 14);
                break;
            case "Build":
                action_texture = new Rectangle(369, 376, 10, 10);
                break;
            case "Quest":
                action_texture = new Rectangle(403, 496, 5, 14);
                break;
            default:
                break;
        }
        return action_texture;
    }
}

public class DrawChoosePage
{
    //private bool is_portrait_chosen = false;

    public static List<Rectangle> Draw(SpriteBatch b, int x, int y, int width, int height, Dictionary<string, List<List<string>>> npc_data)
    {
        return DrawCharacterPage(b, x, y, width, height, npc_data);
    }
    public static List<Rectangle> Draw(SpriteBatch b,int x, int y, int width, int height, int location_display_index,  List<string> location_data)
    {
        return new List<Rectangle>()
        {
             DrawLocationPage(b, x, y, width, height, location_data, location_display_index)
        };
    }
    public static Dictionary<string, Rectangle> Draw(SpriteBatch b, int x, int y, int width, int height, bool is_portrait_chosen)
    {
        return DrawActionPage(b, x, y, width, height, is_portrait_chosen);
    }
    public static List<Rectangle> Draw(SpriteBatch b, int x, int y, int width, int height, int time_display_index)
    {
        return new List<Rectangle>()
        {
             DrawTimePage(b, x, y, width, height)
        };
    }

    public static List<Rectangle> DrawCharacterPage(SpriteBatch b, int x, int y, int width, int height, Dictionary<string, List<List<string>>> npc_data)
    {
        Rectangle background = new(603, 414, 74, 74);
        DrawPortraitBackground.Draw(b, Game1.mouseCursors, background, x - 2 * Game1.tileSize, y - Game1.tileSize, width + 4 * Game1.tileSize, height + Game1.tileSize * 3 / 2, Color.White);
        return DrawPortraitWithName(b, new Vector2(x + (width - 6 * (Game1.tileSize + 16) - Game1.tileSize) / 2, y - Game1.tileSize / 2), npc_data);
    }

    private static List<Rectangle> DrawPortraitWithName(SpriteBatch b, Vector2 position, Dictionary<string, List<List<string>>> npc_data)
    {
        int i = 0;
        const int cnt_per_row = 7;
        List<Rectangle> choose_which_npc = new();
        //loaddata.NPCdata type is IEnumerable<NPC>
        foreach (var npc in npc_data)
        {
            string display_name = NPC.GetDisplayName(npc.Key);
            string texture_name = NPC.getTextureNameForCharacter(npc.Key);
            b.Draw(Game1.content.Load<Texture2D>("Portraits\\" + texture_name), new Vector2(position.X + i % cnt_per_row * (Game1.tileSize + 16), position.Y + i / cnt_per_row * (Game1.tileSize + 32)), new Rectangle(0, 0, NPC.portrait_width, NPC.portrait_height), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, layerDepth: 1);
            choose_which_npc.Add(new Rectangle((int)position.X + i % cnt_per_row * (Game1.tileSize + 16), (int)position.Y + i / cnt_per_row * (Game1.tileSize + 32), NPC.portrait_width, NPC.portrait_height));
            if (Game1.smallFont.MeasureString(display_name).X > Game1.tileSize)
            {
                DrawLimitWidthString.Draw(b, display_name, Game1.smallFont, (int)position.X + i % cnt_per_row * (Game1.tileSize + 16), (int)position.Y + i / cnt_per_row * (Game1.tileSize + 32) + Game1.tileSize, Game1.tileSize + 16, Color.Black);
            }
            else
            {
                b.DrawString(Game1.smallFont, display_name, new Vector2(position.X + i % cnt_per_row * (Game1.tileSize + 16), position.Y + i / cnt_per_row * (Game1.tileSize + 32) + Game1.tileSize), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
            }
            i++;
        }
        return choose_which_npc;
    }
    public static Rectangle DrawLocationPage(SpriteBatch b, int x, int y, int width, int height, List<string> locations, int location_display_index = 0)
    {
        const int display_cnt = 6;
        const int box_height = Game1.tileSize;
        //if (location_display_index == 0) location_display_index = display_cnt;
        //Game1.DrawBox(x, y, width, height);
        for (int i = 0; i < display_cnt && i + location_display_index < locations.Count; i++)
        {
            IClickableMenu.drawTextureBox(b, x, y + height - (display_cnt - i) * box_height, width, box_height, Color.White);
            b.DrawString(Game1.smallFont, (i + location_display_index + 1).ToString() + " " + Translations.GetStr("ChooseLocation", locations[i + location_display_index]), new Vector2(x + 16, y + 16 + height - (display_cnt - i) * box_height), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9999999f);
        }
        return new Rectangle(x, y + height - display_cnt * box_height, width, height);
    }
    public static Dictionary<string, Rectangle> DrawActionPage(SpriteBatch b, int x, int y, int width, int height, bool is_portrait_chosen = false)
    {
        Dictionary<string, Rectangle> action_bounds = new();
        List<ActionButton> action_buttons = new();
        //List<string> actions1 = new() { "Talking", "Gifting", "Buying", "Building", "Questing" };
        //List<string> actions2 = new() { "Foraging", "Combat", "Fishing", "Mining", "Farming", "Donating" };
        if (is_portrait_chosen)
        {
            action_buttons.Add(new ActionButton(x + width / 4 - 32,  y + Game1.tileSize * 2, 64, 64, "Talk"));
            action_buttons.Add(new ActionButton(x + width * 3 / 4 - 16,  y + Game1.tileSize * 2, 64, 64, "Gift"));
            action_buttons.Add(new ActionButton(x + width / 4 - 32, y + height / 2, 64, 64, "Buy"));
            action_buttons.Add(new ActionButton(x + width * 3 / 4 - 16, y + height / 2, 64, 64, "Build"));
            action_buttons.Add(new ActionButton(x + width / 4 - 32, y + height - Game1.tileSize * 2, 64, 64, "Quest"));
        }
        else
        {
            action_buttons.Add(new ActionButton(x + width / 4 - 32, y + Game1.tileSize * 2, 64, 64, "Forage"));
            action_buttons.Add(new ActionButton(x + width * 3 / 4 - 16, y + Game1.tileSize * 2, 64, 64, "Combat"));
            action_buttons.Add(new ActionButton(x + width / 4 - 32, y + height / 2, 64, 64, "Fish"));
            action_buttons.Add(new ActionButton(x + width * 3 / 4 - 16, y + height / 2, 64, 64, "Mine"));
            action_buttons.Add(new ActionButton(x + width / 4 - 32, y + height - Game1.tileSize * 2, 64, 64, "Farm"));
            action_buttons.Add(new ActionButton(x + width * 3 / 4 - 16, y + height - Game1.tileSize * 2, 64, 64, "Donate"));
        }
        foreach (var button in action_buttons)
        {
            button.Draw(b);
            action_bounds.Add(button.text, button.bound);
        }
        return action_bounds;
    }
    public static Rectangle DrawTimePage(SpriteBatch b, int x, int y, int width, int height)
    {
        
        return new Rectangle(x, y, width, height);
    }
}
public class DrawPortraitBackground
{
    public static void Draw(SpriteBatch b, Texture2D texture, Rectangle sourceRect, int x, int y, int width, int height, Color color, int edge_width = 10, float scale = 5f, float draw_layer = -1f)
    {
        int num1 = edge_width;
        int num2 = sourceRect.Width - edge_width * 2;
        float layerDepth = draw_layer - 0.03f;
        if (draw_layer < 0f)
        {
            draw_layer = 0.8f - (float)y * 1E-06f;
            layerDepth = 0.77f;
        }
        b.Draw(texture, new Rectangle((int)((float)num1 * scale) + x, (int)((float)num1 * scale) + y, width - (int)((float)num1 * scale * 2f), height - (int)((float)num1 * scale * 2f)), new Rectangle(num1 + sourceRect.X, num1 + sourceRect.Y, num2, num2), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
        b.Draw(texture, new Vector2(x, y), new Rectangle(sourceRect.X, sourceRect.Y, num1, num1), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
        b.Draw(texture, new Vector2(x + width - (int)((float)num1 * scale), y), new Rectangle(sourceRect.X + num1 + num2, sourceRect.Y, num1, num1), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
        b.Draw(texture, new Vector2(x, y + height - (int)((float)num1 * scale)), new Rectangle(sourceRect.X, num1 + num2 + sourceRect.Y, num1, num1), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
        b.Draw(texture, new Vector2(x + width - (int)((float)num1 * scale), y + height - (int)((float)num1 * scale)), new Rectangle(sourceRect.X + num1 + num2, num1 + num2 + sourceRect.Y, num1, num1), color, 0f, Vector2.Zero, scale, SpriteEffects.None, draw_layer);
        b.Draw(texture, new Rectangle(x + (int)((float)num1 * scale), y, width - (int)((float)num1 * scale) * 2, (int)((float)num1 * scale)), new Rectangle(sourceRect.X + num1, sourceRect.Y, num2, num1), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
        b.Draw(texture, new Rectangle(x + (int)((float)num1 * scale), y + height - (int)((float)num1 * scale), width - (int)((float)num1 * scale) * 2, (int)((float)num1 * scale)), new Rectangle(sourceRect.X + num1, num1 + num2 + sourceRect.Y, num2, num1), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
        b.Draw(texture, new Rectangle(x, y + (int)((float)num1 * scale), (int)((float)num1 * scale), height - (int)((float)num1 * scale) * 2), new Rectangle(sourceRect.X, num1 + sourceRect.Y, num1, num2), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
        b.Draw(texture, new Rectangle(x + width - (int)((float)num1 * scale), y + (int)((float)num1 * scale), (int)((float)num1 * scale), height - (int)((float)num1 * scale) * 2), new Rectangle(sourceRect.X + num1 + num2, num1 + sourceRect.Y, num1, num2), color, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
    }
}
public class DrawReminder
{
    public static void Draw(SpriteBatch b, int x, int y, int width, int height, List<RemindMessage> remindMessage)
    {
        DrawPortraitBackground.Draw(b, Game1.mouseCursors, new Rectangle(603, 414, 74, 74), x, y, width, height, Color.White);
        int num1 = 0;
        for (int i = 0; i < remindMessage.Count; i++)
        {
            num1 += DrawMultilineString(b, remindMessage[i].GetRemindMessageDisplay(), Game1.smallFont, x + 32, y + 32 + num1, Color.Black, width - 64);
        }
    }
    public static int DrawMultilineString(SpriteBatch b, string text, SpriteFont font, int x, int y, Color color, int line_width = 200)
    {
        Vector2 size = font.MeasureString(text);
        int line_height = (int)size.Y + 4;
        //the count of lines
        int line_count = (int)Math.Ceiling(size.X / line_width);
        //count of characters in each line
        int char_count = (int)Math.Floor(text.Length * line_width / size.X);
        for (int i = 0; i < line_count; i++)
        {
            int start_index = i * char_count;
            int end_index = Math.Min(text.Length, start_index + char_count);
            string line = text.Substring(start_index, end_index - start_index);
            b.DrawString(font, line, new Vector2(x, y + i * line_height), color);
        }
        return line_height * line_count;
    }
}