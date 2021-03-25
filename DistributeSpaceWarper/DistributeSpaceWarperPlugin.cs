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

        public void Start()
        {
            Logger = base.Logger;
            DistributeSpaceWarper.Config.Init(Config);
            Harmony.CreateAndPatchAll(typeof(Patch_StationComponent));
            Logger.LogInfo("Loaded!");
        }
    }
}