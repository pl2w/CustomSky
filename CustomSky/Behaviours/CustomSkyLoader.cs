using System.IO;
using UnityEngine;

namespace CustomSky.Behaviours;

public static class CustomSkyLoader
{
    public static void ApplyCustomSkies(string dllDir)
    {
        var mgr = BetterDayNightManager.instance;
        if (!mgr) 
            return;

        var skyDir = Path.Combine(dllDir, "CustomSkies");
        if (!Directory.Exists(skyDir))
        {
            Debug.Log("[CustomSky] No CustomSkies folder found, skipping.");
            Directory.CreateDirectory(skyDir);
            return;
        }

        if (PluginConfig.UseSingleSkyTexture.Value)
        {
            var filePath = Path.Combine(skyDir, PluginConfig.SingleSkyTextureFileName.Value);
            if (!File.Exists(filePath))
            {
                Plugin.Log.LogWarning($"Single texture '{PluginConfig.SingleSkyTextureFileName.Value}' not found at {filePath}, falling back to none.");
            }
            else
            {
                var tex = LoadTexture(filePath);
                if (!tex) 
                    return;
                
                ApplySingleTexture(mgr.dayNightSkyboxTextures, tex);
                ApplySingleTexture(mgr.cloudsDayNightSkyboxTextures, tex);
                ApplySingleTexture(mgr.beachDayNightSkyboxTextures, tex);
                ApplySingleTexture(mgr.dayNightWeatherSkyboxTextures, tex);
            }
        }
        else
        {
            ApplyLayer(mgr.dayNightSkyboxTextures, skyDir, "Sky", SkySlotMap.SlotToTimeName);
            ApplyLayer(mgr.cloudsDayNightSkyboxTextures, skyDir, "Clouds", SkySlotMap.SlotToTimeName);
            ApplyLayer(mgr.beachDayNightSkyboxTextures, skyDir, "Beach", SkySlotMap.SlotToTimeName);
            ApplyLayer(mgr.dayNightWeatherSkyboxTextures, skyDir, "Weather", SkySlotMap.WeatherSlotToName);   
        }
    }

    private static void ApplyLayer(Texture2D[] targetArray, string customDir, string label, string[] slotNames)
    {
        var cache = new System.Collections.Generic.Dictionary<string, Texture2D>();

        for (var i = 0; i < targetArray.Length && i < slotNames.Length; i++)
        {
            var name = slotNames[i];

            if (!cache.TryGetValue(name, out var tex))
            {
                var filePath = Path.Combine(customDir, $"{label}_{name}.png");
                if (!File.Exists(filePath))
                    continue;

                tex = LoadTexture(filePath);
                cache[name] = tex;
            }

            if (tex)
                targetArray[i] = tex;
        }

        Plugin.Log.LogInfo($"Applied {cache.Count} custom textures to {label} layer.");
    }
    
    private static void ApplySingleTexture(Texture2D[] targetArray, Texture2D tex)
    {
        for (var i = 0; i < targetArray.Length; i++)
            targetArray[i] = tex;

        Plugin.Log.LogInfo($"Applied single texture '{tex.name}' to all {targetArray.Length} slots.");
    }

    private static Texture2D LoadTexture(string filePath)
    {
        try
        {
            var fileData = File.ReadAllBytes(filePath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(fileData))
            {
                tex.name = Path.GetFileNameWithoutExtension(filePath);
                return tex;
            }
            Plugin.Log.LogWarning($"Failed to decode image: {filePath}");
        }
        catch (System.Exception ex)
        {
            Plugin.Log.LogError($"Error loading {filePath}: {ex}");
        }
        return null;
    }
}