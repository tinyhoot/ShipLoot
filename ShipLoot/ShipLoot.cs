using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace ShipLoot
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
    internal class ShipLoot : BaseUnityPlugin
    {
        public const string GUID = "com.github.tinyhoot.ShipLoot";
        public const string NAME = "ShipLoot";
        public const string VERSION = "1.0";

        internal new static ShipLootConfig Config;
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Config = new ShipLootConfig(base.Config);
            Config.RegisterOptions();
            
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            SetLobbyCompatibility();
        }

        /// <summary>
        /// Register the compatibility level of this mod with TeamBMX's LobbyCompatibility.
        /// All of it done via reflection to avoid hard dependencies.
        /// </summary>
        private void SetLobbyCompatibility()
        {
            // Do nothing if the user did not install that mod.
            if (!Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility"))
                return;
            
            // Try to find the public API method.
            var method = AccessTools.Method("LobbyCompatibility.Features.PluginHelper:RegisterPlugin");
            if (method is null)
            {
                Log.LogWarning("Found LobbyCompatibility mod but failed to find plugin register API method!");
                return;
            }
            
            Log.LogDebug("Registering compatibility with LobbyCompatibility.");
            try
            {
                // The register method uses enums as its last two parameters. 0, 0 stands for client side mod, no
                // version check.
                method.Invoke(null, new object[] { GUID, new Version(VERSION), 0, 0 });
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to register plugin compatibility with LobbyCompatibility.\n{ex}");
                return;
            }
            
            Log.LogDebug("Successfully registered with LobbyCompatibility.");
        }
    }
}