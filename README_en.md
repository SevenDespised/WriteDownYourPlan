# WriteDownYourPlan
[en](README_en.md) | [zh-cn](README_zh.md)
## Introduction
WriteDownYourPlan is a game tool that allows you to easily create plans and receive pop - up reminders when the plans start or end. In the plan menu, you can directly select NPCs, locations, and actions without manual input, and it will also provide you with some information about these elements. Custom text is also supported. <br>
It's perfect for players who like to plan in game! (Or those who always forget things.) <br>
In addition, the reminder board will provide you with a lot of other useful information! Such as festivals, birthdays, NPC locations, or weather. This depends on the content of the plan you choose. <br>

------------------------
------------------------
Currently, only Chinese and English are supported. Any support for translation is highly welcomed. <br>
Any feedback on bugs or features is also welcomed. You can leave messages on the following three platforms: <br>
nexusmod <br>
[github](https://github.com/SevenDespised/WriteDownYourPlan)
## Installation
1. Install smapi
2. Unzip the zip file to get the mod folder WriteDownYourPlan
3. Move it into Stardew Valley/Mods 
4. Launch the game using smapi
## Features
1. Plan Editor: You can create plans within the game and receive reminders when the plans start or end.
2. Content Selector: You can directly select game elements such as NPCs, locations, and actions without having to input them manually.
- Custom Text: You can customize text on the item interface to add additional information.
- Part Correlation: When you select an NPC, the available action and location options will change accordingly.
3. Time Selector: You can set the start and end times of the plan and choose to have repeated reminders.
4. Plan Reminder: After completing the plan selection, the mod will automatically organize the reminder text for you and display the start and end times.
5. Reminder Board: The reminder board will display information related to the plan, such as location, weather, festivals, etc. You can view the specific content in the [Message List](messages_en.txt). 
## Usage Tutorial
Using this mod is very simple. In most cases, you can use it smoothly without any help.
1. After starting the game, press F3 to open the plan menu (you can customize the shortcut key in the config.json file, but please continue to use F1 - F12. **Using other keys may cause the menu to close unexpectedly during text input**).
2. Click on a blank plan bar, and you will enter the plan editing page. First, edit a concise title for the plan. Then, on this page, click the edit keys for parts such as NPCs, locations, etc., and you will enter the content selection page.
3. **At present, if you have selected one of these parts, except for NPCs, you cannot cancel it, only change it.** You can deselect an NPC by clicking the same avatar again: This is to reduce useless information on the reminder board. If you don't want to change the NPC, you can click the black area to return.
4. On the time selection page, you can choose the start and end times of the plan. When the time you selected arrives, the game will pop up a reminder in the lower - left corner. If you choose repeated reminders, before the plan deadline, starting from the start date, it will remind you at the corresponding time every day/week/season/year. On this page, you can configure additional reminder conditions. Currently, only luck and whether it's raining are included, and more conditions are planned for future updates.
5. On the item interface, you can use custom text to supplement information. **Note that it has no practical meaning, just text.**
6. After completing all selections, click the OK button to save the plan, and click the return button to abandon saving the plan. **You must select at least one item other than the time (including the title) to save it normally.**
7. After completing the plan editing, hover your mouse over your plan, and the mod will automatically organize the text and display the start and end times. Click the delete button on the right to delete the plan. **Be careful, once the plan is deleted, it cannot be restored.**

Now you have created one of your plans. Enjoy your game!
## In Addition
- The default maximum number of plans is 20. You can modify it by changing "MaxPlan" in config.json. It is recommended to modify it to a multiple of 5.
- You can change "DisplayHUDMessage": true to "DisplayHUDMessage": false in config.json to turn off the pop - up messages in the lower - left corner.
- Plan content is saved in your game save file. It will only be saved when you go to bed and the game shows that it has been saved. If you close the game, your plans for the day will be lost.

However, the mod also provides another method: You can use left shift + F3 to save the plan content to data/your_farm_name.json, but it will not read from it.
If you set ReadDatatFromJson to true in config.json
> "ReadDataFromJson": true

After restarting the game, the mod will read the plan content from the json file instead of the save file. If smapi does not prompt you "model is loaded", then the setting is successful. In this way, you can manually modify the plans outside the game. <br>
If you want to return to the normal mode, change ReadDatatFromJson back to false and restart the game. The plan content during this period is still saved to the save file normally (just not read). <br>
This method is for debugging purposes. It is highly not recommended. Please make sure you know exactly what you are doing. 