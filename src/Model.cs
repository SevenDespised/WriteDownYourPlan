using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.GameData;


namespace WritedownYourPlan.src;
public sealed class ModConfig
{
    public KeybindList DisplayWYPButton { get; set; } = new(SButton.F2);
    public KeybindList SaveModelButton { get; set; } = KeybindList.Parse($"{SButton.LeftShift} + {SButton.F2}");
    public int MaxPlan { get; set; } = 20;
    public bool ReadDatatFromJson { get; set; } = false;
    public bool DisplayHUDMessage { get; set; } = true;
}

public class TestCla1
{
    public Dictionary<string, HashSet<string>> res = new();
}
public sealed class MenuEnum
{
    public enum PageEnum
    {
        mainPage,
        editPage,
        choosePage,
    }
    public enum ChoosePageEnum
    {
        characterPage = 0,
        locationPage,
        actionPage,
        timePage,
        itemPage,
    }
}
public sealed class TimeList
{  
    public static List<string> Seasons { get; set; } = new() { Translations.GetStr("ChooseDate.Season.Spring"), Translations.GetStr("ChooseDate.Season.Summer"), Translations.GetStr("ChooseDate.Season.Fall"), Translations.GetStr("ChooseDate.Season.Winter") };
    public static List<string> SeasonsLong { get; set; } = new() { Translations.GetStr("ChooseDate.Season.SpringLong"), Translations.GetStr("ChooseDate.Season.SummerLong"), Translations.GetStr("ChooseDate.Season.FallLong"), Translations.GetStr("ChooseDate.Season.WinterLong") , Translations.GetStr("ChooseDate.Per")};
    public static List<string> Days { get; set; } = new() { Translations.GetStr("ChooseDate.Day.Monday"), Translations.GetStr("ChooseDate.Day.Tuesday"), Translations.GetStr("ChooseDate.Day.Wednesday"), Translations.GetStr("ChooseDate.Day.Thursday"), Translations.GetStr("ChooseDate.Day.Friday"), Translations.GetStr("ChooseDate.Day.Saturday"), Translations.GetStr("ChooseDate.Day.Sunday") };
    public static List<string> Hours { get; set; } = new() { "06 am", "07 am", "08 am", "09 am", "10 am", "11 am", "12 am", "01 pm", "02 pm", "03 pm", "04 pm", "05 pm", "06 pm", "07 pm", "08 pm", "09 pm", "10 pm", "11 pm", "00 am", "01 am", "02 am" };
    public static void Init()
    {
        Seasons = new() { Translations.GetStr("ChooseDate.Season.Spring"), Translations.GetStr("ChooseDate.Season.Summer"), Translations.GetStr("ChooseDate.Season.Fall"), Translations.GetStr("ChooseDate.Season.Winter") };
        SeasonsLong= new() { Translations.GetStr("ChooseDate.Season.SpringLong"), Translations.GetStr("ChooseDate.Season.SummerLong"), Translations.GetStr("ChooseDate.Season.FallLong"), Translations.GetStr("ChooseDate.Season.WinterLong") , Translations.GetStr("ChooseDate.Per")};
        Days = new() { Translations.GetStr("ChooseDate.Day.Monday"), Translations.GetStr("ChooseDate.Day.Tuesday"), Translations.GetStr("ChooseDate.Day.Wednesday"), Translations.GetStr("ChooseDate.Day.Thursday"), Translations.GetStr("ChooseDate.Day.Friday"), Translations.GetStr("ChooseDate.Day.Saturday"), Translations.GetStr("ChooseDate.Day.Sunday") };
        Hours= new() { "06 am", "07 am", "08 am", "09 am", "10 am", "11 am", "12 am", "01 pm", "02 pm", "03 pm", "04 pm", "05 pm", "06 pm", "07 pm", "08 pm", "09 pm", "10 pm", "11 pm", "00 am", "01 am", "02 am" };
    }
}
public sealed class Plan
{
    public string title = "";
    public string npc = "";
    public string location = "";
    public string action = "";
    public string time = "";
    public int special = 0;
    public string repeat = "";
    public string item = "";
}
public class HoverFlags
{
    private bool mainPage_HoverPlan = false;
    private bool timePage_HoverTime = false;
    public bool MainPage_HoverPlan { get => mainPage_HoverPlan; set => mainPage_HoverPlan = value; }
    public bool TimePage_HoverTime { get => timePage_HoverTime; set => timePage_HoverTime = value; }
    public void SetDefaultFlags()
    {
        MainPage_HoverPlan = false;
        TimePage_HoverTime = false;
    }
}
public sealed class Time
{
    public int year = 1;
    public int season = 1;
    public int week = -1;
    public int day = 1;
    public int hour = 1;
}
public sealed class PlanData
{
    public List<Plan> plan = new();
    public int max_plan = 20;
}

public sealed class ModData
{
    public List<string> location_data = new();
    public Dictionary<string, List<List<string>>> npc_data = new();
}

public sealed class LoadData
{
    public static readonly IEnumerable<NPC> NPCdata = Utility.getAllCharacters().Distinct(); // fix rare issue where the game duplicates an NPC (seems to happen when the player's child is born)
    public static readonly IDictionary<string, StardewValley.GameData.Characters.CharacterData> CharacterData = Game1.characterData;
    public IList<GameLocation> LocationData = Game1.locations;
    public static readonly Dictionary<string, string> FestivalData = DataLoader.Festivals_FestivalDates(Game1.content);
    public static readonly Dictionary<string, PassiveFestivalData> PassiveFestivalData = DataLoader.PassiveFestivals(Game1.content);
}

