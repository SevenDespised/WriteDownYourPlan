using System;
using System.Reflection.Metadata;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace WritedownYourPlan.src;

public class ModEntry: Mod
{
    public static IModHelper Helper1 {get; internal set;} = null!;
    public static IMonitor Monitor1 {get; internal set;} = null!;
    private ModConfig? Config;
    public PlanData? planData = null;
    public ModData? modData;
    public Reminder? reminderForHud;
    //以下用于存储数据和调试
    public TestCla1 cla = new();

    public override void Entry(IModHelper helper)
    {
        Config = Helper.ReadConfig<ModConfig>();
        Monitor.Log("Config is read!", LogLevel.Debug);
        Helper1 = helper;
        Monitor1 = Monitor;

        helper.Events.Input.ButtonPressed += OnButtonPressed;
        helper.Events.Display.WindowResized += OnWindowResized;
        helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
        helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.Display.MenuChanged += OnMenuChanged;
        helper.Events.GameLoop.Saving += OnSaving;
        helper.Events.GameLoop.Saved += OnSaved;
        helper.Events.GameLoop.DayStarted += OnDayStarted;
        helper.Events.GameLoop.TimeChanged += OnTimeChanged;
        //todo:hudmessage
        //Constants.SaveFolderName;
        //以下用于存储数据和调试
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    { 
        reminderForHud?.UpdateReminder();
        Monitor.Log("Reminder is updated!(DayStart)", LogLevel.Debug);
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        //model = Helper.Data.ReadJsonFile<PlanData>("data/plandata.json") ?? new PlanData();
        modData = Helper.Data.ReadJsonFile<ModData>("data/data.json") ?? new ModData();
        if(Context.IsMainPlayer)
        {
            //DeleteSaveData();
            planData = Helper.Data.ReadSaveData<PlanData>("plandata") ?? new PlanData();
            Monitor.Log("Model is loaded!", LogLevel.Debug);
            reminderForHud = new Reminder(planData, modData);
            Monitor.Log("Reminder is initialized!(host)", LogLevel.Debug);
        }
        TimeList.Init();
    }
    private void OnWindowResized(object? sender, WindowResizedEventArgs e)
    {
        if (Game1.activeClickableMenu is WypMenu)
            ShowWypMenu();
    }
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        if ((Config ?? new ModConfig()).DisplayWYPButton.JustPressed())
        {
            ToggleMenu();
        }
        else if ((Config ?? new ModConfig()).SaveModelButton.JustPressed() && Game1.activeClickableMenu is WypMenu)
        {
            SaveDataToJson();
            ToggleMenu();
        }

        //以下用于存储数据和调试
        else if (e.Button == SButton.F5 && Game1.activeClickableMenu is WypMenu)
        {
            Debug();
            cla.res = (Dictionary<string, HashSet<string>>)Test();
            //WriteJson("data/npc.json", cla);
            //HideWypMenu();
        }
    }
    private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
    {
        if (Context.IsMainPlayer)
        {
            PlanData message = Helper.Data.ReadSaveData<PlanData>("plandata" + e.Peer.PlayerID.ToString()) ?? new PlanData();
            Helper.Multiplayer.SendMessage(message, "PlanData", modIDs: new[] { ModManifest.UniqueID }, playerIDs: new[] { e.Peer.PlayerID });
            Monitor.Log("Sent PlanData to " + e.Peer.PlayerID.ToString(), LogLevel.Debug);
        }
    }
    private void OnSaving(object? sender, SavingEventArgs e)
    {
        if (!Context.IsMainPlayer)
        {
            Helper.Multiplayer.SendMessage(planData, "PlanData", modIDs: new[] { ModManifest.UniqueID }, playerIDs: new[] { Game1.MasterPlayer.UniqueMultiplayerID });
            Monitor.Log("Sent PlanData to host", LogLevel.Debug);
        }
        else
        {
            SaveData();
        }
    }
    private void OnSaved(object? sender, SavedEventArgs e)
    {
        if (Context.IsMainPlayer)
        {
            //SaveData();
        }
    }
    private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
        if (Context.IsMainPlayer)
        {
            if (e.FromModID == ModManifest.UniqueID && e.Type == "PlanData")
            {
                Helper.Data.WriteSaveData("plandata" + e.FromPlayerID.ToString(), e.ReadAs<PlanData>());
                Monitor.Log("Received PlanData from " + e.FromPlayerID.ToString(), LogLevel.Debug);
            }
        }
        else
        {
            if (e.FromModID == ModManifest.UniqueID && e.Type == "PlanData")
            {
                planData ??= e.ReadAs<PlanData>();
                Monitor.Log("Received PlanData from host", LogLevel.Debug);
                if (modData != null)
                {
                    reminderForHud = new Reminder(planData, modData);
                    Monitor.Log("Reminder is initialized!(client)", LogLevel.Debug);
                }
            }
        }
    }
    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // when closing wypmenu, update reminder
        if (e.OldMenu is WypMenu)
        {
            reminderForHud?.UpdateReminder();
            Monitor.Log("Reminder is updated!(menuclosed)", LogLevel.Debug);
        }
    }
    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        if (reminderForHud == null || reminderForHud.RemindMessages == null || !(Config ?? new ModConfig()).DisplayHUDMessage)
        {
            return;
        }
        for (int i = 0; i < reminderForHud.RemindMessages.Count; i++)
        {
            var remindeMessage = reminderForHud.RemindMessages[i];
            foreach (var message in remindeMessage)
            {
                if (message.RemindTime != 0)
                {
                    if (Game1.timeOfDay == message.RemindTime || (Game1.timeOfDay == 610 && message.RemindTime == 600))
                    {
                        string hudText = Translations.GetStr("ReminderMessage", "PlanNum", new {num = i + 1}) + " " + message.GetRemindMessageDisplay();
                        Game1.addHUDMessage(new HUDMessage(hudText, HUDMessage.newQuest_type));
                    }
                }
            }
        }
    }
    private void ToggleMenu()
    {
        if (Game1.activeClickableMenu is WypMenu)
        {
            HideWypMenu();
        }
        else if (Game1.activeClickableMenu == null && Context.IsPlayerFree)
            ShowWypMenu();
    }
    private void HideWypMenu()
    {
        if (Game1.activeClickableMenu is WypMenu)
        {
            Game1.playSound("bigDeSelect"); // match default behaviour when closing a menu
            Game1.activeClickableMenu = null;
        }
    }
    private void ShowWypMenu()
    {
        Game1.activeClickableMenu = new WypMenu(planData ?? new PlanData(), modData ?? new ModData(), Config ?? new ModConfig());
    }
    public void SaveData()
    {
        //Helper.Data.WriteSaveData<PlanData>("plandata", null);
        Helper.Data.WriteSaveData("plandata", planData);
    }
    public void SaveDataToJson()
    {
        Helper.Data.WriteJsonFile("data/" + Constants.SaveFolderName + ".json", planData);
    }
    public void DeleteSaveData()
    {
        Helper.Data.WriteSaveData<ModData>("plandata", null);
    }




    //以下用于存储数据和调试
    private void Debug()
    {
        modData = Helper.Data.ReadJsonFile<ModData>("data/data1.json") ?? new ModData();
        //Monitor.Log(data.location_data["Mine"]);
    }
    public object Test()
    {
        WypMenu wypMenu = new(planData ?? new PlanData(), modData ?? new ModData(), Config ?? new ModConfig());
        Dictionary<string, HashSet<string>> res = new();
        Dictionary<string, string> schedule = new();

        foreach (KeyValuePair<string, StardewValley.GameData.Characters.CharacterData> kv in LoadData.CharacterData)
        {
            res.Add(kv.Key, new HashSet<string>());
            try
            {
                schedule = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + kv.Key);
            }
            catch (Exception)
            {
                continue;
            }
            foreach (var item in schedule)
            {
                foreach(string str in item.Value.Split('/'))
                {
                    string[] tmp = str.Split(" ");
                    if (int.TryParse(tmp[0], out int num))
                        res[kv.Key].Add(tmp[1]);
                }
            }
        }
        return res.OrderBy(x => x.Key).ToDictionary(p => p.Key, o => o.Value);
    }
}
