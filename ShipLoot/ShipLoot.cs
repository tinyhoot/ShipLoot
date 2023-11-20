using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ShipLoot
{
    [BepInPlugin(GUID, NAME, VERSION)]
    internal class ShipLoot : BaseUnityPlugin
    {
        public const string GUID = "com.github.tinyhoot.ShipLoot";
        public const string NAME = "ShipLoot";
        public const string VERSION = "1.0";
        
        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}