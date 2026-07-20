using BepInEx.Configuration;

namespace CustomSky;

public static class PluginConfig
{
    public static ConfigEntry<bool> UseSingleSkyTexture;
    public static ConfigEntry<string> SingleSkyTextureFileName;

    public static void Init(ConfigFile config)
    {
        UseSingleSkyTexture = config.Bind(
            "Sky",
            "UseSingleTextureForAllSky",
            false,
            "If true, use one texture for all sky time-of-day slots instead of per-slot textures.");

        SingleSkyTextureFileName = config.Bind(
            "Sky",
            "SingleSkyTextureFileName",
            "Sky_All.png",
            "Filename inside ./CustomSkies used for every sky slot when UseSingleTextureForAllSky is true.");
    }
}