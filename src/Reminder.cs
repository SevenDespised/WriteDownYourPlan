using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;

namespace WritedownYourPlan.src;
public class Reminder
{
    readonly PlanData planData;
    readonly ModData modData;
    private List<List<RemindMessage>>? remindMessages = null;
    //generate attribute of remind message
    public List<List<RemindMessage>>? RemindMessages
    {
        get
        {
            return remindMessages;
        }
    }
    const string reminderStartKey = "Start";
    const string reminderEndKey = "End";
    const string reminderRepeatStartKey = "RepeatStart";
    const string reminderFestivalKey = "Festival";
    const string reminderNpcLocKey = "NpcLoc";
    const string reminderBirthdayKey = "Birthday";
    const string reminderRainKey = "Rain";
    const string reminderNorainKey = "NoRain";
    const string reminderLuckyKey = "Lucky";
    const string reminderUnluckyKey = "Unlucky";
    const string reminderActUnmatchKey = "ActUnmatch";
    const string reminderLocUnmatchKey = "LocUnmatch";
    const string reminderContainBirthdayKey = "ContainBirthday";
    const string reminderContainFestivalKey = "ContainFestival";
    const string reminderContainPassiveFestivalKey = "ContainPassiveFestival";

    public Reminder(PlanData planData, ModData modData)
    {
        this.planData = planData;
        this.modData = modData;
    }
    
    public void Draw(SpriteBatch b, int x, int y, int width, int height, int planIndex)
    {
        if (remindMessages == null || remindMessages[planIndex].Count == 0)
        {
            return;
        }
        DrawReminder.Draw(b, x, y, width, height, remindMessages[planIndex]);
    }
    public void InitReminder()
    {
        remindMessages = GetAllRemindMessages();
    }
    internal void UpdateReminder()
    {
        remindMessages = GetAllRemindMessages();
    }
    public void DeleteReminder()
    {
        remindMessages = null;
    }
    private List<List<RemindMessage>> GetAllRemindMessages()
    {
        List<List<RemindMessage>> res = new();
        for (int i = 0; i < planData.plan.Count; i++)
        {
            List<RemindMessage> remindMessages = new();
            Plan plan = planData.plan[i];
            // concat all remind messages from all detectors
            remindMessages.AddRange(DetectDate(plan));
            remindMessages.AddRange(DetectNPCloc(plan));
            remindMessages.AddRange(DetectPartMatch(plan));
            remindMessages.AddRange(DetectSpecialDay(plan));
            remindMessages.AddRange(DetectPlanContained(plan));
            res.Add(remindMessages);
        }
        return res;
    }
    private static bool IsContainedByPlan(SDate date, Plan plan)
    {
        List<SDate> dates = TimeUtils.DateParse(plan.time, out _);
        if (date >= dates[0] && date <= dates[1])
        {
            return true;
        }
        return false;
    }                   
    private List<RemindMessage> DetectPlanContained(Plan plan)
    {
        List<RemindMessage> res = new();
        foreach (var npc in LoadData.NPCdata)
        {
            if (npc.Name == plan.npc)
            {
                //check if npc is in the plan
                SDate birthday = new(npc.Birthday_Day, npc.Birthday_Season);
                if (IsContainedByPlan(birthday, plan))
                {
                    res.Add(new RemindMessage(reminderContainBirthdayKey, new List<string> { npc.displayName, birthday.ToString() }, 0));
                }
                break;
            }
        }
        //festival
        foreach (var festival in LoadData.FestivalData)
        {
            string lettersPattern = @"[A-Za-z]+";
            string digitsPattern = @"\d+";

            string season = Regex.Replace(festival.Key, digitsPattern, "");
            int day = int.Parse(Regex.Replace(festival.Key, lettersPattern, ""));
            SDate festivalDate = new(day, season);
            if (IsContainedByPlan(festivalDate, plan))
            {
                res.Add(new RemindMessage(reminderContainFestivalKey, new List<string> { festival.Value, festivalDate.ToString() }, 0));
            }
        }
        foreach (var passiveFestival in LoadData.PassiveFestivalData)
        {
            SDate startDate = new(passiveFestival.Value.StartDay, passiveFestival.Value.Season);
            SDate endDate = new(passiveFestival.Value.EndDay, passiveFestival.Value.Season);
            if (IsContainedByPlan(startDate, plan))
            {
                res.Add(new RemindMessage(reminderContainPassiveFestivalKey, new List<string> { passiveFestival.Key, startDate.ToString(), endDate.ToString() }, 0));
            }
        }

        return res;
    }
    private List<RemindMessage> DetectDate(Plan plan)
    {
        //todo: detect time state: start tomorrow, start today, start in two hours, start now
        //end tomorrow, end today, end in two hours, end now
        //rainy tomorrow, rainy today, lucky today
        //festival tomorrow, festival today, birthday tomorrow, birthday today,
        List<RemindMessage> res = new();
        List<SDate> dates = TimeUtils.DateParse(plan.time, out int[] time);;
        string timeString = "";
        SDate now = SDate.Now();
        // compare current time with start time
        if (dates.Count > 0 && now == dates[0])
        {
            if (time[0] != 0)
            {
                timeString = TimeList.Hours[(time[0] - 600) / 100];
            }
            res.Add(new RemindMessage(reminderStartKey, timeString == ""? null : new List<string> { timeString }, time[0] == 600 ? 610 : time[0]));
            timeString = "";
        }
        if (dates.Count > 0 && now == dates[1])
        {
            if (time[1] != 0)
            {
                timeString = TimeList.Hours[(time[1] - 600) / 100];
            }
            res.Add(new RemindMessage(reminderEndKey, timeString == ""? null : new List<string> { timeString }, time[1] == 600 ? 610 : time[1]));
            timeString = "";
        }
        //repeat
        if (plan.repeat != "")
        {
            string[] repeatIndex = TimeUtils.RepeatParse(plan.repeat, out bool[] repeat);
            int[] nowParse = TimeUtils.SDate2Index4Repeat(now);
            bool isRepeatToday = true;
            foreach (bool b in repeat)
            {
                if (!b && nowParse[0] != int.Parse(repeatIndex[0]))
                {
                    isRepeatToday = false;
                    break;
                }
            }
            if (isRepeatToday)
            {
                if (time[0] != 0)
                {
                    timeString = TimeList.Hours[(time[0] - 600) / 100];
                }
                res.Add(new RemindMessage(reminderRepeatStartKey, timeString == "" ? null : new List<string> { timeString }, time[0] == 600 ? 610 : time[0]));
            }
        }
        //festival
        if (Utility.isFestivalDay() || Utility.IsPassiveFestivalDay())
        {
            res.Add(new RemindMessage(reminderFestivalKey, null, 610));
        }

        //birthday
        NPC birthdayNPC = Utility.getTodaysBirthdayNPC();
        if (birthdayNPC != null && birthdayNPC.Name == plan.npc)
        {
            res.Add(new RemindMessage(reminderBirthdayKey, new List<string> { birthdayNPC.displayName }, 0));
        }
        return res;
    }
    private List<RemindMessage> DetectNPCloc(Plan plan)
    {
        List<RemindMessage> res = new();
        //current location
        foreach (var npc in LoadData.NPCdata)
        {
            if (npc.Name == plan.npc)
            {
                res.Add(new RemindMessage(reminderNpcLocKey, new List<string> { npc.displayName, npc.currentLocation.GetDisplayName() }, 0));
            }
        }
        return res;
    }
    private List<RemindMessage> DetectPartMatch(Plan plan)
    {
        List<RemindMessage> res = new();
        if (plan.npc != "")
        {
            if (!modData.npc_data[plan.npc][1].Contains(plan.action))
            {
                res.Add(new RemindMessage(reminderActUnmatchKey, new List<string> { NPC.GetDisplayName(plan.npc), Translations.GetStr("ChooseAction", plan.action) }, 0));
            }
            if (!modData.npc_data[plan.npc][0].Contains(plan.location))
            {
                res.Add(new RemindMessage(reminderLocUnmatchKey, new List<string> { NPC.GetDisplayName(plan.npc), Translations.GetStr("ChooseLocation", plan.location) }, 0));
            }
        }
        return res;
    }
    private List<RemindMessage> DetectSpecialDay(Plan plan)
    {
        List<RemindMessage> res = new();
        string weatherToday = Game1.netWorldState.Value.GetWeatherForLocation("Default").Weather;
        double luck = Game1.player.DailyLuck;

        //weather and luck
        switch (weatherToday)
        {
            case Game1.weather_rain:
                if (plan.special == 1 || plan.special == 3)
                {
                    res.Add(new RemindMessage(reminderRainKey, null, 610));
                }
                break;
            default:
                if (plan.special == 0 || plan.special == 2)
                {
                    res.Add(new RemindMessage(reminderNorainKey, null, 610));
                }
                break;
        }
        if (luck > 0.02)
        {
            if (plan.special == 2 || plan.special == 3)
            {
                res.Add(new RemindMessage(reminderLuckyKey, null, 610));
            }
        }
        else
        {
            if (plan.special == 0 || plan.special == 1)
            {
                res.Add(new RemindMessage(reminderUnluckyKey, null, 610));
            }
        }
        return res;
    }

}

public sealed class RemindMessage
{
    string displayKey = "";
    List<string> additionalInfo = new();
    int remindTime = 0;
    public string DisplayKey { get => displayKey; set => displayKey = value; }
    public List<string> AdditionalInfo { get => additionalInfo; set => additionalInfo = value; }
    public int RemindTime { get => remindTime; set => remindTime = value; }

    public RemindMessage(string displayKey, List<string>? additionalInfo, int remindTime)
    {
        DisplayKey = displayKey;
        AdditionalInfo = additionalInfo ?? new List<string>();
        RemindTime = remindTime;
    }
    public static string GetRemindMessageDisplay(RemindMessage message)
    {
        return Translations.GetStr("ReminderMessage", message.DisplayKey, message.AdditionalInfo);
    }
    public string GetRemindMessageDisplay()
    {
        return Translations.GetStr("ReminderMessage", DisplayKey, AdditionalInfo);
    }
}