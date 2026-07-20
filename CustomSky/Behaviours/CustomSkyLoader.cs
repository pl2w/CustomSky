using System.IO;
using UnityEngine;

namespace CustomSky.Behaviours;

public static class CustomSkyLoader
{
    private static readonly string[] SlotToTimeName = ["night", "sunrise", "sunrise", "day", "day", "day", "sunset", "sunset", "night", "night"];

    private static readonly string[] WeatherSlotToName =
    [
        "overcastnight", "overcastnight", "overcast", "overcast", "overcast",
          "overcast", "overcast", "overcastnight", "overcastnight", "overcastnight"
    ];

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

        ApplyLayer(mgr.dayNightSkyboxTextures, skyDir, "Sky", SlotToTimeName);
        ApplyLayer(mgr.cloudsDayNightSkyboxTextures, skyDir, "Clouds", SlotToTimeName);
        ApplyLayer(mgr.beachDayNightSkyboxTextures, skyDir, "Beach", SlotToTimeName);
        ApplyLayer(mgr.dayNightWeatherSkyboxTextures, skyDir, "Weather", WeatherSlotToName);
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