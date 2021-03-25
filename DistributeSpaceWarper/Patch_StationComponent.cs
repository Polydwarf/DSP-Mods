using HarmonyLib;
using System.Collections.Generic;

namespace DistributeSpaceWarper
{
    static class Patch_StationComponent
    {
        [HarmonyPatch(typeof(PlanetTransport), "GameTick", typeof(long), typeof(bool))]
        [HarmonyPostfix]
        public static void GameTick_Postfix(PlanetTransport __instance)
        {
            ELogisticStorage defaultLocalMode = Config.General.WarperLocalMode.Value;
            ELogisticStorage defaultRemoteMode = Config.General.WarperRemoteMode.Value;
            int defaultMaxValue = Config.General.WarperMaxValue.Value;
            int warperId = 1210;
            bool needRefreshTraffic = false;
            PrefabDesc prefabDesc = LDB.items.Select(2104).prefabDesc;
            for (int j = 1; j < __instance.stationCursor; j++)
            {
                if (__instance.stationPool[j] != null && __instance.stationPool[j].id == j)
                {
                    StationComponent stationComponent = __instance.stationPool[j];
                    if (stationComponent.isCollector == true || stationComponent.isStellar == false)
                    {
                        continue;
                    }
                    if (stationComponent.storage.Length < prefabDesc.stationMaxItemKinds + 1)
                    {
                        List<StationStore> storeCopy = new List<StationStore>(stationComponent.storage);
                        storeCopy.Add(new StationStore(warperId, 0,0,0,defaultMaxValue, 
                            defaultLocalMode, defaultRemoteMode));
                        for (int index = 0; index < storeCopy.Count - 1; index++)
                        {
                            if (storeCopy[index].itemId == warperId)
                            {
                                storeCopy[storeCopy.Count - 1] = storeCopy[index];
                                storeCopy[index] = default(StationStore);
                            }
                        }
                        stationComponent.storage = storeCopy.ToArray();
                        needRefreshTraffic = true;
                    }
                    else if (stationComponent.storage[stationComponent.storage.Length - 1].itemId != warperId)
                    {
                        stationComponent.storage[stationComponent.storage.Length - 1] = new StationStore(warperId, 0, 0, 0, defaultMaxValue,
                            defaultLocalMode, defaultRemoteMode);
                        needRefreshTraffic = true;
                    }
                }
            }
            if (needRefreshTraffic)
            {
                __instance.RefreshTraffic();
            }
        }
    }
}

