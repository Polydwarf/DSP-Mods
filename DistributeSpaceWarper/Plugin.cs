using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace DistributeSpaceWarper
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("DSPGAME.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource _logger;
        private const string PluginGuid = "PD820.DSP.DistributeSpaceWarper.Remix";
        private const string PluginName = "Distribute Space Warper Remix";
        private const string PluginVersion = "1.0.0.0";

        public void Awake()
        {
            _logger = base.Logger;
            Harmony harmony = new Harmony(PluginGuid);
            DistributeSpaceWarper.Config.Init(Config);
            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(Patch));
            _logger.LogMessage("Awaken the Remixed distributed space warper");
        }
        public void Start()
        {
            _logger.LogMessage("Start the Remixed distributed space warper");
        }
    }
}