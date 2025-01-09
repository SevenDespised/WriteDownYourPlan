using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Reflection;

namespace WritedownYourPlan.src;
public static class TimeUtils
{
    public static List<string[]> DeadlineSplit(string time)
    {
        string[] time_split = time.Split('-');
        return new List<string[]>() { TimeSplit(time_split[0]), TimeSplit(time_split[1])};
    }
    public static string[] TimeSplit(string time)
    {
        return time.Split(' ');
    }
    public static List<string> Index2TimeString(List<SelectByArrowButton> select_by_arrow_buttons)
    {
        List<string> time_list = new();
        string start_date = select_by_arrow_buttons[0].GetSelectedValue() + " " + select_by_arrow_buttons[1].GetSelectedValue() + " " + select_by_arrow_buttons[2].GetSelectedValue() + " " + select_by_arrow_buttons[3].GetSelectedValue();
        string end_date = select_by_arrow_buttons[4].GetSelectedValue() + " " + select_by_arrow_buttons[5].GetSelectedValue() + " " + select_by_arrow_buttons[6].GetSelectedValue() + " " + select_by_arrow_buttons[7].GetSelectedValue();
        time_list.Add(start_date + "-" + end_date);
        if  (select_by_arrow_buttons.Count > 8)
        {
            string repeat_time = select_by_arrow_buttons[8].GetSelectedValue() + " " + select_by_arrow_buttons[9].GetSelectedValue() + " " + select_by_arrow_buttons[10].GetSelectedValue() + " " + select_by_arrow_buttons[11].GetSelectedValue();
            time_list.Add(repeat_time);
        }
        else
        {
            time_list.Add("");
        }

        return time_list;
    }
    public static List<int>? TimeString2Index(List<string> time_list)
    {
        if (time_list[0] == "")
        {
            return null;
        }
        List<int> index_list = new();
        string[] start_date = time_list[0].Split('-')[0].Split(' ');
        string[] end_date = time_list[0].Split('-')[1].Split(' ');
        string repeat_time = time_list[1];
        index_list.Add(int.Parse(start_date[0]));
        index_list.Add(int.Parse(start_date[1]));
        index_list.Add(int.Parse(start_date[2]));
        index_list.Add(int.Parse(start_date[3]));
        index_list.Add(int.Parse(end_date[0]));
        index_list.Add(int.Parse(end_date[1]));
        index_list.Add(int.Parse(end_date[2]));
        index_list.Add(int.Parse(end_date[3]));
        if (repeat_time!= "")
        {
            index_list.Add(int.Parse(repeat_time.Split(' ')[0]));
            index_list.Add(int.Parse(repeat_time.Split(' ')[1]));
            index_list.Add(int.Parse(repeat_time.Split(' ')[2]));
            index_list.Add(int.Parse(repeat_time.Split(' ')[3]));
        }
        else
        {
            index_list.Add(-1);
            index_list.Add(-1);
            index_list.Add(-1);
            index_list.Add(-1);
        }
        return index_list;
    }
    public static List<string>? Index2DisplayText(List<int>? index_list)
    {
        if (index_list == null)
            return null;
        List<string> disp_text_list = new();
        Dictionary<string, string> StartDateDisplayText = new()
        {
            {"year", (index_list[0] + 1).ToString()},
            {"season", TimeList.SeasonsLong[index_list[1]]},
            {"day", (index_list[2] + 1).ToString()},
            {"hour", TimeList.Hours[index_list[3]]}
        };
        Dictionary<string, string> EndDateDisplayText = new()
        {
            {"year", (index_list[4] + 1).ToString()},
            {"season", TimeList.SeasonsLong[index_list[5]]},
            {"day", (index_list[6] + 1).ToString()},
            {"hour", TimeList.Hours[index_list[7]]}
        };
        disp_text_list.Add(Translations.GetStr("ChooseDate.DisplayDate", StartDateDisplayText) + "-" + Translations.GetStr("ChooseDate.DisplayDate", EndDateDisplayText));
        //disp_text_list.Add(Translations.GetStr("ChooseDate.DisplayDate", EndDateDisplayText));
        if (index_list[8] != -1)
        {
            Dictionary<string, string> RepeatDisplayText = new()
            {
                {"year", (index_list[4] + 1).ToString()},
                {"season", TimeList.SeasonsLong[index_list[5]]},
                {"week", (index_list[6] + 1).ToString()},
                {"day", (index_list[6] + 1).ToString()}
            };
            disp_text_list.Add(Translations.GetStr("ChooseDate.DisplayRepeat", RepeatDisplayText));
        }
        return disp_text_list;
    }
    public static List<string>? TimeString2DisplayText(List<string> time_string_list)
    {
        List<string>? display_text = Index2DisplayText(TimeString2Index(time_string_list));
        return display_text;
    }
    public static int EncodeSpecialTimeButton(List<SelectButton> select_buttons, int state_counts)
    {
        int state = 0;
        for (int i = 0; i < select_buttons.Count; i++)
        {
            state += (select_buttons[i].GetSelectedOptionIndex() + 1) * (int)Math.Pow(state_counts, i);
        }
        return state;
    }
    public static List<int> DecodeSpecialTimeButton(int state, List<SelectButton> select_buttons, int state_counts)
    {
        List<int> index_list = new();
        for (int i = 0; i < select_buttons.Count; i++)
        {
            int option_index = (int)Math.Floor((double)(state / (int)Math.Pow(state_counts, i)) % state_counts);
            select_buttons[i].SetSelectedOptionIndex(option_index - 1);
            index_list.Add(option_index - 1);
        }
        return index_list;
    }
    public static List<SDate> DateParse(string dateString, out int[] time)
    {
        List<SDate> res = new();
        List<int>? idx;
        List<string> seasonString = new() { "spring", "summer", "fall", "winter" };                                                                                                                                                                                      
        time = new int[2] { 0, 0 };

        idx = TimeString2Index(new List<string> { dateString, "" });
        if (idx == null)
        {
            return res; 
        }
        time[0] = 600 + 100 * idx[3]; // time start from 600 to 2600
        res.Add(new SDate(idx[2] + 1, seasonString[idx[1]], idx[0] + 1));
        time[1] = 600 + 100 * idx[7]; 
        res.Add(new SDate(idx[6] + 1, seasonString[idx[5]], idx[4] + 1));
        return res;
    }
    public static string[] RepeatParse(string repeatString, out bool[] repeat)
    {
        string[] repeatIndex = repeatString.Split(' ');
        repeat = new bool[4] { false, false, false, false };
        if (repeatIndex[0] == "100")
        {
            repeat[0] = true;
        }
        if (repeatIndex[1] == "4")
        {
            repeat[1] = true;
        }
        if (repeatIndex[2] == "4")
        {
            repeat[2] = true;
        }
        if (repeatIndex[3] == "7")
        {
            repeat[3] = true;
        }
        return repeatIndex;
    }
    public static int[] SDate2Index4Repeat(SDate date)
    {
        int[] index = new int[4];
        int day = (date.Day - 1) % 7;
        int week = (date.Day - 1) / 7;
        string season = date.Season.ToString();
        index[0] = date.Year - 1;
        if (season == "Spring")
        {
            index[1] = 0;
        }
        else if (season == "Summer")
        {
            index[1] = 1;
        }
        else if (season == "Fall")
        {
            index[1] = 2;
        }
        else if (season == "Winter")
        {
            index[1] = 3;
        }
        index[2] = week;
        index[3] = day;
        return index;
    }
}
public class TextUtils
{
    public static string GetOrganizedText(Plan plan)
    {
        string translation_name = "";
        string[] text_part_count_enum = {"One.", "Two.", "Three.", "Four."};
        int text_part_count = -1;
        Dictionary<string, string> plan_texts = GetPlanText(plan);
        Dictionary<string, string> tokens = new();
        //ModEntry.Monitor1.Log(plan_texts.Count.ToString(), LogLevel.Debug);
        //foreach (string key in plan_texts.Keys)
        //    ModEntry.Monitor1.Log(key, LogLevel.Debug);
        if (plan_texts["npc"] != "")
        {
            translation_name += "NPC";
            tokens.Add("npc", Translations.GetNPCName(plan_texts["npc"]));
            text_part_count++;
        }
        if (plan_texts["location"] != "")
        {
            translation_name += "LOC";
            tokens.Add("location", Translations.GetStr("ChooseLocation." + plan_texts["location"]));
            text_part_count++;
        }
        if (plan_texts["action"] != "")
        {
            translation_name += "ACT";
            tokens.Add("action", Translations.GetStr("ChooseAction." + plan_texts["action"]));
            text_part_count++;
        }
        if (plan_texts["item"] != "")
        {
            translation_name += "ITEM";
            tokens.Add("item",  plan_texts["item"]);
            text_part_count++;
        }
        if (text_part_count == -1)
        {
            return "";
        }
        else
        {
            return Translations.GetStr("OrganizedText." + text_part_count_enum[text_part_count] + translation_name, tokens);
        }
    }
    public static Dictionary<string, string> GetPlanText(Plan plan)
    {
        Dictionary<string, string> plan_texts = new();
        //get all plan fields and store them in a list
        //foreach field, get its value and add it to the plan_texts list
        FieldInfo[] fields = plan.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo field in fields)
        {
            string name = field.Name;
            object? value = field.GetValue(plan);
            if (value is not null)
            {
                plan_texts.Add(name, value.ToString() ?? "");
            }
            else
            {
                plan_texts.Add(name, "");
            }
        }
        return plan_texts;
    }
}
public class Translations
{
    public static string GetStr(string key)
    {
        return key == "" ? "" : ModEntry.Helper1.Translation.Get(key);
    }
    public static string GetStr(string key, object tokens)
    {
        return key == "" ? "" : ModEntry.Helper1.Translation.Get(key, tokens);
    }

    public static string GetStr(string prefix, string key)
    {
        return key == "" ? "" : ModEntry.Helper1.Translation.Get(prefix + "." + key);
    }
    public static string GetStr(string prefix, string key, object tokens)
    {
        return key == "" ? "" : ModEntry.Helper1.Translation.Get(prefix + "." + key, tokens);
    }
    public static string GetNPCName(string name)
    {
        return NPC.GetDisplayName(name);
    }
}