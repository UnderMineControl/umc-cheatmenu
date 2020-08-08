# Cheat Menu
This repo contains the code the Cheat Menu mod for [UMC](https://github.com/calico-crusade/underminecontrol).

## Key Binds / Options
Unfortunately, the key binds are not configurable at the moment. I hope to add this in the future
* ```F1``` Opens all doors in the current room
* ```F2``` Closes all doors in the current room
* ```F3``` Makes your character invulnerable (God Mode) 
	* You still take damage when you sacrifice your health at alters or shops or levers.
* ```F4``` Sets your max and current HP to 1500
* ```F5``` Gives you a random blessing
* ```F6``` Removes a random curse
* ```F7``` Gives your 99 bombs and keys, 999 Thorium, and 10000 gold.
* ```F8``` Doubles your current gold
* ```F9``` Dumps all items, effects and enemies to CSV files on your desktop (game might freeze for a bit)

## Manual Installation
Steps to install the cheat menu mod:
1. Create the following folder: ```steamapps\common\UnderMine\Mods\CheatMenu```
2. Download the [latest release](https://github.com/calico-crusade/umc-cheatmenu/releases) of the mod. 
3. Extract the contents of the .zip file into the folder you created.

## Building this project
You will need to manually reference some libraries. Unfortunately, I cannot provide them in the github due to legal reasons.

The following libraries need to be referenced and can be found in ```steamapps\common\UnderMine\UnderMine_Data\Managed```
* BehaviorDesigner.dll
* BehaviorDesigner.Runtime.dll
* UnderMine.dll
* UnityEngine.CoreModule.dll
* UntiyEngine.dll

You will also need to reference the ```UnderMineControl.API.dll``` that can either be found [here](https://github.com/calico-crusade/underminecontrol/releases) or in the following directory if you have [UnderMineControl](https://github.com/calico-crusade/underminecontrol) installed: ```steamapps\common\UnderMine\BepInEx\plugins\UnderMineControl```