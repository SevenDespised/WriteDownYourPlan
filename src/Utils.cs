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
    public static string Index2TimeString(List<SelectByArrowButton> select_by_arrow_buttons)
    {
        //delete about repeat
        string start_date = select_by_arrow_buttons[0].GetSelectedValue() + " " + select_by_arrow_buttons[1].GetSelectedValue() + " " + select_by_arrow_buttons[2].GetSelectedValue() + " " + select_by_arrow_buttons[3].GetSelectedValue();
        string end_date = select_by_arrow_buttons[4].GetSelectedValue() + " " + select_by_arrow_buttons[5].GetSelectedValue() + " " + select_by_arrow_buttons[6].GetSelectedValue() + " " + select_by_arrow_buttons[7].GetSelectedValue();
        return start_date + "-" + end_date;
    }
    public static List<int>? TimeString2Index(string dateString)
    {
        //delete about repeat
        if (dateString == "")
        {
            return null;
        }
        List<int> index_list = new();
        string[] start_date = dateString.Split('-')[0].Split(' ');
        string[] end_date = dateString.Split('-')[1].Split(' ');
        index_list.Add(int.Parse(start_date[0]));
        index_list.Add(int.Parse(start_date[1]));
        index_list.Add(int.Parse(start_date[2]));
        index_list.Add(int.Parse(start_date[3]));
        index_list.Add(int.Parse(end_date[0]));
        index_list.Add(int.Parse(end_date[1]));
        index_list.Add(int.Parse(end_date[2]));
        index_list.Add(int.Parse(end_date[3]));
        return index_list;
    }
    public static string? Index2DisplayText(List<int>? index_list)
    {
        //delete about repeat
        if (index_list == null)
            return null;
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
        return Translations.GetStr("ChooseDate.DisplayDate", StartDateDisplayText) + "-" + Translations.GetStr("ChooseDate.DisplayDate", EndDateDisplayText);
        //disp_text_list.Add(Translations.GetStr("ChooseDate.DisplayDate", EndDateDisplayText));
    }
    public static string? TimeString2DisplayText(string timeString)
    {
        string? display_text = Index2DisplayText(TimeString2Index(timeString));
        return display_text;
    }
    public static void DecodeRepeatButton(SelectButton selectButton, int state)
    {
        selectButton.SetSelectedOptionIndex(state);
    }
    public static int EncodeRepeatButton(SelectButton selectButton)
    {
        return selectButton.GetSelectedOptionIndex();
    }
    //option index starts from -1, special value from 0 to 8
    //0: (null, null), 1: (sunny, null), 2: (rainy, null), 3: (null, lucky), 4: (sunny, lucky), 5: (rainy, lucky), 6: (null, unlucky), 7: (sunny, unlucky), 8: (rainy, unlucky) 
    public static int EncodeSpecialDateButton(List<SelectButton> select_buttons, int state_counts)
    {
        int state = 0;
        for (int i = 0; i < select_buttons.Count; i++)
        {
            state += (select_buttons[i].GetSelectedOptionIndex() + 1) * (int)Math.Pow(state_counts, i);
        }
        return state;
    }
    public static List<int> DecodeSpecialDateButton(int state, List<SelectButton> select_buttons, int state_counts)
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
    public static int[] DecodeSpecialDate(int state, int button_count = 2, int state_count = 3)
    {
        int[] index_list = new int[button_count];
        for (int i = 0; i < button_count; i++)
        {
            int option_index = (int)Math.Floor((double)(state / (int)Math.Pow(state_count, i)) % state_count);
            index_list[i] = option_index;
        }
        return index_list;
    }
    public static List<SDate>? DateParse(string dateString, out int[] time)
    {
        List<SDate> res = new();
        List<int>? idx;
        List<string> seasonString = new() { "spring", "summer", "fall", "winter" };                                                                                                                                                                                      
        time = new int[2] { 0, 0 };

        idx = TimeString2Index(dateString);
        if (idx == null)
        {
            return null; 
        }
        time[0] = 600 + 100 * idx[3]; // time start from 600 to 2600
        res.Add(new SDate(idx[2] + 1, seasonString[idx[1]], idx[0] + 1));
        time[1] = 600 + 100 * idx[7]; 
        res.Add(new SDate(idx[6] + 1, seasonString[idx[5]], idx[4] + 1));
        return res;
    }
    public static int[] SDate2Index4Repeat(SDate date)
    {
        int[] index = new int[4];
        int day = (date.Day - 1) % 7;
        int week = (date.Day - 1) / 7;
        index[0] = date.Year - 1;
        index[1] = date.SeasonIndex;
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