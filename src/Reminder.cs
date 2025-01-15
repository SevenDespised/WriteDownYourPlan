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

    private bool IsFestivalDetected = false;

    public Reminder(PlanData planData, ModData modData)
    {
        this.planData = planData;
        this.modData = modData;
    }
    
    public void Draw(SpriteBatch b, int x, int y, int width, int height, int planIndex)
    {
        if (remindMessages == null || planIndex >= remindMessages.Count ||remindMessages[planIndex].Count == 0)
        {
            return;
        }
        DrawReminder.Draw(b, x, y, width, height, remindMessages[planIndex]);
    }
    public void InitReminder()
    {
        InitAllFlags();
        remindMessages = GetAllRemindMessages();
    }
    internal void UpdateReminder()
    {
        InitAllFlags();
        remindMessages = GetAllRemindMessages();
    }
    public void DeleteReminder()
    {
        remindMessages = null;
    }
    private void InitAllFlags()
    {
        IsFestivalDetected = false;
    }
    private List<List<RemindMessage>>? GetAllRemindMessages()
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
        return res.Count == 0 ? null : res;
    }
    //
    private static bool IsContainedByPlan(SDate date, List<SDate> dates)
    {
        if (dates == null)
        {
            return false;
        }
        //SDate start = dates[1] >= SDate.Now() && SDate.Now() >= dates[0] ? SDate.Now() : dates[0];
        if (date >= dates[0] && date <= dates[1])
        {
            return true;
        }
        return false;
    }                   
    private List<RemindMessage> DetectPlanContained(Plan plan)
    {
        List<RemindMessage> res = new();
        List<SDate>? dates = TimeUtils.DateParse(plan.time, out _);
        //if plan has started but not ended, use nowToEndDates to take place dates
        List<SDate>? nowToEndDates = null;
        if (dates == null)
        {
            return res;
        }
        if(dates[1] < SDate.Now())
        {
            //res.Add(new RemindMessage());
            return res;
        }
        if(dates[0] < SDate.Now())                                                                      
        {
            nowToEndDates = new List<SDate> { SDate.Now(), dates[1] };
        }
        foreach (var npc in LoadData.NPCdata)
        {
            if (npc.Name == plan.npc)
            {
                //check if npc is in the plan
                SDate birthday = new(npc.Birthday_Day, npc.Birthday_Season);
                List<SDate> datesForCheck = nowToEndDates?? dates;
                if (IsContainedByPlan(birthday, datesForCheck) || IsContainedByPlan(birthday, datesForCheck.Select(d => new SDate(d.Day, d.Season, d.Year + 1)).ToList()))
                {
                    res.Add(new RemindMessage(reminderContainBirthdayKey, new { npc = npc.displayName, date =  birthday.ToLocaleString(withYear: false) }, 0));
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
            SDate festivalDate = new(day, season, SDate.Now().Year > dates[0].Year ? SDate.Now().Year : dates[0].Year);
            //fist check year, second check (year + 1)
            List<SDate> datesForCheck = nowToEndDates?? dates;
            if (IsContainedByPlan(festivalDate, datesForCheck) || IsContainedByPlan(festivalDate, datesForCheck.Select(d => new SDate(d.Day, d.Season, d.Year + 1)).ToList()))
            {
                res.Add(new RemindMessage(reminderContainFestivalKey, new { festival = festival.Value, date = festivalDate.ToLocaleString(withYear: false) }, 0));
                break;            
            }
        }
        foreach (var passiveFestival in LoadData.PassiveFestivalData)
        {
            SDate startDate = new(passiveFestival.Value.StartDay, passiveFestival.Value.Season);
            SDate endDate = new(passiveFestival.Value.EndDay, passiveFestival.Value.Season);
            //fist check year, second check year + 1 
            List<SDate> datesForCheck = nowToEndDates?? dates;
            if (IsContainedByPlan(startDate, datesForCheck) || IsContainedByPlan(startDate, datesForCheck.Select(d => new SDate(d.Day, d.Season, d.Year + 1)).ToList()))
            {
                res.Add(new RemindMessage(reminderContainPassiveFestivalKey, new { festival = passiveFestival.Value.DisplayName, date = startDate.ToLocaleString(withYear: false), enddate = endDate.ToLocaleString(withYear: false) }, 0));
                break;
            }
        }

        return res;
    }
    private List<RemindMessage> DetectDate(Plan plan)
    {
        List<SDate>? dates = TimeUtils.DateParse(plan.time, out int[] time);
        List<RemindMessage> res = new();
        if (dates == null)
        {
            return res;
        }
        string timeString = "";
        SDate now = SDate.Now();
        // compare current time with start time
        if (now == dates[0])
        {
            if (time[0] != 0)
            {
                timeString = TimeList.Hours[(time[0] - 600) / 100];
            }
            res.Add(new RemindMessage(reminderStartKey, timeString == ""? null : new { time = timeString }, time[0] == 600 ? 610 : time[0]));
            timeString = "";
        }
        if (now == dates[1])
        {
            if (time[1] != 0)
            {
                timeString = TimeList.Hours[(time[1] - 600) / 100];
            }
            res.Add(new RemindMessage(reminderEndKey, timeString == ""? null : new { time = timeString }, time[1] == 600 ? 610 : time[1]));
            timeString = "";
        }
        //repeat
        if (plan.repeat != -1)
        {
            //year, month, week, day 
            int[] intervals = new int[4]{112, 28, 7, 1};
            int interval = now.DaysSinceStart - dates[0].DaysSinceStart;
            //check if today date is less than or equal to end,  greater than start(not equal, when today is equal to plan start day, add "remindStartKey")
            if (now <= dates[1] && interval > 0 && interval % intervals[plan.repeat] == 0)
            {
                if (time[0] != 0)
                {
                    timeString = TimeList.Hours[(time[0] - 600) / 100];
                }
                res.Add(new RemindMessage(reminderRepeatStartKey, timeString == "" ? null : new { time = timeString }, time[0] == 600 ? 610 : time[0]));
            }
        }
        //festival
        if (Utility.isFestivalDay() || Utility.IsPassiveFestivalDay())
        {
            if (IsFestivalDetected)
            {   
                res.Add(new RemindMessage(reminderFestivalKey, null, 0));
            }
            else
            {
                IsFestivalDetected = true;
                res.Add(new RemindMessage(reminderFestivalKey, null, 630));
            }
        }
        else
        {
            IsFestivalDetected = false;
        }

        //birthday
        NPC birthdayNPC = Utility.getTodaysBirthdayNPC();
        if (birthdayNPC != null && birthdayNPC.Name == plan.npc)
        {
            res.Add(new RemindMessage(reminderBirthdayKey, new { npc = birthdayNPC.displayName }, 0));
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
                res.Add(new RemindMessage(reminderNpcLocKey, new { npc = npc.displayName, location = npc.currentLocation.GetDisplayName() }, 0));
            }
        }
        return res;
    }
    private List<RemindMessage> DetectPartMatch(Plan plan)
    {
        List<RemindMessage> res = new();
        if (plan.npc != "")
        {
            if (plan.action != "" && !modData.npc_data[plan.npc][1].Contains(plan.action))
            {
                res.Add(new RemindMessage(reminderActUnmatchKey, new { npc = NPC.GetDisplayName(plan.npc), action = Translations.GetStr("ChooseAction", plan.action) }, 0));
            }
            if (plan.location != "" && !modData.npc_data[plan.npc][0].Contains(plan.location))
            {
                res.Add(new RemindMessage(reminderLocUnmatchKey, new { npc =  NPC.GetDisplayName(plan.npc), location = Translations.GetStr("ChooseLocation", plan.location) }, 0));
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
        int[] stateIndex = TimeUtils.DecodeSpecialDate(plan.special);
        switch (weatherToday)
        {
            case Game1.weather_rain:
                if (stateIndex[0] == 2)
                {
                    res.Add(new RemindMessage(reminderRainKey, null, 630));
                }
                break;
            default:
                if (stateIndex[0] == 1)
                {
                    res.Add(new RemindMessage(reminderNorainKey, null, 630));
                }
                break;
        }
        if (luck > 0.02)
        {
            if (stateIndex[1] == 1)
            {
                res.Add(new RemindMessage(reminderLuckyKey, null, 630));
            }
        }
        else
        {
            if (stateIndex[1] == 2)
            {
                res.Add(new RemindMessage(reminderUnluckyKey, null, 630));
            }
        }
        return res;
    }

}

public sealed class RemindMessage
{
    string displayKey = "";
    object? additionalInfo;
    int remindTime = 0;
    public string DisplayKey { get => displayKey; set => displayKey = value; }
    public object? AdditionalInfo { get => additionalInfo; set => additionalInfo = value; }
    public int RemindTime { get => remindTime; set => remindTime = value; }

    public RemindMessage(string displayKey, object? additionalInfo, int remindTime)
    {
        DisplayKey = displayKey;
        AdditionalInfo = additionalInfo;
        RemindTime = remindTime;
    }
    public static string GetRemindMessageDisplay(RemindMessage message)
    {
        return message.AdditionalInfo == null ? Translations.GetStr("ReminderMessage", message.DisplayKey) : Translations.GetStr("ReminderMessage", message.DisplayKey, message.AdditionalInfo);
    }
    public string GetRemindMessageDisplay()
    {
        return AdditionalInfo == null ? Translations.GetStr("ReminderMessage", DisplayKey) : Translations.GetStr("ReminderMessage", DisplayKey, AdditionalInfo);
    }
}