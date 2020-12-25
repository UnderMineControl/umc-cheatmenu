# Cheat Menu
This repo contains the code the Cheat Menu mod for [UMC](https://github.com/calico-crusade/underminecontrol).

## Key Binds / Options
Unfortunately, the key binds are not configurable at the moment. I hope to add this in the future
* ```F1``` Opens the menu
* Toggle Doors: Opens or closes all doors in the current room, regardless of enemies
* God Mode: Makes you invulnerable
* Max Health: Sets your max health
* Current Health: Sets your current health
* Bombs: Type a number to add that many bombs (will not remove bombs)
* Keys: Type a number to add that many keys (will not remove keys)
* Gold: Type a number to add that much gold (will not remove gold)
* Thorium: Type a number to add that much Thorium (will not remove Thorium)
* Give Curse: Apply a random curse
* Give Blessing: Apply a random blessing. Blessings must be unlocked first
* Remove Curse: Remove a random curse
* Spawn Relic: Spawn a random relic from the pool (must be unlocked and not already seen in this run)
* Print equipment: Prints all your equipment to the console (console must be enabled for this to be useful)
* Print All Entities: Currently broken/does nothing
* Entity Name/Spawn Enemy: Spawn a specific enemy by name (broken in 1.1.0)
* Relic Name/Spawn Relic: Spawn a relic by name or id (broken in 1.1.0)
* Potion Name/Spawn Potion: Spawn a potion by name or id (broken in 1.1.0)

## Manual Installation
Steps to install the cheat menu mod:
Use the [UMC Loader](https://github.com/UnderMineControl/underminecontrol-loader)

## Building this project
You will need to manually reference some libraries. Unfortunately, I cannot provide them in the github due to legal reasons.

The following libraries need to be referenced and can be found in ```steamapps\common\UnderMine\UnderMine_Data\Managed```
* BehaviorDesigner.dll
* BehaviorDesigner.Runtime.dll
* UnderMine.dll
* UnityEngine.CoreModule.dll
* UntiyEngine.dll

You will also need to reference the ```UnderMineControl.API.dll``` that can either be found [here](https://github.com/calico-crusade/underminecontrol/releases) or in the following directory if you have [UnderMineControl](https://github.com/calico-crusade/underminecontrol) installed: ```steamapps\common\UnderMine\BepInEx\plugins\UnderMineControl```
