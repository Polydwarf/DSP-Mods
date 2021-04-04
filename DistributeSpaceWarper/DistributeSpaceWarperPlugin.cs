using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace DistributeSpaceWarper
{
    [BepInPlugin( PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("DSPGAME.exe")]
    public class DistributeSpaceWarperPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _logger;
        private const string PluginGuid = "ShadowAngel.DSP.DistributeSpaceWarper";
        private const string PluginName = "Distribute Space Warper";
        private const string PluginVersion = "1.0.3.0";

        public void Awake()
        {
            Harmony harmony = new Harmony(PluginGuid);
            DistributeSpaceWarper.Config.Init(Config);
            harmony.PatchAll(typeof(DistributeSpaceWarperPlugin));
            harmony.PatchAll(typeof(PatchStationComponent));
        }
        public void Start()
        {
            _logger = base.Logger;
            _logger.LogInfo("Loaded!");
        }
    }
}