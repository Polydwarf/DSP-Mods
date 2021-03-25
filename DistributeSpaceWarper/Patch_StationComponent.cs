using System;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace DistributeSpaceWarper
{
    static class Patch_StationComponent
    {
        [HarmonyPatch(typeof(PlanetTransport), "GameTick", typeof(long), typeof(bool))]
        [HarmonyPostfix]
        public static void GameTick_Postfix(PlanetTransport __instance)
        {
            int warperId = 1210;
            int IlsId = 2104;
            bool needRefreshTraffic = false;
            PrefabDesc prefabDesc = LDB.items.Select(IlsId).prefabDesc;
            int warperSlot = prefabDesc.stationMaxItemKinds;
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
                        storeCopy.Add(new StationStore(warperId, 0,0,0,100, ELogisticStorage.Demand, ELogisticStorage.None));
                        stationComponent.storage = storeCopy.ToArray();
                        needRefreshTraffic = true;
                    }
                    if (stationComponent.warperNecessary == true && stationComponent.storage[warperSlot].localLogic != ELogisticStorage.Demand)
                    {
                        stationComponent.storage[warperSlot].localLogic = ELogisticStorage.Demand;
                        needRefreshTraffic = true;
                    }
                    else if (stationComponent.warperNecessary == false && stationComponent.storage[warperSlot].localLogic != ELogisticStorage.Supply)
                    {
                        stationComponent.storage[warperSlot].localLogic = ELogisticStorage.Supply;
                        needRefreshTraffic = true;
                    }
                }
            }
            if (needRefreshTraffic)
            {
                __instance.RefreshTraffic();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationStorage), "OnItemPickerReturn", typeof(ItemProto))]
        public static bool OnItemPickerReturn_Prefix(UIStationStorage __instance, ItemProto itemProto)
        {
            int warperId = 1210;
            int IlsId = 2104;
            PrefabDesc prefabDesc = LDB.items.Select(IlsId).prefabDesc;
            int warperSlot = prefabDesc.stationMaxItemKinds;
            
            if (itemProto == null)
            {
                return false;
            }
            if (__instance.station == null || __instance.index >= __instance.station.storage.Length)
            {
                return false;
            }
            ItemProto itemProto2 = LDB.items.Select((int)__instance.stationWindow.factory.entityPool[__instance.station.entityId].protoId);
            if (itemProto2 == null)
            {
                return false;
            }
            for (int i = 0; i < __instance.station.storage.Length; i++)
            {
                if (__instance.station.storage[i].itemId == itemProto.ID)
                {
                    if (__instance.station.isStellar == true && __instance.station.isCollector == false && i == warperSlot && itemProto.ID == warperId)
                    {
                        continue;
                    }
                    UIRealtimeTip.Popup("不选择重复物品".Translate(), true, 0);
                    return false;
                }
            }
            __instance.stationWindow.transport.SetStationStorage(__instance.station.id, __instance.index, itemProto.ID, itemProto2.prefabDesc.stationMaxItemCount, ELogisticStorage.Supply, (!__instance.station.isStellar) ? ELogisticStorage.None : ELogisticStorage.Supply, GameMain.mainPlayer.package);
            return false;
        }
        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationWindow), "_OnCreate")]
        public static bool _OnCreatePrefix(UIStationWindow __instance, ref UIStationStorage[] ___storageUIs, UIStationStorage ___storageUIPrefab)
        {
            ___storageUIs = new UIStationStorage[5];
            for (int i = 0; i < 5; i++)
            {
                ___storageUIs[i] = UnityEngine.Object.Instantiate(___storageUIPrefab, ___storageUIPrefab.transform.parent);
                ((RectTransform) ___storageUIs[i].transform).anchoredPosition = new Vector2(40f, (float)(-90 - 76 * i));
                ___storageUIs[i].stationWindow = __instance;
                ___storageUIs[i]._Create();
            }
            return false;
        }
    }
}

