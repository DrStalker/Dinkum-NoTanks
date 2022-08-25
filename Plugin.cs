using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
//using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;


namespace NoTanks;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
        private static Harmony _harmony;
    
    private void Awake()
    {
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} starting patcher #############################");
        _harmony = Harmony.CreateAndPatchAll(typeof(NepNoTankPatch));
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded! #############################");
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
    }
}

[HarmonyPatch]
internal class NepNoTankPatch
{


    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SprinklerTile), nameof(SprinklerTile.waterTiles))]
    private static void NepNoTank(SprinklerTile __instance, int xPos, int yPos, List<int[]> waterTanks)
    {
        
        if (!__instance.isSilo)
        {
         	// Watering code copied from main game, but now it's not inside a check for tank distance.
            // This does mean if there is a tank the ground will be watered twice/extra growback checked twice.
            WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = 1;
			for (int j = -__instance.horizontalSize; j < __instance.horizontalSize + 1; j++)
			{
				for (int k = -__instance.verticlSize; k < __instance.verticlSize + 1; k++)
				{
                    //if there is a wet version, make it wet
					if (WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos + j, yPos + k]].wetVersion != -1)
					{
						WorldManager.manageWorld.tileTypeMap[xPos + j, yPos + k] = WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos + j, yPos + k]].wetVersion;
						WorldManager.manageWorld.chunkHasChangedToday[Mathf.RoundToInt((float)((xPos + j) / 10)), Mathf.RoundToInt((float)((yPos + k) / 10))] = true;
					}
                    //extra growback for using a sprinkler on land that is not tilled (e.g.: growing grass)
					if (WorldManager.manageWorld.onTileMap[xPos + j, yPos + k] == -1)
					{
						if (WorldManager.manageWorld.tileTypeMap[xPos + j, yPos + k] == 1)
						{
							WorldManager.manageWorld.onTileMap[xPos + j, yPos + k] = GenerateMap.generate.bushLandGrowBack.objectsInBiom[0].tileObjectId;
							WorldManager.manageWorld.chunkHasChangedToday[Mathf.RoundToInt((float)((xPos + j) / 10)), Mathf.RoundToInt((float)((yPos + k) / 10))] = true;
						}
						if (WorldManager.manageWorld.tileTypeMap[xPos + j, yPos + k] == 4)
						{
							WorldManager.manageWorld.onTileMap[xPos + j, yPos + k] = GenerateMap.generate.tropicalGrowBack.objectsInBiom[0].tileObjectId;
							WorldManager.manageWorld.chunkHasChangedToday[Mathf.RoundToInt((float)((xPos + j) / 10)), Mathf.RoundToInt((float)((yPos + k) / 10))] = true;
						}
						if (WorldManager.manageWorld.tileTypeMap[xPos + j, yPos + k] == 15)
						{
							WorldManager.manageWorld.onTileMap[xPos + j, yPos + k] = GenerateMap.generate.coldLandGrowBack.objectsInBiom[0].tileObjectId;
							WorldManager.manageWorld.chunkHasChangedToday[Mathf.RoundToInt((float)((xPos + j) / 10)), Mathf.RoundToInt((float)((yPos + k) / 10))] = true;
						}
					}
				}
			}
			if (NetworkMapSharer.share.isServer)
			{
                //I assume this is so the sprinklers keep working for the first few hours of the day
				WorldManager.manageWorld.sprinkerContinuesToWater(xPos, yPos);
				return;
			}

        }

    }

}

