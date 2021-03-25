using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace DistributeSpaceWarper
{
    [BepInPlugin( pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("DSPGAME.exe")]
    public class DistributeSpaceWarperPlugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger;
        public const string pluginGuid = "ShadowAngel.DSP.DistributeSpaceWarper";
        public const string pluginName = "Distribute Space Warper";
        public const string pluginVersion = "1.0.0.0";

        public void Awake()
        {
            var harmony = new Harmony(pluginGuid);

            harmony.PatchAll(typeof(DistributeSpaceWarperPlugin));
            harmony.PatchAll(typeof(Patch_StationComponent));
        }
        public void Start()
        {
            Logger = base.Logger;
            Logger.LogInfo("Loaded!");
        }
    }
}