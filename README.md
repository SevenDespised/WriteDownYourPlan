# WriteDownYourPlan
## 简介
WriteDownYourPlan是一个在游戏内使用的工具，你可以用它方便地制定计划，并在计划开始或结束时弹出提醒。同时，在计划菜单内可以直接选择NPC、地点、行为而无需手动输入，并且会提示你有关这些内容的一些信息，当然也支持自定义文本。<br>
很适合喜欢在游戏内做规划的玩家！（或者是总是忘记事情的玩家。）<br>
除此之外，提示板还会为你提示很多额外的有用信息！比如节日、生日、NPC的位置或者天气。这取决于你选择了什么样的计划内容。<br>

------------------------
------------------------
现在仅支持中文和英文，任何翻译上的支持都非常欢迎。<br>
任何bug或功能上的反馈都非常欢迎，你可以在以下三个平台中留言。<br>
nexusmod<br>
[github](https://github.com/SevenDespised/WriteDownYourPlan)
## 安装
1.安装smapi<br>
2.解压zip文件，得到mod文件夹WriteDownYourPlan<br>
3.移入Stardew Valley/Mods<br>
4.使用smapi启动游戏<br>
## 使用教程
使用此mod是非常简单的，绝大多数情况下你不需要任何帮助也可以顺利使用。
1. 在游戏开始后，按F3打开计划菜单(你可以在config.json文件中自定义快捷键，但请继续使用F1-F12,**使用其它按键可能导致文本输入时意外关闭菜单**)。
2. 点击空白计划栏，你将进入计划编辑页面，首先为计划编辑一个简明的标题，然后在这个页面分别点击NPC、地点等部分的编辑键，你将进入内容选择页面。
3. **目前，若你已经选择了其中的某个部分，除了NPC以外均不能取消，只能更改。**NPC可以通过重新点击相同的头像取消选中：这是为了减少提示板上无用的信息。若不想更改npc，可以通过点击黑色区域返回。
4. 在时间选择页面，你可以选择计划起止时间，在到达你所选择的时间后，游戏将会在左下角弹出提醒；若选择重复提醒，将在计划截止日期前，从开始日期开始，每天/周/季/年的相应时间提醒。在此页面，你可以配置额外的提醒条件，目前仅包括运气与是否下雨，更多条件已在未来更新计划中。
5. 在物品界面，你可以使用自定义文本补充信息，**注意它没有实际的意义，仅仅只是文本。**
6. 在完成所有选择后，点击OK按钮保存计划，点击返回按钮放弃保存计划。**你至少要选择除了时间以外的任意一项（包括标题），才能正常保存。**
7. 完成计划编辑后，将鼠标悬停在你的计划上，mod将自动为你组织文本，并显示起止时间。点击右边的删除按钮可以删除计划，**请小心，计划删除后不能恢复。**

现在你制定了你的一个计划，享受你接下来的游戏吧！
## 除此之外
- 计划默认最大数量为20，可以通过更改config.json中"MaxPlan"来修改，请尽量修改为5的倍数。
- 可以通过更改config.json中的"DisplayHUDMessage": true改为"DisplayHUDMessage": false，可以关闭左下角的弹出消息。
- 计划内容是保存在你的存档里的，只有当你上床睡觉，显示游戏已存档后才会保存；若你关闭了游戏，你当天的计划也会丢失。

但mod也提供了另一种方法：你可以使用左shift+F3将计划内容保存至data/你的农场名.json，但不会从中读取。
若你将config.json中ReadDatatFromJson设置为true
> "ReadDataFromJson": true

重启游戏后，mod将从json文件中读取计划内容，而非存档文件。如果smapi没有提示你"model is loaded"，那么设置就成功了。通过这种方式，你可以在游戏外手动地修改计划。<br>
若你想要回到正常的模式，请将ReadDatatFromJson改回true并重启游戏。这期间的计划内容也是正常保存至存档的（只是没有被读取）。<br>
这种方法是为了调试使用的，非常不推荐这样做，请确保你清楚地知道你在做什么。
