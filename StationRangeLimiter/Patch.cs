using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace StationRangeLimiter
{
    static class Patch
    {
	    private class ModData
	    {
		    public List<int> EnforcedLocalRangeStations = new List<int>();
		    public List<int> EnforcedRemoteRangeStations = new List<int>(); 
	    }
	    private static ModData _data = new ModData();  
	   

	    [HarmonyPostfix]
	    [HarmonyPatch(typeof(GameSave), "LoadCurrentGame", typeof(string))]
	    public static void LoadCurrentGame_Postfix(string saveName)
	    {
		    string path = Application.persistentDataPath + "/" + "SRL_Mod_" + saveName + ".lst";
		    if (File.Exists(path))
		    {
			    string jsonString = File.ReadAllText(path);
			    JsonUtility.FromJsonOverwrite(jsonString, _data);
		    }
	    }
	    
	    [HarmonyPostfix]
	    [HarmonyPatch(typeof(GameSave), "SaveCurrentGame", typeof(string))]
	    
	    public static void SaveCurrentGame_Postfix(string saveName)
	    {
		    string path = Application.persistentDataPath + "/" + "SRL_Mod_" + saveName + ".lst";
		    string jsonString = JsonUtility.ToJson(_data);
		    File.WriteAllText(path, jsonString);
	    }

	    [HarmonyPostfix]
	    [HarmonyPatch(typeof(UIStationWindow), "_OnUpdate")]
	    public static void _OnUpdate_Postfix(UIStationWindow __instance, UIButton ___warperNecessaryButton, Image ___warperNecessaryCheck)
	    {
		    UIButton enforceRemoteRangeButton =
			    ___warperNecessaryButton.transform.parent.Find("EnforceRemoteRangeButton").GetComponent<UIButton>();
		    UIButton enforceLocalRangeButton =
			    ___warperNecessaryButton.transform.parent.Find("EnforceLocalRangeButton").GetComponent<UIButton>();
		    StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
		    bool enforceRemoteRange = _data.EnforcedRemoteRangeStations.Contains(stationComponent.entityId);
		    bool enforceLocalRange = _data.EnforcedLocalRangeStations.Contains(stationComponent.entityId);
		    enforceLocalRangeButton.transform.Find(___warperNecessaryCheck.name).GetComponent<Image>().enabled = enforceLocalRange == true;
		    enforceRemoteRangeButton.transform.Find(___warperNecessaryCheck.name).GetComponent<Image>().enabled = enforceRemoteRange == true;
	    }
	    
	    [HarmonyPostfix]
	    [HarmonyPatch(typeof(UIStationWindow), "_OnRegEvent")]
	    public static void _OnRegEvent_Postfix(UIStationWindow __instance, bool ___event_lock, UIButton ___warperNecessaryButton, Image ___warperNecessaryCheck)
	    {
		    UIButton enforceRemoteRangeButton =
			    ___warperNecessaryButton.transform.parent.Find("EnforceRemoteRangeButton").GetComponent<UIButton>();
		    UIButton enforceLocalRangeButton =
			    ___warperNecessaryButton.transform.parent.Find("EnforceLocalRangeButton").GetComponent<UIButton>();
		    enforceRemoteRangeButton.onClick += (i) =>
		    {
			    OnEnforceRemoteRangeButtonClick(enforceRemoteRangeButton, ___warperNecessaryCheck, __instance, ___event_lock);
		    };
		    enforceLocalRangeButton.onClick += (i) =>
		    {
			    OnEnforceLocalRangeButtonClick(enforceLocalRangeButton, ___warperNecessaryCheck, __instance, ___event_lock);
		    };
	    }

	    [HarmonyPostfix]
	    [HarmonyPatch(typeof(UIStationWindow), "_OnUnregEvent")]
	    public static void _OnUnregEvent_Postfix(UIStationWindow __instance, bool ___event_lock, UIButton ___warperNecessaryButton, Image ___warperNecessaryCheck)
	    {
		    UIButton enforceRemoteRangeButton =
			    ___warperNecessaryButton.transform.parent.Find("EnforceRemoteRangeButton").GetComponent<UIButton>();
		    UIButton enforceLocalRangeButton =
			    ___warperNecessaryButton.transform.parent.Find("EnforceLocalRangeButton").GetComponent<UIButton>();
		    enforceRemoteRangeButton.onClick -= (i) =>
		    {
			    OnEnforceRemoteRangeButtonClick(enforceRemoteRangeButton, ___warperNecessaryCheck, __instance, ___event_lock);
		    };
		    enforceLocalRangeButton.onClick -= (i) =>
		    {
			    OnEnforceLocalRangeButtonClick(enforceLocalRangeButton, ___warperNecessaryCheck, __instance, ___event_lock);
		    };
	    }
	    
	    private static void OnEnforceRemoteRangeButtonClick(UIButton enforceRemoteRangeButton, Image warperNecessaryCheck, UIStationWindow instance, bool event_lock)
	    {
		    if (event_lock || instance.stationId == 0 || instance.factory == null)
			    return;
		    StationComponent stationComponent = instance.transport.stationPool[instance.stationId];
		    if (stationComponent == null || stationComponent.id != instance.stationId)
			    return;
		    bool enforceRemoteRange = _data.EnforcedRemoteRangeStations.Contains(stationComponent.entityId);
		    if (enforceRemoteRange == true)
		    {
			    _data.EnforcedRemoteRangeStations.Remove(stationComponent.entityId);
		    }
		    else
		    {
			    _data.EnforcedRemoteRangeStations.Add(stationComponent.entityId);
		    }
		    enforceRemoteRangeButton.transform.Find(warperNecessaryCheck.name).GetComponent<Image>().enabled = enforceRemoteRange == false;
	    }

	    private static void OnEnforceLocalRangeButtonClick(UIButton enforceLocalRangeButton, Image warperNecessaryCheck, UIStationWindow instance, bool event_lock)
	    {
		    if (event_lock || instance.stationId == 0 || instance.factory == null)
			    return;
		    StationComponent stationComponent = instance.transport.stationPool[instance.stationId];
		    if (stationComponent == null || stationComponent.id != instance.stationId)
			    return;
		    bool enforceLocalRange = _data.EnforcedLocalRangeStations.Contains(stationComponent.entityId);
		    if (enforceLocalRange == true)
		    {
			    _data.EnforcedLocalRangeStations.Remove(stationComponent.entityId);
		    }
		    else
		    {
			    _data.EnforcedLocalRangeStations.Add(stationComponent.entityId);
		    }
		    enforceLocalRangeButton.transform.Find(warperNecessaryCheck.name).GetComponent<Image>().enabled = enforceLocalRange == false;
	    }
	            
	    [HarmonyPostfix]
	    [HarmonyPatch(typeof(UIStationWindow), "_OnCreate")]
	    public static void _OnCreatePostfix(UIStationWindow __instance, UIButton ___warperNecessaryButton, Image ___warperNecessaryCheck)
	    {
		    UIButton enforceRemoteRangeButton = UnityEngine.Object.Instantiate(___warperNecessaryButton, ___warperNecessaryButton.transform.parent);
		    RectTransform enforceRemoteRangeButtonRectTransform = (RectTransform) enforceRemoteRangeButton.transform;
		    enforceRemoteRangeButtonRectTransform.anchoredPosition -= new Vector2(0, 40);
		    enforceRemoteRangeButton.name = "EnforceRemoteRangeButton";
		    UnityEngine.Object.Destroy( enforceRemoteRangeButton.GetComponentInChildren<Localizer>());
		    enforceRemoteRangeButton.GetComponentInChildren<Text>().text = "Enforce remote range";
		    UIButton enforceLocalRangeButton = UnityEngine.Object.Instantiate(___warperNecessaryButton, ___warperNecessaryButton.transform.parent);
		    RectTransform enforceLocalRangeButtonRectTransform = (RectTransform) enforceLocalRangeButton.transform;
		    enforceLocalRangeButton.name = "EnforceLocalRangeButton";
		    enforceLocalRangeButtonRectTransform.anchoredPosition -= new Vector2(0, 20);
		    UnityEngine.Object.Destroy( enforceLocalRangeButton.GetComponentInChildren<Localizer>());
		    enforceLocalRangeButton.GetComponentInChildren<Text>().text = "Enforce local range";
	    }

	    [HarmonyPrefix]
	    [HarmonyPatch(typeof(StationComponent), "InternalTickRemote", typeof(int), typeof(double), typeof(float),
		    typeof(float), typeof(int),
		    typeof(StationComponent[]), typeof(AstroPose[]), typeof(VectorLF3), typeof(Quaternion), typeof(bool),
		    typeof(int[]))]
	    public static bool InternalTickRemote_Prefix(StationComponent __instance, bool ___warperFree,
		    int ____tmp_iter_remote, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed, int shipCarries,
		    StationComponent[] gStationPool, AstroPose[] astroPoses, VectorLF3 relativePos, Quaternion relativeRot,
		    bool starmap, int[] consumeRegister)
	    {
	    bool flag = shipWarpSpeed > shipSailSpeed + 1f;
		___warperFree = DSPGame.IsMenuDemo;
		if (__instance.warperCount < __instance.warperMaxCount)
		{
			for (int i = 0; i < __instance.storage.Length; i++)
			{
				if (__instance.storage[i].itemId == 1210 && __instance.storage[i].count > 0)
				{
					__instance.warperCount++;
					StationStore[] array = __instance.storage;
					int num = i;
					array[num].count = array[num].count - 1;
					break;
				}
			}
		}
		if (timeGene == __instance.gene)
		{
			____tmp_iter_remote++;
			if (__instance.remotePairCount > 0 && __instance.idleShipCount > 0)
			{
				__instance.remotePairProcess %= __instance.remotePairCount;
				int num2 = __instance.remotePairProcess;
				SupplyDemandPair supplyDemandPair;
				StationComponent stationComponent;
				double num4;
				bool flag3;
				StationComponent stationComponent2;
				double num5;
				bool flag5;
				for (;;)
				{
					int num3 = (shipCarries - 1) * __instance.deliveryShips / 100;
					supplyDemandPair = __instance.remotePairs[__instance.remotePairProcess];
					if (supplyDemandPair.supplyId == __instance.gid && __instance.storage[supplyDemandPair.supplyIndex].max <= num3)
					{
						num3 = __instance.storage[supplyDemandPair.supplyIndex].max - 1;
					}
					if (supplyDemandPair.supplyId == __instance.gid && __instance.storage[supplyDemandPair.supplyIndex].count > num3 && __instance.storage[supplyDemandPair.supplyIndex].remoteSupplyCount > num3 && __instance.storage[supplyDemandPair.supplyIndex].totalSupplyCount > num3)
					{
						stationComponent = gStationPool[supplyDemandPair.demandId];
						if (stationComponent != null)
						{
							num4 = (astroPoses[__instance.planetId].uPos - astroPoses[stationComponent.planetId].uPos).magnitude + (double)astroPoses[__instance.planetId].uRadius + (double)astroPoses[stationComponent.planetId].uRadius;
							bool flag2 = num4 < __instance.tripRangeShips;
							bool demandRemoteRange = num4 < stationComponent.tripRangeShips;
							bool enforceRemoteRange = _data.EnforcedRemoteRangeStations.Contains(stationComponent.entityId);
							flag3 = (num4 >= __instance.warpEnableDist);
							if (__instance.warperNecessary && flag3 && (__instance.warperCount < 2 || !flag))
							{
								flag2 = false;
							}
							if (flag2 && (enforceRemoteRange && demandRemoteRange || enforceRemoteRange == false)
							     && stationComponent.storage[supplyDemandPair.demandIndex].remoteDemandCount > 0 && stationComponent.storage[supplyDemandPair.demandIndex].totalDemandCount > 0)
							{
								break;
							}
						}
					}
					if (supplyDemandPair.demandId == __instance.gid && __instance.storage[supplyDemandPair.demandIndex].remoteDemandCount > 0 && __instance.storage[supplyDemandPair.demandIndex].totalDemandCount > 0)
					{
						stationComponent2 = gStationPool[supplyDemandPair.supplyId];
						if (stationComponent2 != null)
						{
							num5 = (astroPoses[__instance.planetId].uPos - astroPoses[stationComponent2.planetId].uPos).magnitude + (double)astroPoses[__instance.planetId].uRadius + (double)astroPoses[stationComponent2.planetId].uRadius;
							bool flag4 = num5 < __instance.tripRangeShips;
							bool supplyRemoteRange = num5 < stationComponent2.tripRangeShips;
							bool enforceRemoteRange = _data.EnforcedRemoteRangeStations.Contains(stationComponent2.entityId);
							if (flag4 && !__instance.includeOrbitCollector && stationComponent2.isCollector)
							{
								flag4 = false;
							}
							flag5 = (num5 >= __instance.warpEnableDist);
							if (__instance.warperNecessary && flag5 && (__instance.warperCount < 2 || !flag))
							{
								flag4 = false;
							}
							if (stationComponent2.storage[supplyDemandPair.supplyIndex].max <= num3)
							{
								num3 = stationComponent2.storage[supplyDemandPair.supplyIndex].max - 1;
							}
							
							if (flag4 && (enforceRemoteRange && supplyRemoteRange || enforceRemoteRange == false) && 
							    stationComponent2.storage[supplyDemandPair.supplyIndex].count >= num3 && stationComponent2.storage[supplyDemandPair.supplyIndex].remoteSupplyCount >= num3 && stationComponent2.storage[supplyDemandPair.supplyIndex].totalSupplyCount >= num3)
							{
								goto Block_41;
							}
						}
					}
					__instance.remotePairProcess++;
					__instance.remotePairProcess %= __instance.remotePairCount;
					if (num2 == __instance.remotePairProcess)
					{
						goto IL_1009;
					}
				}
				long num6 = __instance.CalcTripEnergyCost(num4, shipSailSpeed, flag);
				if (__instance.energy >= num6)
				{
					int num7 = (shipCarries >= __instance.storage[supplyDemandPair.supplyIndex].count) ? __instance.storage[supplyDemandPair.supplyIndex].count : shipCarries;
					int num8 = __instance.QueryIdleShip(__instance.nextShipIndex);
					if (num8 >= 0)
					{
						__instance.nextShipIndex = (num8 + 1) % __instance.workShipDatas.Length;
						__instance.workShipDatas[__instance.workShipCount].stage = -2;
						__instance.workShipDatas[__instance.workShipCount].planetA = __instance.planetId;
						__instance.workShipDatas[__instance.workShipCount].planetB = stationComponent.planetId;
						__instance.workShipDatas[__instance.workShipCount].otherGId = stationComponent.gid;
						__instance.workShipDatas[__instance.workShipCount].direction = 1;
						__instance.workShipDatas[__instance.workShipCount].t = 0f;
						__instance.workShipDatas[__instance.workShipCount].itemId = (__instance.workShipOrders[__instance.workShipCount].itemId = __instance.storage[supplyDemandPair.supplyIndex].itemId);
						__instance.workShipDatas[__instance.workShipCount].itemCount = num7;
						__instance.workShipDatas[__instance.workShipCount].gene = ____tmp_iter_remote;
						__instance.workShipDatas[__instance.workShipCount].shipIndex = num8;
						__instance.workShipOrders[__instance.workShipCount].otherStationGId = stationComponent.gid;
						__instance.workShipOrders[__instance.workShipCount].thisIndex = supplyDemandPair.supplyIndex;
						__instance.workShipOrders[__instance.workShipCount].otherIndex = supplyDemandPair.demandIndex;
						__instance.workShipOrders[__instance.workShipCount].thisOrdered = 0;
						__instance.workShipOrders[__instance.workShipCount].otherOrdered = num7;
						if (flag3)
						{
							if (__instance.warperCount >= 2)
							{
								ShipData[] array2 = __instance.workShipDatas;
								int num9 = __instance.workShipCount;
								array2[num9].warperCnt = array2[num9].warperCnt + 2;
								__instance.warperCount -= 2;
								consumeRegister[1210] += 2;
							}
							else if (__instance.warperCount >= 1)
							{
								ShipData[] array3 = __instance.workShipDatas;
								int num10 = __instance.workShipCount;
								array3[num10].warperCnt = array3[num10].warperCnt + 1;
								__instance.warperCount--;
								consumeRegister[1210]++;
							}
							else if (___warperFree)
							{
								ShipData[] array4 = __instance.workShipDatas;
								int num11 = __instance.workShipCount;
								array4[num11].warperCnt = array4[num11].warperCnt + 2;
							}
						}
						StationStore[] array5 = stationComponent.storage;
						int demandIndex = supplyDemandPair.demandIndex;
						array5[demandIndex].remoteOrder = array5[demandIndex].remoteOrder + num7;
						__instance.workShipCount++;
						__instance.idleShipCount--;
						__instance.IdleShipGetToWork(num8);
						StationStore[] array6 = __instance.storage;
						int supplyIndex = supplyDemandPair.supplyIndex;
						array6[supplyIndex].count = array6[supplyIndex].count - num7;
						__instance.energy -= num6;
					}
				}
				goto IL_1009;
				Block_41:
				long num12 = __instance.CalcTripEnergyCost(num5, shipSailSpeed, flag);
				if (!stationComponent2.isCollector)
				{
					bool flag6 = false;
					__instance.remotePairProcess %= __instance.remotePairCount;
					int num13 = __instance.remotePairProcess + 1;
					int num14 = __instance.remotePairProcess;
					num13 %= __instance.remotePairCount;
					SupplyDemandPair supplyDemandPair2;
					for (;;)
					{
						supplyDemandPair2 = __instance.remotePairs[num13];
						int num3 = 0;
						if (supplyDemandPair2.supplyId == __instance.gid && supplyDemandPair2.demandId == stationComponent2.gid
						                                                 && __instance.storage[supplyDemandPair2.supplyIndex].count >= num3
						                                                 && __instance.storage[supplyDemandPair2.supplyIndex].remoteSupplyCount >= num3
						                                                 && __instance.storage[supplyDemandPair2.supplyIndex].totalSupplyCount >= num3
						                                                 && stationComponent2.storage[supplyDemandPair2.demandIndex].remoteDemandCount > 0 
						                                                 && stationComponent2.storage[supplyDemandPair2.demandIndex].totalDemandCount > 0)
						{
							break;
						}
						num13++;
						num13 %= __instance.remotePairCount;
						if (num14 == num13)
						{
							goto IL_C9A;
						}
					}
					if (__instance.energy >= num12)
					{
						int num15 = (shipCarries >= __instance.storage[supplyDemandPair2.supplyIndex].count) ? __instance.storage[supplyDemandPair2.supplyIndex].count : shipCarries;
						int num16 = __instance.QueryIdleShip(__instance.nextShipIndex);
						if (num16 >= 0)
						{
							__instance.nextShipIndex = (num16 + 1) % __instance.workShipDatas.Length;
							__instance.workShipDatas[__instance.workShipCount].stage = -2;
							__instance.workShipDatas[__instance.workShipCount].planetA = __instance.planetId;
							__instance.workShipDatas[__instance.workShipCount].planetB = stationComponent2.planetId;
							__instance.workShipDatas[__instance.workShipCount].otherGId = stationComponent2.gid;
							__instance.workShipDatas[__instance.workShipCount].direction = 1;
							__instance.workShipDatas[__instance.workShipCount].t = 0f;
							__instance.workShipDatas[__instance.workShipCount].itemId = (__instance.workShipOrders[__instance.workShipCount].itemId = __instance.storage[supplyDemandPair2.supplyIndex].itemId);
							__instance.workShipDatas[__instance.workShipCount].itemCount = num15;
							__instance.workShipDatas[__instance.workShipCount].gene = ____tmp_iter_remote;
							__instance.workShipDatas[__instance.workShipCount].shipIndex = num16;
							__instance.workShipOrders[__instance.workShipCount].otherStationGId = stationComponent2.gid;
							__instance.workShipOrders[__instance.workShipCount].thisIndex = supplyDemandPair2.supplyIndex;
							__instance.workShipOrders[__instance.workShipCount].otherIndex = supplyDemandPair2.demandIndex;
							__instance.workShipOrders[__instance.workShipCount].thisOrdered = 0;
							__instance.workShipOrders[__instance.workShipCount].otherOrdered = num15;
							if (flag5)
							{
								if (__instance.warperCount >= 2)
								{
									ShipData[] array7 = __instance.workShipDatas;
									int num17 = __instance.workShipCount;
									array7[num17].warperCnt = array7[num17].warperCnt + 2;
									__instance.warperCount -= 2;
									consumeRegister[1210] += 2;
								}
								else if (__instance.warperCount >= 1)
								{
									ShipData[] array8 = __instance.workShipDatas;
									int num18 = __instance.workShipCount;
									array8[num18].warperCnt = array8[num18].warperCnt + 1;
									__instance.warperCount--;
									consumeRegister[1210]++;
								}
								else if (___warperFree)
								{
									ShipData[] array9 = __instance.workShipDatas;
									int num19 = __instance.workShipCount;
									array9[num19].warperCnt = array9[num19].warperCnt + 2;
								}
							}
							StationStore[] array10 = stationComponent2.storage;
							int demandIndex2 = supplyDemandPair2.demandIndex;
							array10[demandIndex2].remoteOrder = array10[demandIndex2].remoteOrder + num15;
							__instance.workShipCount++;
							__instance.idleShipCount--;
							__instance.IdleShipGetToWork(num16);
							StationStore[] array11 = __instance.storage;
							int supplyIndex2 = supplyDemandPair2.supplyIndex;
							array11[supplyIndex2].count = array11[supplyIndex2].count - num15;
							__instance.energy -= num12;
							flag6 = true;
						}
					}
					IL_C9A:
					if (flag6)
					{
						goto IL_1009;
					}
				}
				if (__instance.energy >= num12)
				{
					int num20 = __instance.QueryIdleShip(__instance.nextShipIndex);
					if (num20 >= 0)
					{
						__instance.nextShipIndex = (num20 + 1) % __instance.workShipDatas.Length;
						__instance.workShipDatas[__instance.workShipCount].stage = -2;
						__instance.workShipDatas[__instance.workShipCount].planetA = __instance.planetId;
						__instance.workShipDatas[__instance.workShipCount].planetB = stationComponent2.planetId;
						__instance.workShipDatas[__instance.workShipCount].otherGId = stationComponent2.gid;
						__instance.workShipDatas[__instance.workShipCount].direction = 1;
						__instance.workShipDatas[__instance.workShipCount].t = 0f;
						__instance.workShipDatas[__instance.workShipCount].itemId = (__instance.workShipOrders[__instance.workShipCount].itemId = __instance.storage[supplyDemandPair.demandIndex].itemId);
						__instance.workShipDatas[__instance.workShipCount].itemCount = 0;
						__instance.workShipDatas[__instance.workShipCount].gene = ____tmp_iter_remote;
						__instance.workShipDatas[__instance.workShipCount].shipIndex = num20;
						__instance.workShipOrders[__instance.workShipCount].otherStationGId = stationComponent2.gid;
						__instance.workShipOrders[__instance.workShipCount].thisIndex = supplyDemandPair.demandIndex;
						__instance.workShipOrders[__instance.workShipCount].otherIndex = supplyDemandPair.supplyIndex;
						__instance.workShipOrders[__instance.workShipCount].thisOrdered = shipCarries;
						__instance.workShipOrders[__instance.workShipCount].otherOrdered = -shipCarries;
						if (flag5)
						{
							if (__instance.warperCount >= 2)
							{
								ShipData[] array12 = __instance.workShipDatas;
								int num21 = __instance.workShipCount;
								array12[num21].warperCnt = array12[num21].warperCnt + 2;
								__instance.warperCount -= 2;
								consumeRegister[1210] += 2;
							}
							else if (__instance.warperCount >= 1)
							{
								ShipData[] array13 = __instance.workShipDatas;
								int num22 = __instance.workShipCount;
								array13[num22].warperCnt = array13[num22].warperCnt + 1;
								__instance.warperCount--;
								consumeRegister[1210]++;
							}
							else if (___warperFree)
							{
								ShipData[] array14 = __instance.workShipDatas;
								int num23 = __instance.workShipCount;
								array14[num23].warperCnt = array14[num23].warperCnt + 2;
							}
						}
						StationStore[] array15 = __instance.storage;
						int demandIndex3 = supplyDemandPair.demandIndex;
						array15[demandIndex3].remoteOrder = array15[demandIndex3].remoteOrder + shipCarries;
						StationStore[] array16 = stationComponent2.storage;
						int supplyIndex3 = supplyDemandPair.supplyIndex;
						array16[supplyIndex3].remoteOrder = array16[supplyIndex3].remoteOrder - shipCarries;
						__instance.workShipCount++;
						__instance.idleShipCount--;
						__instance.IdleShipGetToWork(num20);
						__instance.energy -= num12;
					}
				}
				IL_1009:
				__instance.remotePairProcess++;
				__instance.remotePairProcess %= __instance.remotePairCount;
			}
		}
		float num24 = Mathf.Sqrt(shipSailSpeed / 600f);
		float num25 = num24;
		if (num25 > 1f)
		{
			num25 = Mathf.Log(num25) + 1f;
		}
		AstroPose astroPose = astroPoses[__instance.planetId];
		float num26 = shipSailSpeed * 0.03f * num25;
		float num27 = shipSailSpeed * 0.12f * num25;
		float num28 = shipSailSpeed * 0.4f * num24;
		float num29 = num24 * 0.006f + 1E-05f;
		int j = 0;
		while (j < __instance.workShipCount)
		{
			ShipData shipData = __instance.workShipDatas[j];
			bool flag7 = false;
			Quaternion quaternion = Quaternion.identity;
			if (shipData.otherGId <= 0)
			{
				shipData.direction = -1;
				if (shipData.stage > 0)
				{
					shipData.stage = 0;
				}
			}
			if (shipData.stage < -1)
			{
				if (shipData.direction > 0)
				{
					shipData.t += 0.03335f;
					if (shipData.t > 1f)
					{
						shipData.t = 0f;
						shipData.stage = -1;
					}
				}
				else
				{
					shipData.t -= 0.03335f;
					if (shipData.t < 0f)
					{
						shipData.t = 0f;
						__instance.AddItem(shipData.itemId, shipData.itemCount);
						if (__instance.workShipOrders[j].itemId > 0)
						{
							if (__instance.storage[__instance.workShipOrders[j].thisIndex].itemId == __instance.workShipOrders[j].itemId)
							{
								StationStore[] array17 = __instance.storage;
								int thisIndex = __instance.workShipOrders[j].thisIndex;
								array17[thisIndex].remoteOrder = array17[thisIndex].remoteOrder - __instance.workShipOrders[j].thisOrdered;
							}
							__instance.workShipOrders[j].ClearThis();
						}
						Array.Copy(__instance.workShipDatas, j + 1, __instance.workShipDatas, j, __instance.workShipDatas.Length - j - 1);
						Array.Copy(__instance.workShipOrders, j + 1, __instance.workShipOrders, j, __instance.workShipOrders.Length - j - 1);
						__instance.workShipCount--;
						__instance.idleShipCount++;
						__instance.WorkShipBackToIdle(shipData.shipIndex);
						Array.Clear(__instance.workShipDatas, __instance.workShipCount, __instance.workShipDatas.Length - __instance.workShipCount);
						Array.Clear(__instance.workShipOrders, __instance.workShipCount, __instance.workShipOrders.Length - __instance.workShipCount);
						j--;
						goto IL_34F2;
					}
				}
				shipData.uPos = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, __instance.shipDiskPos[shipData.shipIndex]);
				shipData.uVel.x = 0f;
				shipData.uVel.y = 0f;
				shipData.uVel.z = 0f;
				shipData.uSpeed = 0f;
				shipData.uRot = astroPose.uRot * __instance.shipDiskRot[shipData.shipIndex];
				shipData.uAngularVel.x = 0f;
				shipData.uAngularVel.y = 0f;
				shipData.uAngularVel.z = 0f;
				shipData.uAngularSpeed = 0f;
				shipData.pPosTemp = Vector3.zero;
				shipData.pRotTemp = Quaternion.identity;
				__instance.shipRenderers[shipData.shipIndex].anim.z = 0f;
				goto IL_32E5;
			}
			if (shipData.stage == -1)
			{
				if (shipData.direction > 0)
				{
					shipData.t += num29;
					float num30 = shipData.t;
					if (shipData.t > 1f)
					{
						shipData.t = 1f;
						num30 = 1f;
						shipData.stage = 0;
					}
					__instance.shipRenderers[shipData.shipIndex].anim.z = num30;
					num30 = (3f - num30 - num30) * num30 * num30;
					shipData.uPos = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, __instance.shipDiskPos[shipData.shipIndex] + __instance.shipDiskPos[shipData.shipIndex].normalized * (25f * num30));
					shipData.uRot = astroPose.uRot * __instance.shipDiskRot[shipData.shipIndex];
				}
				else
				{
					shipData.t -= num29 * 0.6666667f;
					float num30 = shipData.t;
					if (shipData.t < 0f)
					{
						shipData.t = 1f;
						num30 = 0f;
						shipData.stage = -2;
					}
					__instance.shipRenderers[shipData.shipIndex].anim.z = num30;
					num30 = (3f - num30 - num30) * num30 * num30;
					VectorLF3 lhs = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, __instance.shipDiskPos[shipData.shipIndex]);
					VectorLF3 lhs2 = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, shipData.pPosTemp);
					shipData.uPos = lhs * (double)(1f - num30) + lhs2 * (double)num30;
					shipData.uRot = astroPose.uRot * Quaternion.Slerp(__instance.shipDiskRot[shipData.shipIndex], shipData.pRotTemp, num30 * 2f - 1f);
				}
				shipData.uVel.x = 0f;
				shipData.uVel.y = 0f;
				shipData.uVel.z = 0f;
				shipData.uSpeed = 0f;
				shipData.uAngularVel.x = 0f;
				shipData.uAngularVel.y = 0f;
				shipData.uAngularVel.z = 0f;
				shipData.uAngularSpeed = 0f;
				goto IL_32E5;
			}
			if (shipData.stage == 0)
			{
				AstroPose astroPose2 = astroPoses[shipData.planetB];
				VectorLF3 lhs3;
				if (shipData.direction > 0)
				{
					lhs3 = astroPose2.uPos + Maths.QRotateLF(astroPose2.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * 25f);
				}
				else
				{
					lhs3 = astroPose.uPos + Maths.QRotateLF(astroPose.uRot, __instance.shipDiskPos[shipData.shipIndex] + __instance.shipDiskPos[shipData.shipIndex].normalized * 25f);
				}
				VectorLF3 vectorLF = lhs3 - shipData.uPos;
				double num31 = vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z;
				double num32 = Math.Sqrt(num31);
				VectorLF3 vectorLF2 = (shipData.direction <= 0) ? (astroPose2.uPos - shipData.uPos) : (astroPose.uPos - shipData.uPos);
				double num33 = vectorLF2.x * vectorLF2.x + vectorLF2.y * vectorLF2.y + vectorLF2.z * vectorLF2.z;
				bool flag8 = num33 <= (double)(astroPose.uRadius * astroPose.uRadius) * 2.25;
				bool flag9 = false;
				if (num32 < 6.0)
				{
					shipData.t = 1f;
					shipData.stage = shipData.direction;
					flag9 = true;
				}
				float num34 = 0f;
				if (flag)
				{
					double magnitude = (astroPose.uPos - astroPose2.uPos).magnitude;
					double num35 = magnitude * 2.0;
					double num36 = ((double)shipWarpSpeed >= num35) ? num35 : ((double)shipWarpSpeed);
					double num37 = __instance.warpEnableDist * 0.5;
					if (shipData.warpState <= 0f)
					{
						shipData.warpState = 0f;
						if (num33 > 25000000.0 && num32 > num37 && shipData.uSpeed >= shipSailSpeed && (shipData.warperCnt > 0 || ___warperFree))
						{
							shipData.warperCnt--;
							shipData.warpState += (float)dt;
						}
					}
					else
					{
						num34 = (float)(num36 * ((Math.Pow(1001.0, (double)shipData.warpState) - 1.0) / 1000.0));
						double num38 = (double)num34 * 0.0449 + 5000.0 + (double)shipSailSpeed * 0.25;
						double num39 = num32 - num38;
						if (num39 < 0.0)
						{
							num39 = 0.0;
						}
						if (num32 < num38)
						{
							shipData.warpState -= (float)(dt * 4.0);
						}
						else
						{
							shipData.warpState += (float)dt;
						}
						if (shipData.warpState < 0f)
						{
							shipData.warpState = 0f;
						}
						else if (shipData.warpState > 1f)
						{
							shipData.warpState = 1f;
						}
						if (shipData.warpState > 0f)
						{
							num34 = (float)(num36 * ((Math.Pow(1001.0, (double)shipData.warpState) - 1.0) / 1000.0));
							if ((double)num34 * dt > num39)
							{
								num34 = (float)(num39 / dt * 1.01);
							}
						}
					}
				}
				double num40 = num32 / ((double)shipData.uSpeed + 0.1) * 0.382 * (double)num25;
				float num41;
				if (shipData.warpState > 0f)
				{
					num41 = (shipData.uSpeed = shipSailSpeed + num34);
					if (num41 > shipSailSpeed)
					{
						num41 = shipSailSpeed;
					}
				}
				else
				{
					float num42 = (float)((double)shipData.uSpeed * num40) + 6f;
					if (num42 > shipSailSpeed)
					{
						num42 = shipSailSpeed;
					}
					float num43 = (float)dt * ((!flag8) ? num27 : num26);
					if (shipData.uSpeed < num42 - num43)
					{
						shipData.uSpeed += num43;
					}
					else if (shipData.uSpeed > num42 + num28)
					{
						shipData.uSpeed -= num28;
					}
					else
					{
						shipData.uSpeed = num42;
					}
					num41 = shipData.uSpeed;
				}
				int num44 = -1;
				double rhs = 0.0;
				double num45 = 1E+40;
				int num46 = shipData.planetA / 100 * 100;
				int num47 = shipData.planetB / 100 * 100;
				for (int k = num46; k < num46 + GameMain.galaxy.PlanetById(num46 + 1).star.planetCount + 1; k++)
				{
					float uRadius = astroPoses[k].uRadius;
					if (uRadius >= 1f)
					{
						VectorLF3 vectorLF3 = shipData.uPos - astroPoses[k].uPos;
						double num48 = vectorLF3.x * vectorLF3.x + vectorLF3.y * vectorLF3.y + vectorLF3.z * vectorLF3.z;
						double num49 = -((double)shipData.uVel.x * vectorLF3.x + (double)shipData.uVel.y * vectorLF3.y + (double)shipData.uVel.z * vectorLF3.z);
						if ((num49 > 0.0 || num48 < (double)(uRadius * uRadius * 7f)) && num48 < num45)
						{
							rhs = ((num49 >= 0.0) ? num49 : 0.0);
							num44 = k;
							num45 = num48;
						}
					}
				}
				if (num47 != num46)
				{
					for (int l = num47; l < num47 + GameMain.galaxy.PlanetById(num47 + 1).star.planetCount + 1; l++)
					{
						float uRadius2 = astroPoses[l].uRadius;
						if (uRadius2 >= 1f)
						{
							VectorLF3 vectorLF4 = shipData.uPos - astroPoses[l].uPos;
							double num50 = vectorLF4.x * vectorLF4.x + vectorLF4.y * vectorLF4.y + vectorLF4.z * vectorLF4.z;
							double num51 = -((double)shipData.uVel.x * vectorLF4.x + (double)shipData.uVel.y * vectorLF4.y + (double)shipData.uVel.z * vectorLF4.z);
							if ((num51 > 0.0 || num50 < (double)(uRadius2 * uRadius2 * 7f)) && num50 < num45)
							{
								rhs = ((num51 >= 0.0) ? num51 : 0.0);
								num44 = l;
								num45 = num50;
							}
						}
					}
				}
				VectorLF3 vectorLF5 = VectorLF3.zero;
				VectorLF3 rhs2 = VectorLF3.zero;
				float num52 = 0f;
				VectorLF3 vectorLF6 = Vector3.zero;
				if (num44 > 0)
				{
					float num53 = astroPoses[num44].uRadius;
					if (num44 % 100 == 0)
					{
						num53 *= 2.5f;
					}
					double num54 = Math.Max(1.0, ((astroPoses[num44].uPosNext - astroPoses[num44].uPos).magnitude - 0.5) * 0.6);
					double num55 = 1.0 + 1600.0 / (double)num53;
					double num56 = 1.0 + 250.0 / (double)num53;
					num55 *= num54 * num54;
					double num57 = (double)((num44 != shipData.planetA && num44 != shipData.planetB) ? 1.5f : 1.25f);
					double num58 = Math.Sqrt(num45);
					double num59 = (double)num53 / num58 * 1.6 - 0.1;
					if (num59 > 1.0)
					{
						num59 = 1.0;
					}
					else if (num59 < 0.0)
					{
						num59 = 0.0;
					}
					double num60 = num58 - (double)num53 * 0.82;
					if (num60 < 1.0)
					{
						num60 = 1.0;
					}
					double num61 = (double)(num41 - 6f) / (num60 * (double)num25) * 0.6 - 0.01;
					if (num61 > 1.5)
					{
						num61 = 1.5;
					}
					else if (num61 < 0.0)
					{
						num61 = 0.0;
					}
					VectorLF3 vectorLF7 = shipData.uPos + (VectorLF3)shipData.uVel * (float)rhs - astroPoses[num44].uPos;
					double num62 = vectorLF7.magnitude / (double)num53;
					if (num62 < num57)
					{
						double num63 = (num62 - 1.0) / (num57 - 1.0);
						if (num63 < 0.0)
						{
							num63 = 0.0;
						}
						num63 = 1.0 - num63 * num63;
						rhs2 = vectorLF7.normalized * (num61 * num61 * num63 * 2.0 * (double)(1f - shipData.warpState));
					}
					VectorLF3 v = shipData.uPos - astroPoses[num44].uPos;
					VectorLF3 lhs4 = new VectorLF3(v.x / num58, v.y / num58, v.z / num58);
					vectorLF5 += lhs4 * num59;
					num52 = (float)num59;
					double num64 = num58 / (double)num53;
					num64 *= num64;
					num64 = (num55 - num64) / (num55 - num56);
					if (num64 > 1.0)
					{
						num64 = 1.0;
					}
					else if (num64 < 0.0)
					{
						num64 = 0.0;
					}
					if (num64 > 0.0)
					{
						VectorLF3 v2 = Maths.QInvRotateLF(astroPoses[num44].uRot, v);
						VectorLF3 lhs5 = Maths.QRotateLF(astroPoses[num44].uRotNext, v2) + astroPoses[num44].uPosNext;
						num64 = (3.0 - num64 - num64) * num64 * num64;
						vectorLF6 = (lhs5 - shipData.uPos) * num64;
					}
				}
				Vector3 vector;
				shipData.uRot.ForwardUp(out shipData.uVel, out vector);
				Vector3 vector2 = (VectorLF3)vector * (1f - num52) + vectorLF5 * num52;
				vector2 -= Vector3.Dot(vector2, shipData.uVel) * shipData.uVel;
				vector2.Normalize();
				Vector3 vector3 = vectorLF.normalized + rhs2;
				Vector3 a = Vector3.Cross(shipData.uVel, vector3);
				float num65 = shipData.uVel.x * vector3.x + shipData.uVel.y * vector3.y + shipData.uVel.z * vector3.z;
				Vector3 a2 = Vector3.Cross(vector, vector2);
				float num66 = vector.x * vector2.x + vector.y * vector2.y + vector.z * vector2.z;
				if (num65 < 0f)
				{
					a = a.normalized;
				}
				if (num66 < 0f)
				{
					a2 = a2.normalized;
				}
				float d = (num40 >= 3.0) ? (num41 / shipSailSpeed * ((!flag8) ? 1f : 0.2f)) : ((3.25f - (float)num40) * 4f);
				a = a * d + a2 * 2f;
				Vector3 a3 = a - shipData.uAngularVel;
				float d2 = (a3.sqrMagnitude >= 0.1f) ? 0.05f : 1f;
				shipData.uAngularVel += a3 * d2;
				double num67 = (double)shipData.uSpeed * dt;
				shipData.uPos.x = shipData.uPos.x + (double)shipData.uVel.x * num67 + vectorLF6.x;
				shipData.uPos.y = shipData.uPos.y + (double)shipData.uVel.y * num67 + vectorLF6.y;
				shipData.uPos.z = shipData.uPos.z + (double)shipData.uVel.z * num67 + vectorLF6.z;
				Vector3 normalized = shipData.uAngularVel.normalized;
				float magnitude2 = shipData.uAngularVel.magnitude;
				double num68 = (double)magnitude2 * dt * 0.5;
				float w = (float)Math.Cos(num68);
				float num69 = (float)Math.Sin(num68);
				Quaternion lhs6 = new Quaternion(normalized.x * num69, normalized.y * num69, normalized.z * num69, w);
				shipData.uRot = lhs6 * shipData.uRot;
				if (shipData.warpState > 0f)
				{
					float num70 = shipData.warpState * shipData.warpState * shipData.warpState;
					shipData.uRot = Quaternion.Slerp(shipData.uRot, Quaternion.LookRotation(vector3, vector2), num70);
					shipData.uAngularVel *= 1f - num70;
				}
				if (num32 < 100.0)
				{
					float num71 = 1f - (float)num32 / 100f;
					num71 = (3f - num71 - num71) * num71 * num71;
					num71 *= num71;
					if (shipData.direction > 0)
					{
						quaternion = Quaternion.Slerp(shipData.uRot, astroPose2.uRot * (gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f)), num71);
					}
					else
					{
						Vector3 vector4 = (shipData.uPos - astroPose.uPos).normalized;
						Vector3 normalized2 = (shipData.uVel - Vector3.Dot(shipData.uVel, vector4) * vector4).normalized;
						quaternion = Quaternion.Slerp(shipData.uRot, Quaternion.LookRotation(normalized2, vector4), num71);
					}
					flag7 = true;
				}
				if (flag9)
				{
					shipData.uRot = quaternion;
					if (shipData.direction > 0)
					{
						shipData.pPosTemp = Maths.QInvRotateLF(astroPose2.uRot, shipData.uPos - astroPose2.uPos);
						shipData.pRotTemp = Quaternion.Inverse(astroPose2.uRot) * shipData.uRot;
					}
					else
					{
						shipData.pPosTemp = Maths.QInvRotateLF(astroPose.uRot, shipData.uPos - astroPose.uPos);
						shipData.pRotTemp = Quaternion.Inverse(astroPose.uRot) * shipData.uRot;
					}
					quaternion = Quaternion.identity;
					flag7 = false;
				}
				if (__instance.shipRenderers[shipData.shipIndex].anim.z > 1f)
				{
					ShipRenderingData[] array18 = __instance.shipRenderers;
					int shipIndex = shipData.shipIndex;
					array18[shipIndex].anim.z = array18[shipIndex].anim.z - (float)dt * 0.3f;
				}
				else
				{
					__instance.shipRenderers[shipData.shipIndex].anim.z = 1f;
				}
				__instance.shipRenderers[shipData.shipIndex].anim.w = shipData.warpState;
				goto IL_32E5;
			}
			if (shipData.stage == 1)
			{
				AstroPose astroPose3 = astroPoses[shipData.planetB];
				float num72;
				if (shipData.direction > 0)
				{
					shipData.t -= num29 * 0.6666667f;
					num72 = shipData.t;
					if (shipData.t < 0f)
					{
						shipData.t = 1f;
						num72 = 0f;
						shipData.stage = 2;
					}
					num72 = (3f - num72 - num72) * num72 * num72;
					float num73 = num72 * 2f;
					float num74 = num72 * 2f - 1f;
					VectorLF3 lhs7 = astroPose3.uPos + Maths.QRotateLF(astroPose3.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * 7.2700005f);
					if (num72 > 0.5f)
					{
						VectorLF3 lhs8 = astroPose3.uPos + Maths.QRotateLF(astroPose3.uRot, shipData.pPosTemp);
						shipData.uPos = lhs7 * (double)(1f - num74) + lhs8 * (double)num74;
						shipData.uRot = astroPose3.uRot * Quaternion.Slerp(gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f), shipData.pRotTemp, num74 * 1.5f - 0.5f);
					}
					else
					{
						VectorLF3 lhs9 = astroPose3.uPos + Maths.QRotateLF(astroPose3.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * -14.4f);
						shipData.uPos = lhs9 * (double)(1f - num73) + lhs7 * (double)num73;
						shipData.uRot = astroPose3.uRot * (gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
					}
				}
				else
				{
					shipData.t += num29;
					num72 = shipData.t;
					if (shipData.t > 1f)
					{
						shipData.t = 1f;
						num72 = 1f;
						shipData.stage = 0;
					}
					num72 = (3f - num72 - num72) * num72 * num72;
					shipData.uPos = astroPose3.uPos + Maths.QRotateLF(astroPose3.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * (-14.4f + 39.4f * num72));
					shipData.uRot = astroPose3.uRot * (gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
				}
				shipData.uVel.x = 0f;
				shipData.uVel.y = 0f;
				shipData.uVel.z = 0f;
				shipData.uSpeed = 0f;
				shipData.uAngularVel.x = 0f;
				shipData.uAngularVel.y = 0f;
				shipData.uAngularVel.z = 0f;
				shipData.uAngularSpeed = 0f;
				__instance.shipRenderers[shipData.shipIndex].anim.z = num72 * 1.7f - 0.7f;
				goto IL_32E5;
			}
			if (shipData.direction > 0)
			{
				shipData.t -= 0.0334f;
				if (shipData.t < 0f)
				{
					shipData.t = 0f;
					StationComponent stationComponent3 = gStationPool[shipData.otherGId];
					StationStore[] array19 = stationComponent3.storage;
					if ((astroPoses[shipData.planetA].uPos - astroPoses[shipData.planetB].uPos).sqrMagnitude > __instance.warpEnableDist * __instance.warpEnableDist && shipData.warperCnt == 0 && stationComponent3.warperCount > 0)
					{
						shipData.warperCnt++;
						stationComponent3.warperCount--;
					}
					if (shipData.itemCount > 0)
					{
						stationComponent3.AddItem(shipData.itemId, shipData.itemCount);
						shipData.itemCount = 0;
						if (__instance.workShipOrders[j].otherStationGId > 0)
						{
							if (array19[__instance.workShipOrders[j].otherIndex].itemId == __instance.workShipOrders[j].itemId)
							{
								StationStore[] array20 = array19;
								int otherIndex = __instance.workShipOrders[j].otherIndex;
								array20[otherIndex].remoteOrder = array20[otherIndex].remoteOrder - __instance.workShipOrders[j].otherOrdered;
							}
							__instance.workShipOrders[j].ClearOther();
						}
						if (__instance.remotePairCount > 0)
						{
							__instance.remotePairProcess %= __instance.remotePairCount;
							int num75 = __instance.remotePairProcess;
							int num76 = __instance.remotePairProcess;
							SupplyDemandPair supplyDemandPair3;
							for (;;)
							{
								supplyDemandPair3 = __instance.remotePairs[num76];
								if (supplyDemandPair3.demandId == __instance.gid && supplyDemandPair3.supplyId == stationComponent3.gid && __instance.storage[supplyDemandPair3.demandIndex].remoteDemandCount > 0 && __instance.storage[supplyDemandPair3.demandIndex].totalDemandCount > 0 && array19[supplyDemandPair3.supplyIndex].count >= shipCarries && array19[supplyDemandPair3.supplyIndex].remoteSupplyCount >= shipCarries && array19[supplyDemandPair3.supplyIndex].totalSupplyCount >= shipCarries)
								{
									break;
								}
								num76++;
								num76 %= __instance.remotePairCount;
								if (num75 == num76)
								{
									goto IL_2FCE;
								}
							}
							int num77 = array19[supplyDemandPair3.supplyIndex].count;
							if (num77 > shipCarries)
							{
								num77 = shipCarries;
							}
							shipData.itemId = (__instance.workShipOrders[j].itemId = __instance.storage[supplyDemandPair3.demandIndex].itemId);
							shipData.itemCount = num77;
							StationStore[] array21 = array19;
							int supplyIndex4 = supplyDemandPair3.supplyIndex;
							array21[supplyIndex4].count = array21[supplyIndex4].count - num77;
							__instance.workShipOrders[j].otherStationGId = stationComponent3.gid;
							__instance.workShipOrders[j].thisIndex = supplyDemandPair3.demandIndex;
							__instance.workShipOrders[j].otherIndex = supplyDemandPair3.supplyIndex;
							__instance.workShipOrders[j].thisOrdered = num77;
							__instance.workShipOrders[j].otherOrdered = 0;
							StationStore[] array22 = __instance.storage;
							int demandIndex4 = supplyDemandPair3.demandIndex;
							array22[demandIndex4].remoteOrder = array22[demandIndex4].remoteOrder + num77;
						}
						IL_2FCE:;
					}
					else
					{
						int itemId = shipData.itemId;
						int num78 = shipCarries;
						stationComponent3.TakeItem(ref itemId, ref num78);
						shipData.itemCount = num78;
						if (__instance.workShipOrders[j].otherStationGId > 0)
						{
							if (array19[__instance.workShipOrders[j].otherIndex].itemId == __instance.workShipOrders[j].itemId)
							{
								StationStore[] array23 = array19;
								int otherIndex2 = __instance.workShipOrders[j].otherIndex;
								array23[otherIndex2].remoteOrder = array23[otherIndex2].remoteOrder - __instance.workShipOrders[j].otherOrdered;
							}
							__instance.workShipOrders[j].ClearOther();
						}
						if (__instance.storage[__instance.workShipOrders[j].thisIndex].itemId == __instance.workShipOrders[j].itemId && __instance.workShipOrders[j].thisOrdered != num78)
						{
							int num79 = num78 - __instance.workShipOrders[j].thisOrdered;
							StationStore[] array24 = __instance.storage;
							int thisIndex2 = __instance.workShipOrders[j].thisIndex;
							array24[thisIndex2].remoteOrder = array24[thisIndex2].remoteOrder + num79;
							RemoteLogisticOrder[] array25 = __instance.workShipOrders;
							int num80 = j;
							array25[num80].thisOrdered = array25[num80].thisOrdered + num79;
						}
					}
					shipData.direction = -1;
				}
			}
			else
			{
				shipData.t += 0.0334f;
				if (shipData.t > 1f)
				{
					shipData.t = 0f;
					shipData.stage = 1;
				}
			}
			AstroPose astroPose4 = astroPoses[shipData.planetB];
			shipData.uPos = astroPose4.uPos + Maths.QRotateLF(astroPose4.uRot, gStationPool[shipData.otherGId].shipDockPos + gStationPool[shipData.otherGId].shipDockPos.normalized * -14.4f);
			shipData.uVel.x = 0f;
			shipData.uVel.y = 0f;
			shipData.uVel.z = 0f;
			shipData.uSpeed = 0f;
			shipData.uRot = astroPose4.uRot * (gStationPool[shipData.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
			shipData.uAngularVel.x = 0f;
			shipData.uAngularVel.y = 0f;
			shipData.uAngularVel.z = 0f;
			shipData.uAngularSpeed = 0f;
			shipData.pPosTemp = Vector3.zero;
			shipData.pRotTemp = Quaternion.identity;
			__instance.shipRenderers[shipData.shipIndex].anim.z = 0f;
			goto IL_32E5;
			IL_34F2:
			j++;
			continue;
			IL_32E5:
			__instance.workShipDatas[j] = shipData;
			if (flag7)
			{
				__instance.shipRenderers[shipData.shipIndex].SetPose(shipData.uPos, quaternion, relativePos, relativeRot, shipData.uVel * shipData.uSpeed, (shipData.itemCount <= 0) ? 0 : shipData.itemId);
				if (starmap)
				{
					__instance.shipUIRenderers[shipData.shipIndex].SetPose(shipData.uPos, quaternion, (float)(astroPoses[shipData.planetA].uPos - astroPoses[shipData.planetB].uPos).magnitude, shipData.uSpeed, (shipData.itemCount <= 0) ? 0 : shipData.itemId);
				}
			}
			else
			{
				__instance.shipRenderers[shipData.shipIndex].SetPose(shipData.uPos, shipData.uRot, relativePos, relativeRot, shipData.uVel * shipData.uSpeed, (shipData.itemCount <= 0) ? 0 : shipData.itemId);
				if (starmap)
				{
					__instance.shipUIRenderers[shipData.shipIndex].SetPose(shipData.uPos, shipData.uRot, (float)(astroPoses[shipData.planetA].uPos - astroPoses[shipData.planetB].uPos).magnitude, shipData.uSpeed, (shipData.itemCount <= 0) ? 0 : shipData.itemId);
				}
			}
			if (__instance.shipRenderers[shipData.shipIndex].anim.z < 0f)
			{
				__instance.shipRenderers[shipData.shipIndex].anim.z = 0f;
				goto IL_34F2;
			}
			goto IL_34F2;
		}
		__instance.ShipRenderersOnTick(astroPoses, relativePos, relativeRot);
		return false;
	    }

	    [HarmonyPrefix]
	    [HarmonyPatch(typeof(StationComponent), "InternalTickLocal",
		    typeof(int), typeof(float), typeof(float), typeof(float), typeof(int), typeof(StationComponent[]))]
	    public static bool InternalTickLocal(StationComponent __instance, int ____tmp_iter_remote, int timeGene, float dt, float power, float droneSpeed, int droneCarries, StationComponent[] stationPool)
	    {
		    __instance.energy += (long) ((int) ((float) __instance.energyPerTick * power));
		    __instance.energy -= 1000L;
		    if (__instance.energy > __instance.energyMax)
		    {
			    __instance.energy = __instance.energyMax;
		    }
		    else if (__instance.energy < 0L)
		    {
			    __instance.energy = 0L;
		    }

		    if (timeGene == __instance.gene % 20)
		    {
			    ____tmp_iter_remote++;
			    if (__instance.localPairCount > 0 && __instance.idleDroneCount > 0)
			    {
				    __instance.localPairProcess %= __instance.localPairCount;
				    int num = __instance.localPairProcess;
				    SupplyDemandPair supplyDemandPair;
				    StationComponent stationComponent;
				    float x;
				    float y;
				    float z;
				    float x2;
				    float y2;
				    float z2;
				    double num5;
				    double num6;
				    StationComponent stationComponent2;
				    float x3;
				    float y3;
				    float z3;
				    float x4;
				    float y4;
				    float z4;
				    double num9;
				    double num10;
				    for (;;)
				    {
					    int num2 = (droneCarries - 1) * __instance.deliveryDrones / 100;
					    supplyDemandPair = __instance.localPairs[__instance.localPairProcess];
					    if (supplyDemandPair.supplyId == __instance.id &&
					        __instance.storage[supplyDemandPair.supplyIndex].max <= num2)
					    {
						    num2 = __instance.storage[supplyDemandPair.supplyIndex].max - 1;
					    }

					    if (supplyDemandPair.supplyId == __instance.id &&
					        __instance.storage[supplyDemandPair.supplyIndex].count > num2 &&
					        __instance.storage[supplyDemandPair.supplyIndex].localSupplyCount > num2 &&
					        __instance.storage[supplyDemandPair.supplyIndex].totalSupplyCount > num2)
					    {
						    stationComponent = stationPool[supplyDemandPair.demandId];
						    if (stationComponent != null)
						    {
							    x = __instance.droneDock.x;
							    y = __instance.droneDock.y;
							    z = __instance.droneDock.z;
							    x2 = stationComponent.droneDock.x;
							    y2 = stationComponent.droneDock.y;
							    z2 = stationComponent.droneDock.z;
							    double num3 = Math.Sqrt((double) (x * x + y * y + z * z));
							    double num4 = Math.Sqrt((double) (x2 * x2 + y2 * y2 + z2 * z2));
							    num5 = (num3 + num4) * 0.5;
							    num6 = (double) (x * x2 + y * y2 + z * z2) / (num3 * num4);
							    if (num6 < -1.0)
							    {
								    num6 = -1.0;
							    }
							    else if (num6 > 1.0)
							    {
								    num6 = 1.0;
							    }
							    bool enforceLocalRange = _data.EnforcedLocalRangeStations.Contains(stationComponent.entityId);
							    if ((enforceLocalRange == false && num6 >= __instance.tripRangeDrones - 1E-06 
							         ||  enforceLocalRange == true && num6 >= __instance.tripRangeDrones - 1E-06 && num6 >= stationComponent.tripRangeDrones - 1E-06) &&
							        stationComponent.storage[supplyDemandPair.demandIndex].localDemandCount > 0 &&
							        stationComponent.storage[supplyDemandPair.demandIndex].totalDemandCount > 0)
							    {
								    break;
							    }
						    }
					    }

					    if (supplyDemandPair.demandId == __instance.id &&
					        __instance.storage[supplyDemandPair.demandIndex].localDemandCount > 0 &&
					        __instance.storage[supplyDemandPair.demandIndex].totalDemandCount > 0)
					    {
						    stationComponent2 = stationPool[supplyDemandPair.supplyId];
						    if (stationComponent2 != null)
						    {
							    x3 = __instance.droneDock.x;
							    y3 = __instance.droneDock.y;
							    z3 = __instance.droneDock.z;
							    x4 = stationComponent2.droneDock.x;
							    y4 = stationComponent2.droneDock.y;
							    z4 = stationComponent2.droneDock.z;
							    double num7 = Math.Sqrt((double) (x3 * x3 + y3 * y3 + z3 * z3));
							    double num8 = Math.Sqrt((double) (x4 * x4 + y4 * y4 + z4 * z4));
							    num9 = (num7 + num8) * 0.5;
							    num10 = (double) (x3 * x4 + y3 * y4 + z3 * z4) / (num7 * num8);
							    if (num10 < -1.0)
							    {
								    num10 = -1.0;
							    }
							    else if (num10 > 1.0)
							    {
								    num10 = 1.0;
							    }
							    if (stationComponent2.storage[supplyDemandPair.supplyIndex].max <= num2)
							    {
								    num2 = stationComponent2.storage[supplyDemandPair.supplyIndex].max - 1;
							    }
							    bool enforceLocalRange = _data.EnforcedLocalRangeStations.Contains(stationComponent2.entityId);
							    if ((enforceLocalRange == false && num10 >= __instance.tripRangeDrones - 1E-06 
							         || enforceLocalRange == true &&  num10 >= __instance.tripRangeDrones - 1E-06 &&  num10 >= stationComponent2.tripRangeDrones - 1E-06) &&
							        stationComponent2.storage[supplyDemandPair.supplyIndex].count > num2 &&
							        stationComponent2.storage[supplyDemandPair.supplyIndex].localSupplyCount > num2 &&
							        stationComponent2.storage[supplyDemandPair.supplyIndex].totalSupplyCount > num2)
							    {
								    goto Block_30;
							    }
						    }
					    }

					    __instance.localPairProcess++;
					    __instance.localPairProcess %= __instance.localPairCount;
					    if (num == __instance.localPairProcess)
					    {
						    goto IL_D86;
					    }
				    }

				    double num11 = (double) ((float) Math.Acos(num6));
				    double num12 = num5 * num11;
				    long num13 = (long) (num12 * 20000.0 * 2.0 + 800000.0);
				    if (__instance.energy >= num13)
				    {
					    int num14 = (droneCarries >= __instance.storage[supplyDemandPair.supplyIndex].count)
						    ? __instance.storage[supplyDemandPair.supplyIndex].count
						    : droneCarries;
					    __instance.workDroneDatas[__instance.workDroneCount].begin = new Vector3(x, y, z);
					    __instance.workDroneDatas[__instance.workDroneCount].end = new Vector3(x2, y2, z2);
					    __instance.workDroneDatas[__instance.workDroneCount].endId = stationComponent.id;
					    __instance.workDroneDatas[__instance.workDroneCount].direction = 1f;
					    __instance.workDroneDatas[__instance.workDroneCount].maxt = (float) num12;
					    __instance.workDroneDatas[__instance.workDroneCount].t = -1.5f;
					    __instance.workDroneDatas[__instance.workDroneCount].itemId =
						    (__instance.workDroneOrders[__instance.workDroneCount].itemId =
							    __instance.storage[supplyDemandPair.supplyIndex].itemId);
					    __instance.workDroneDatas[__instance.workDroneCount].itemCount = num14;
					    __instance.workDroneDatas[__instance.workDroneCount].gene = ____tmp_iter_remote;
					    __instance.workDroneOrders[__instance.workDroneCount].otherStationId = stationComponent.id;
					    __instance.workDroneOrders[__instance.workDroneCount].thisIndex = supplyDemandPair.supplyIndex;
					    __instance.workDroneOrders[__instance.workDroneCount].otherIndex = supplyDemandPair.demandIndex;
					    __instance.workDroneOrders[__instance.workDroneCount].thisOrdered = 0;
					    __instance.workDroneOrders[__instance.workDroneCount].otherOrdered = num14;
					    StationStore[] array = stationComponent.storage;
					    int demandIndex = supplyDemandPair.demandIndex;
					    array[demandIndex].localOrder = array[demandIndex].localOrder + num14;
					    __instance.workDroneCount++;
					    __instance.idleDroneCount--;
					    StationStore[] array2 = __instance.storage;
					    int supplyIndex = supplyDemandPair.supplyIndex;
					    array2[supplyIndex].count = array2[supplyIndex].count - num14;
					    __instance.energy -= num13;
				    }

				    goto IL_D86;
				    Block_30:
				    double num15 = (double) ((float) Math.Acos(num10));
				    double num16 = num9 * num15;
				    long num17 = (long) (num16 * 20000.0 * 2.0 + 800000.0);
				    bool flag = false;
				    __instance.localPairProcess %= __instance.localPairCount;
				    int num18 = __instance.localPairProcess + 1;
				    int num19 = __instance.localPairProcess;
				    num18 %= __instance.localPairCount;
				    SupplyDemandPair supplyDemandPair2;
				    for (;;)
				    {
					    supplyDemandPair2 = __instance.localPairs[num18];
					    int num2 = 0;
					    if (supplyDemandPair2.supplyId == __instance.id &&
					        supplyDemandPair2.demandId == stationComponent2.id &&
					        __instance.storage[supplyDemandPair2.supplyIndex].count > num2 &&
					        __instance.storage[supplyDemandPair2.supplyIndex].localSupplyCount > num2 &&
					        __instance.storage[supplyDemandPair2.supplyIndex].totalSupplyCount > num2 &&
					        stationComponent2.storage[supplyDemandPair2.demandIndex].localDemandCount > 0 &&
					        stationComponent2.storage[supplyDemandPair2.demandIndex].totalDemandCount > 0)
					    {
						    break;
					    }

					    num18++;
					    num18 %= __instance.localPairCount;
					    if (num19 == num18)
					    {
						    goto IL_B1E;
					    }
				    }

				    if (__instance.energy >= num17)
				    {
					    int num20 = (droneCarries >= __instance.storage[supplyDemandPair2.supplyIndex].count)
						    ? __instance.storage[supplyDemandPair2.supplyIndex].count
						    : droneCarries;
					    __instance.workDroneDatas[__instance.workDroneCount].begin = new Vector3(x3, y3, z3);
					    __instance.workDroneDatas[__instance.workDroneCount].end = new Vector3(x4, y4, z4);
					    __instance.workDroneDatas[__instance.workDroneCount].endId = stationComponent2.id;
					    __instance.workDroneDatas[__instance.workDroneCount].direction = 1f;
					    __instance.workDroneDatas[__instance.workDroneCount].maxt = (float) num16;
					    __instance.workDroneDatas[__instance.workDroneCount].t = -1.5f;
					    __instance.workDroneDatas[__instance.workDroneCount].itemId =
						    (__instance.workDroneOrders[__instance.workDroneCount].itemId =
							    __instance.storage[supplyDemandPair2.supplyIndex].itemId);
					    __instance.workDroneDatas[__instance.workDroneCount].itemCount = num20;
					    __instance.workDroneDatas[__instance.workDroneCount].gene = ____tmp_iter_remote;
					    __instance.workDroneOrders[__instance.workDroneCount].otherStationId = stationComponent2.id;
					    __instance.workDroneOrders[__instance.workDroneCount].thisIndex = supplyDemandPair2.supplyIndex;
					    __instance.workDroneOrders[__instance.workDroneCount].otherIndex = supplyDemandPair2.demandIndex;
					    __instance.workDroneOrders[__instance.workDroneCount].thisOrdered = 0;
					    __instance.workDroneOrders[__instance.workDroneCount].otherOrdered = num20;
					    StationStore[] array3 = stationComponent2.storage;
					    int demandIndex2 = supplyDemandPair2.demandIndex;
					    array3[demandIndex2].localOrder = array3[demandIndex2].localOrder + num20;
					    __instance.workDroneCount++;
					    __instance.idleDroneCount--;
					    StationStore[] array4 = __instance.storage;
					    int supplyIndex2 = supplyDemandPair2.supplyIndex;
					    array4[supplyIndex2].count = array4[supplyIndex2].count - num20;
					    __instance.energy -= num17;
					    flag = true;
				    }

				    IL_B1E:
				    if (!flag)
				    {
					    if (__instance.energy >= num17)
					    {
						    __instance.workDroneDatas[__instance.workDroneCount].begin = new Vector3(x3, y3, z3);
						    __instance.workDroneDatas[__instance.workDroneCount].end = new Vector3(x4, y4, z4);
						    __instance.workDroneDatas[__instance.workDroneCount].endId = stationComponent2.id;
						    __instance.workDroneDatas[__instance.workDroneCount].direction = 1f;
						    __instance.workDroneDatas[__instance.workDroneCount].maxt = (float) num16;
						    __instance.workDroneDatas[__instance.workDroneCount].t = -1.5f;
						    __instance.workDroneDatas[__instance.workDroneCount].itemId =
							    (__instance.workDroneOrders[__instance.workDroneCount].itemId =
								    __instance.storage[supplyDemandPair.demandIndex].itemId);
						    __instance.workDroneDatas[__instance.workDroneCount].itemCount = 0;
						    __instance.workDroneDatas[__instance.workDroneCount].gene = ____tmp_iter_remote;
						    __instance.workDroneOrders[__instance.workDroneCount].otherStationId = stationComponent2.id;
						    __instance.workDroneOrders[__instance.workDroneCount].thisIndex = supplyDemandPair.demandIndex;
						    __instance.workDroneOrders[__instance.workDroneCount].otherIndex = supplyDemandPair.supplyIndex;
						    __instance.workDroneOrders[__instance.workDroneCount].thisOrdered = droneCarries;
						    __instance.workDroneOrders[__instance.workDroneCount].otherOrdered = -droneCarries;
						    StationStore[] array5 = __instance.storage;
						    int demandIndex3 = supplyDemandPair.demandIndex;
						    array5[demandIndex3].localOrder = array5[demandIndex3].localOrder + droneCarries;
						    StationStore[] array6 = stationComponent2.storage;
						    int supplyIndex3 = supplyDemandPair.supplyIndex;
						    array6[supplyIndex3].localOrder = array6[supplyIndex3].localOrder - droneCarries;
						    __instance.workDroneCount++;
						    __instance.idleDroneCount--;
						    __instance.energy -= num17;
					    }
				    }

				    IL_D86:
				    __instance.localPairProcess++;
				    __instance.localPairProcess %= __instance.localPairCount;
			    }
		    }

		    float num21 = dt * droneSpeed;
		    for (int i = 0; i < __instance.workDroneCount; i++)
		    {
			    if (__instance.workDroneDatas[i].t > 0f && __instance.workDroneDatas[i].t < __instance.workDroneDatas[i].maxt)
			    {
				    DroneData[] array7 = __instance.workDroneDatas;
				    int num22 = i;
				    array7[num22].t = array7[num22].t + num21 * __instance.workDroneDatas[i].direction;
				    if (__instance.workDroneDatas[i].t <= 0f)
				    {
					    __instance.workDroneDatas[i].t = -0.0001f;
				    }
				    else if (__instance.workDroneDatas[i].t >= __instance.workDroneDatas[i].maxt)
				    {
					    __instance.workDroneDatas[i].t = __instance.workDroneDatas[i].maxt + 0.0001f;
				    }
			    }
			    else
			    {
				    DroneData[] array8 = __instance.workDroneDatas;
				    int num23 = i;
				    array8[num23].t = array8[num23].t + dt * __instance.workDroneDatas[i].direction;
				    if (__instance.workDroneDatas[i].t >= __instance.workDroneDatas[i].maxt + 1.5f)
				    {
					    __instance.workDroneDatas[i].direction = -1f;
					    __instance.workDroneDatas[i].t = __instance.workDroneDatas[i].maxt + 1.5f;
					    StationComponent stationComponent3 = stationPool[__instance.workDroneDatas[i].endId];
					    StationStore[] array9 = stationComponent3.storage;
					    if (__instance.workDroneDatas[i].itemCount > 0)
					    {
						    stationComponent3.AddItem(__instance.workDroneDatas[i].itemId, __instance.workDroneDatas[i].itemCount);
						    __instance.workDroneDatas[i].itemCount = 0;
						    if (__instance.workDroneOrders[i].otherStationId > 0)
						    {
							    if (array9[__instance.workDroneOrders[i].otherIndex].itemId == __instance.workDroneOrders[i].itemId)
							    {
								    StationStore[] array10 = array9;
								    int otherIndex = __instance.workDroneOrders[i].otherIndex;
								    array10[otherIndex].localOrder = array10[otherIndex].localOrder -
								                                     __instance.workDroneOrders[i].otherOrdered;
							    }

							    __instance.workDroneOrders[i].ClearOther();
						    }

						    if (__instance.localPairCount > 0)
						    {
							    __instance.localPairProcess %= __instance.localPairCount;
							    int num24 = __instance.localPairProcess;
							    int num25 = __instance.localPairProcess;
							    SupplyDemandPair supplyDemandPair3;
							    for (;;)
							    {
								    supplyDemandPair3 = __instance.localPairs[num25];
								    if (supplyDemandPair3.demandId == __instance.id &&
								        supplyDemandPair3.supplyId == stationComponent3.id &&
								        __instance.storage[supplyDemandPair3.demandIndex].localDemandCount > 0 &&
								        __instance.storage[supplyDemandPair3.demandIndex].totalDemandCount > 0 &&
								        array9[supplyDemandPair3.supplyIndex].count > 0 &&
								        array9[supplyDemandPair3.supplyIndex].localSupplyCount > 0 &&
								        array9[supplyDemandPair3.supplyIndex].totalSupplyCount > 0)
								    {
									    break;
								    }

								    num25++;
								    num25 %= __instance.localPairCount;
								    if (num24 == num25)
								    {
									    goto IL_1292;
								    }
							    }

							    int num26 = array9[supplyDemandPair3.supplyIndex].count;
							    if (num26 > droneCarries)
							    {
								    num26 = droneCarries;
							    }

							    __instance.workDroneDatas[i].itemId = (__instance.workDroneOrders[i].itemId =
								    __instance.storage[supplyDemandPair3.demandIndex].itemId);
							    __instance.workDroneDatas[i].itemCount = num26;
							    StationStore[] array11 = array9;
							    int supplyIndex4 = supplyDemandPair3.supplyIndex;
							    array11[supplyIndex4].count = array11[supplyIndex4].count - num26;
							    __instance.workDroneOrders[i].otherStationId = stationComponent3.id;
							    __instance.workDroneOrders[i].thisIndex = supplyDemandPair3.demandIndex;
							    __instance.workDroneOrders[i].otherIndex = supplyDemandPair3.supplyIndex;
							    __instance.workDroneOrders[i].thisOrdered = num26;
							    __instance.workDroneOrders[i].otherOrdered = 0;
							    StationStore[] array12 = __instance.storage;
							    int demandIndex4 = supplyDemandPair3.demandIndex;
							    array12[demandIndex4].localOrder = array12[demandIndex4].localOrder + num26;
						    }

						    IL_1292: ;
					    }
					    else
					    {
						    int itemId = __instance.workDroneDatas[i].itemId;
						    int num27 = droneCarries;
						    stationComponent3.TakeItem(ref itemId, ref num27);
						    __instance.workDroneDatas[i].itemCount = num27;
						    if (__instance.workDroneOrders[i].otherStationId > 0)
						    {
							    if (array9[__instance.workDroneOrders[i].otherIndex].itemId == __instance.workDroneOrders[i].itemId)
							    {
								    StationStore[] array13 = array9;
								    int otherIndex2 = __instance.workDroneOrders[i].otherIndex;
								    array13[otherIndex2].localOrder = array13[otherIndex2].localOrder -
								                                      __instance.workDroneOrders[i].otherOrdered;
							    }

							    __instance.workDroneOrders[i].ClearOther();
						    }

						    if (__instance.storage[__instance.workDroneOrders[i].thisIndex].itemId ==
							    __instance.workDroneOrders[i].itemId && __instance.workDroneOrders[i].thisOrdered != num27)
						    {
							    int num28 = num27 - __instance.workDroneOrders[i].thisOrdered;
							    StationStore[] array14 = __instance.storage;
							    int thisIndex = __instance.workDroneOrders[i].thisIndex;
							    array14[thisIndex].localOrder = array14[thisIndex].localOrder + num28;
							    LocalLogisticOrder[] array15 = __instance.workDroneOrders;
							    int num29 = i;
							    array15[num29].thisOrdered = array15[num29].thisOrdered + num28;
						    }
					    }
				    }

				    if (__instance.workDroneDatas[i].t < -1.5f)
				    {
					    __instance.AddItem(__instance.workDroneDatas[i].itemId, __instance.workDroneDatas[i].itemCount);
					    if (__instance.workDroneOrders[i].itemId > 0)
					    {
						    if (__instance.storage[__instance.workDroneOrders[i].thisIndex].itemId ==
						        __instance.workDroneOrders[i].itemId)
						    {
							    StationStore[] array16 = __instance.storage;
							    int thisIndex2 = __instance.workDroneOrders[i].thisIndex;
							    array16[thisIndex2].localOrder = array16[thisIndex2].localOrder -
							                                     __instance.workDroneOrders[i].thisOrdered;
						    }

						    __instance.workDroneOrders[i].ClearThis();
					    }

					    Array.Copy(__instance.workDroneDatas, i + 1, __instance.workDroneDatas, i,
						    __instance.workDroneDatas.Length - i - 1);
					    Array.Copy(__instance.workDroneOrders, i + 1, __instance.workDroneOrders, i,
						    __instance.workDroneOrders.Length - i - 1);
					    __instance.workDroneCount--;
					    __instance.idleDroneCount++;
					    Array.Clear(__instance.workDroneDatas, __instance.workDroneCount,
						    __instance.workDroneDatas.Length - __instance.workDroneCount);
					    Array.Clear(__instance.workDroneOrders, __instance.workDroneCount,
						    __instance.workDroneOrders.Length - __instance.workDroneCount);
					    i--;
				    }
			    }
		    }
		    return false;
	    }
    }
}

