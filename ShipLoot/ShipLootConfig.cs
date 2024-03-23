using BepInEx.Configuration;

namespace ShipLoot
{
    internal class ShipLootConfig
    {
        private readonly ConfigFile _configFile;
        
        public ConfigEntry<float> DisplayTime;

        public ShipLootConfig(ConfigFile configFile)
        {
            _configFile = configFile;
        }
        
        public void RegisterOptions()
        {
            DisplayTime = _configFile.Bind(
                "General", 
                nameof(DisplayTime), 
                5f,
                new ConfigDescription("How long to display the total scrap value for, counted in seconds.",
                    new AcceptableValueRange<float>(1f, 30f)));
        }
    }
}