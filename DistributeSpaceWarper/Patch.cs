using System;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;


namespace DistributeSpaceWarper
{
    static class Patch
    {
        private static bool ModDisabled => Config.Utility.DisableMod.Value == true || Config.Utility.UninstallMod.Value == true;
        private static int _vanillaILSID = 2104;
        private static int _gigastationILSID = 2702;

        private static List<int> StationIDs = new List<int>()
        {
            _vanillaILSID,
            //_gigastationILSID
        };


        [HarmonyPatch(typeof(PlanetTransport), "Import", typeof(BinaryReader))]
        [HarmonyPostfix]
        public static void PlanetTransport_Import_Postfix(BinaryReader r, PlanetTransport __instance)
        {
            if (Config.Utility.UninstallMod.Value == true)
            {
                int warperId = ItemProto.kWarperId;
                PrefabDesc prefabDesc = LDB.items.Select(_vanillaILSID).prefabDesc;
                UninstallMod(__instance, prefabDesc, warperId);
            }
        }


        private static void UninstallMod(PlanetTransport instance, PrefabDesc prefabDesc, int warperId)
        {
            int warperSlotIndex = prefabDesc.stationMaxItemKinds;
            for (int j = 1; j < instance.stationCursor; j++)
            {
                if (instance.stationPool[j] != null && instance.stationPool[j].id == j)
                {
                    StationComponent stationComponent = instance.stationPool[j];
                    if (stationComponent.isCollector == true || stationComponent.isStellar == false)
                    {
                        continue;
                    }
                    if (stationComponent.storage.Length > prefabDesc.stationMaxItemKinds
                        && stationComponent.storage.Last().itemId == warperId)
                    {
                        instance.SetStationStorage(
                            stationComponent.id,
                            warperSlotIndex,
                            0, 0,
                            ELogisticStorage.None,
                            ELogisticStorage.None,
                            GameMain.mainPlayer);
                        List<StationStore> storeCopy = new List<StationStore>(stationComponent.storage);
                        storeCopy.RemoveAt(warperSlotIndex);
                        stationComponent.storage = storeCopy.ToArray();
                    }
                    instance.RefreshTraffic();
                    instance.gameData.galacticTransport.RefreshTraffic(stationComponent.gid);
                    stationComponent.UpdateNeeds();
                }
            }
        }

        static bool bDoneLDBDump = false;

        private static void DoHiddenItemAddition(PlanetTransport inPlanetTransport, StationComponent inStationComponent,
            int inItemID, int inItemCount, ELogisticStorage inLocalMode, ELogisticStorage inRemoteMode, bool inWarpersRequiredToggleAutomation)
        {

            Plugin._logger.LogMessage("EntityID : " + inStationComponent.entityId + ", ID : " + inStationComponent.id + ", PCID : " + inStationComponent.pcId);

            // we need to add a slot for the warper

            List<StationStore> storeCopy = new List<StationStore>(inStationComponent.storage);
            storeCopy.Add(new StationStore());
            inStationComponent.storage = storeCopy.ToArray();

            var storageIndex = inStationComponent.storage.Length - 1;

            Plugin._logger.LogMessage("Index To Add " + inItemID + " to : " + storageIndex);

            inPlanetTransport.SetStationStorage(
                inStationComponent.id,
                storageIndex,
                inItemID,
                inItemCount,
                inLocalMode,
                inRemoteMode,
                GameMain.mainPlayer);

            if (inWarpersRequiredToggleAutomation == true)
            {
                if (inStationComponent.warperNecessary == true && inStationComponent.storage[storageIndex].localLogic != inLocalMode)
                {
                    inPlanetTransport.SetStationStorage(
                        inStationComponent.id,
                        storageIndex,
                        inItemID,
                        inItemCount,
                        inRemoteMode,
                        inStationComponent.storage[storageIndex].remoteLogic,
                        GameMain.mainPlayer);
                }
                else if (inStationComponent.warperNecessary == false && inStationComponent.storage[storageIndex].localLogic != ELogisticStorage.Supply)
                {
                    inPlanetTransport.SetStationStorage(
                        inStationComponent.id,
                        storageIndex,
                        inItemID,
                        inItemCount,
                        ELogisticStorage.Supply,
                        inStationComponent.storage[storageIndex].remoteLogic,
                        GameMain.mainPlayer);
                }
            }
        }

        private static void DoShipTransfers()
        {
            //Plugin._logger.LogMessage("About to do Array.FindIndex");

            //int manualWarperStoreIndex = Array.FindIndex(stationComponent.storage, store => store.itemId == warperId);
            //if (manualWarperStoreIndex != -1
            //    && manualWarperStoreIndex != warperSlotIndex
            //    && stationComponent.storage[warperSlotIndex].count < stationComponent.storage[warperSlotIndex].max
            //    && stationComponent.storage[manualWarperStoreIndex].count > stationComponent.storage[manualWarperStoreIndex].max)
            //{
            //    int warperOverflowInManualSlot = stationComponent.storage[manualWarperStoreIndex].count - stationComponent.storage[manualWarperStoreIndex].max;
            //    int warperNeedInAutomaticSlot = stationComponent.storage[warperSlotIndex].max - stationComponent.storage[warperSlotIndex].count;
            //    int transferAmount = Mathf.Min(warperNeedInAutomaticSlot, warperOverflowInManualSlot);
            //    stationComponent.storage[manualWarperStoreIndex].count -= transferAmount;
            //    stationComponent.storage[warperSlotIndex].count += transferAmount;
            //    stationComponent.storage[warperSlotIndex].localOrder = 0;
            //    stationComponent.storage[warperSlotIndex].remoteOrder = 0;
            //    needRefreshTraffic = true;
            //}

            //if (manualWarperStoreIndex != -1 &&
            //    (stationComponent.storage[warperSlotIndex].count == stationComponent.storage[warperSlotIndex].max
            //     && stationComponent.storage[warperSlotIndex].localLogic == ELogisticStorage.Demand && stationComponent.storage[warperSlotIndex].totalOrdered != 0
            //    ||
            //    stationComponent.storage[warperSlotIndex].count == 0 &&
            //    stationComponent.storage[warperSlotIndex].localLogic == ELogisticStorage.Supply && stationComponent.storage[warperSlotIndex].totalOrdered != 0))
            //{
            //    int storageCount = stationComponent.storage[warperSlotIndex].count;
            //    stationComponent.storage[warperSlotIndex].count = 0;

            //    __instance.SetStationStorage(
            //        stationComponent.id,
            //        warperSlotIndex,
            //        0, defaultMaxValue,
            //        defaultLocalMode,
            //        defaultRemoteMode,
            //        GameMain.mainPlayer);

            //    __instance.SetStationStorage(
            //        stationComponent.id,
            //        warperSlotIndex,
            //        warperId, defaultMaxValue,
            //        defaultLocalMode, defaultRemoteMode,
            //        GameMain.mainPlayer);
            //    stationComponent.storage[warperSlotIndex].count = storageCount;
            //    needRefreshTraffic = true;
            //}


            //if (needRefreshTraffic)
            ////{
            //inStationComponent.UpdateNeeds();
            //inPlanetTransport.RefreshTraffic();
            //inPlanetTransport.gameData.galacticTransport.RefreshTraffic(inStationComponent.gid);
            //}
        }

        [HarmonyPatch(typeof(PlanetTransport), "GameTick", typeof(long), typeof(bool))]
        [HarmonyPostfix]
        public static void PlanetTransport_GameTick_Postfix(PlanetTransport __instance)
        {

            if (ModDisabled == true)
            {
                return;
            }

            Plugin._logger.LogInfo("hi");
            // we want everything that is a stellar station, but not a collect station

            //foreach (var aVar in LDB.items.dataArray.Where(i => i.prefabDesc.isStation || i.prefabDesc.isCollectStation || i.prefabDesc.isStellarStation))
            //    Plugin._logger.LogMessage(aVar.name + " (" + aVar.ID + ") : " + aVar.prefabDesc.stationMaxItemKinds + " ( isStation :  " + aVar.prefabDesc.isStation + ", isCollectStation : " + aVar.prefabDesc.isCollectStation + ", isStellarStation : " + aVar.prefabDesc.isStellarStation + " )");


            ELogisticStorage defaultLocalMode = Config.General.WarperLocalMode.Value;
            ELogisticStorage defaultRemoteMode = Config.General.WarperRemoteMode.Value;
            bool warpersRequiredToggleAutomation = Config.General.WarpersRequiredToggleAutomation.Value;
            bool showWarperSlot = Config.General.ShowWarperSlot.Value;
            int defaultMaxValue = Config.General.WarperMaxValue.Value;

            int warperId = ItemProto.kWarperId;
            int droneId = ItemProto.kDroneId;
            int vesselId = ItemProto.kShipId;

            var defaultStorage = new List<PrefabDesc>();
            foreach (var aStationPrefab in LDB.items.dataArray.Where(i => i.prefabDesc.isStellarStation == true && i.prefabDesc.isCollectStation == false))
                defaultStorage.Add(aStationPrefab.prefabDesc);


            foreach (var stationComponent in __instance.stationPool.Where(i => i != null))
            {
                // find the prefab

                if (stationComponent.storage == null) // can happen when you delete a station.. don't ask me why the stationcomponent is still hanging around
                    continue;

                Plugin._logger.LogMessage(stationComponent.storage.Length);

                var aPrefab = defaultStorage.Where(i => i.stationMaxItemKinds == stationComponent.storage.Length).FirstOrDefault();

                if (aPrefab != null)
                {
                    DoHiddenItemAddition(__instance, stationComponent, warperId, defaultMaxValue, defaultLocalMode, defaultRemoteMode, warpersRequiredToggleAutomation);
                    DoHiddenItemAddition(__instance, stationComponent, droneId, defaultMaxValue, defaultLocalMode, defaultRemoteMode, warpersRequiredToggleAutomation);
                    DoHiddenItemAddition(__instance, stationComponent, vesselId, defaultMaxValue, defaultLocalMode, defaultRemoteMode, warpersRequiredToggleAutomation);
                }

                // now, because stations don't load drones and vessels automatically, we need to do that ourselves

                if (stationComponent.storage.Length > 10)
                {
                    if ((stationComponent.idleDroneCount + stationComponent.workDroneCount) < 97)
                        stationComponent.idleDroneCount += 97 - (stationComponent.idleDroneCount + stationComponent.workDroneCount);

                    Plugin._logger.LogMessage(stationComponent.deliveryDrones + "," + stationComponent.idleDroneCount + "," + stationComponent.workDroneCount);

                }

            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationStorage), "OnItemPickerReturn", typeof(ItemProto))]
        public static bool UIStationStorage_OnItemPickerReturn_Prefix(UIStationStorage __instance, ItemProto itemProto)
        {
            if (ModDisabled == true)
            {
                return true;
            }
            Plugin._logger.LogMessage("UIStationStorage_OnItemPickerReturn_Prefix");

            int warperId = ItemProto.kWarperId;

            for (int aStationIter = 0; aStationIter < StationIDs.Count; aStationIter++)
            {
                var aStationID = StationIDs[aStationIter];
                Plugin._logger.LogMessage("Trying UIStationStorage_OnItemPickerReturn_Prefix " + aStationID);

                try
                {
                    var prefabDesc = LDB.items.Select(aStationID).prefabDesc;
                    if (prefabDesc == null)
                    {
                        Plugin._logger.LogMessage("No id here (" + aStationID + ")");
                        continue;
                    }


                    int warperSlotIndex = prefabDesc.stationMaxItemKinds;

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
                            if (__instance.station.isStellar == true && __instance.station.isCollector == false && i == warperSlotIndex && itemProto.ID == warperId)
                            {
                                continue;
                            }
                            UIRealtimeTip.Popup("不选择重复物品".Translate(), true, 0);
                            return false;
                        }
                    }
                    __instance.stationWindow.transport.SetStationStorage(__instance.station.id, __instance.index,
                        itemProto.ID, itemProto2.prefabDesc.stationMaxItemCount, ELogisticStorage.Supply,
                        (!__instance.station.isStellar) ? ELogisticStorage.None : ELogisticStorage.Supply, GameMain.mainPlayer);

                }
                catch (Exception anExc)
                {
                    Plugin._logger.LogError(anExc.Message);
                    Plugin._logger.LogMessage("No id here (" + aStationID + ")");
                    continue;
                }

            }

            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStationWindow), "_OnCreate")]
        public static bool UIStationWindow_OnCreate_Prefix(UIStationWindow __instance, ref UIStationStorage[] ___storageUIs, UIStationStorage ___storageUIPrefab)
        {
            Plugin._logger.LogMessage("UIStationWindow_OnCreate_Prefix");
            return false;

            //bool showWarperSlot = Config.General.ShowWarperSlot.Value;
            //if (showWarperSlot == true || ModDisabled == true)
            //{
            //    return true;
            //}
            //___storageUIs = new UIStationStorage[5];
            //for (int i = 0; i < ___storageUIs.Length; i++)
            //{
            //    ___storageUIs[i] = Object.Instantiate(___storageUIPrefab, ___storageUIPrefab.transform.parent);
            //    ((RectTransform)___storageUIs[i].transform).anchoredPosition = new Vector2(40f, -90 - 76 * i);
            //    ___storageUIs[i].stationWindow = __instance;
            //    ___storageUIs[i]._Create();
            //}
            //return false;
        }
    }
}
