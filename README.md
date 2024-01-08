# The Wheel

> With each turn its cilia pulse and wriggle and its body flushes translucent to crimson. It might be ugly but it is beautiful like the withdrawing of blood from the labyrinths of glass. It does not cease and all its involutions are infinite. The Wheel.

The Wheel is a time control mod for [Book of Hours](https://store.steampowered.com/app/1028310/BOOK_OF_HOURS/), a 2023 game by Weather Factory. It's a port of my [mod of the same name](https://github.com/KatTheFox/The-Wheel) for [Cultist Simulator](https://store.steampowered.com/app/718670/Cultist_Simulator), another Weather Factory game.

## What does it do?

The Wheel adds 3 new sliders to the options menu, which lets you control how fast each of the various speed options are in-game. It also adds the ability to bind keys to skip forward in time by 1 second, 10 seconds, or until the next recipe completion.

# Installation instructions

- You will need to install BepInEx for this to work. Instructions on how to do that can be found [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html). Your Book of Hours game directory can be found by right clicking the game in steam and clicking 'browse local files'
- In your BepInEx config file (Book Of Hours/BepInEx/config/BepInEx.cfg) find the line that says `HideManagerGameObject = false` and change it to `HideManagerGameObject = true`.
- Extract the contents of TheWheelBoh.tar.gz to `Book of Hours/BepInEx/plugins`, when you're done you should have a `TheWheel` folder in the plugins folder
- Find the Book Of Hours content directory, which will be either `Book of Hours/bh_Data/StreamingAssets/bhcontent/core` (on Linux or Windows) or `Book of Hours/OSX.app/Contents/Resources/Data/StreamingAssets/bhcontent/core/` (on Mac)
- You should see a `settings` folder. Put the `thewheelsettings.json` file inside it.
- Open the `culture_append.txt` file in any text editor, and copy the contents. Then open `bhcontent/core/cultures/en/culture.json` in a text editor and paste the contents of `culture_append.txt` right after the last line of text (before the 4 lines that are just brackets) and save it.
- If you've done all that correctly, 3 sliders should appear in the rightmost tab of the settings menu, and 3 new keybind options should appear in the controls menu.

### To uninstall the mod

Delete the `The Wheel` folder from your BepInEx plugins directory. Delete `thewheelsettings.json` from the `bhcontent/core/settings` directory. Remove the lines pertaining to The Wheel from the `bhcontent/core/cultures/en/culture.json` file.

### The game updated and now all the settings show up as 'MISSINGUI_something!'

If an update to the game overwrites the `culture.json` file, you'll need to re-add the lines pertaining to The Wheel's settings. Yes, it sucks, but remember that modding isn't officially supported so it could be much worse.

## Disclaimers

Modding is not officially supported in Book of Hours. Do not contact Weather Factory with bugs resulting from the use of this mod. As the game updates, expect potential breakages. I am not affiliated with Weather Factory in any way.

The Wheel is unofficial content based on Book of Hours by Weather Factory Ltd. You can find out more and support Book of Hours at [www.weatherfactory.biz](https://www.weatherfactory.biz).

<img src="https://weatherfactory.biz/wp-content/uploads/2022/11/sixth-history-logo-text-black.png" width="120" /> 
