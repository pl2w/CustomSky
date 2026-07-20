using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomSky.Behaviours;

public static class CustomSkyLoader
{
    private static readonly Dictionary<Texture2D[], Texture2D[]> Originals = new();

    public static IEnumerator ApplyCustomSkiesCoroutine(string dllDir)
    {
        var mgr = BetterDayNightManager.instance;
        if (!mgr) 
            yield break;

        BackupOriginals();

        var skyDir = Path.Combine(dllDir, "CustomSkies");
        if (!Directory.Exists(skyDir))
        {
            Plugin.Log.LogInfo("No CustomSkies folder found, skipping.");
            Directory.CreateDirectory(skyDir);
            yield break;
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
                Texture2D tex = null;
                yield return LoadTextureAsync(filePath, t => tex = t);
                if (!tex) 
                    yield break;
                
                ApplySingleTexture(mgr.dayNightSkyboxTextures, tex);
                ApplySingleTexture(mgr.cloudsDayNightSkyboxTextures, tex);
                ApplySingleTexture(mgr.beachDayNightSkyboxTextures, tex);
                ApplySingleTexture(mgr.dayNightWeatherSkyboxTextures, tex);
            }
        }
        else
        {
            yield return ApplyLayerCoroutine(mgr.dayNightSkyboxTextures, skyDir, "Sky", SkySlotMap.SlotToTimeName);
            yield return ApplyLayerCoroutine(mgr.cloudsDayNightSkyboxTextures, skyDir, "Clouds", SkySlotMap.SlotToTimeName);
            yield return ApplyLayerCoroutine(mgr.beachDayNightSkyboxTextures, skyDir, "Beach", SkySlotMap.SlotToTimeName);
            yield return ApplyLayerCoroutine(mgr.dayNightWeatherSkyboxTextures, skyDir, "Weather", SkySlotMap.WeatherSlotToName);   
        }

        LightRefresh();
    }

    private static IEnumerator ApplyLayerCoroutine(Texture2D[] targetArray, string customDir, string label, string[] slotNames)
    {
        var cache = new Dictionary<string, Texture2D>();

        for (var i = 0; i < targetArray.Length && i < slotNames.Length; i++)
        {
            var name = slotNames[i];

            if (!cache.TryGetValue(name, out var tex))
            {
                var filePath = Path.Combine(customDir, $"{label}_{name}.png");
                if (!File.Exists(filePath))
                    continue;

                yield return LoadTextureAsync(filePath, t => tex = t);
                if (!tex)
                    continue;

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

    private static IEnumerator LoadTextureAsync(string filePath, System.Action<Texture2D> onComplete)
    {
        using var uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture("file:///" + filePath);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            var tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
            tex.name = Path.GetFileNameWithoutExtension(filePath);
            onComplete?.Invoke(tex);
        }
        else
        {
            Plugin.Log.LogWarning($"Failed to load {filePath}: {uwr.error}");
            onComplete?.Invoke(null);
        }
    }

    private static void BackupOriginals()
    {
        var arrays = new[]
        {
            BetterDayNightManager.instance.dayNightSkyboxTextures,
            BetterDayNightManager.instance.cloudsDayNightSkyboxTextures,
            BetterDayNightManager.instance.beachDayNightSkyboxTextures,
            BetterDayNightManager.instance.dayNightWeatherSkyboxTextures
        };

        foreach (var arr in arrays)
        {
            if (arr == null || Originals.ContainsKey(arr))
                continue;

            var backup = new Texture2D[arr.Length];
            for (var i = 0; i < arr.Length; i++)
                backup[i] = arr[i];

            Originals[arr] = backup;
        }
    }

    public static void DisableCustomSkies()
    {
        var mgr = BetterDayNightManager.instance;
        if (!mgr)
            return;

        var arrays = new[]
        {
            mgr.dayNightSkyboxTextures,
            mgr.cloudsDayNightSkyboxTextures,
            mgr.beachDayNightSkyboxTextures,
            mgr.dayNightWeatherSkyboxTextures
        };

        var restored = 0;
        foreach (var arr in arrays)
        {
            if (arr == null || !Originals.TryGetValue(arr, out var backup))
                continue;

            for (var i = 0; i < arr.Length && i < backup.Length; i++)
                arr[i] = backup[i];

            Originals.Remove(arr);
            restored++;
        }

        Plugin.Log.LogInfo($"Disabled custom skies, restored {restored} texture layers.");

        LightRefresh();
    }

    private static void LightRefresh()
    {
        var mgr = BetterDayNightManager.instance;
        if (!mgr) 
            return;
        
        var idx = mgr.currentTimeIndex;
        mgr.ChangeMaps(idx, (idx + 1) % BetterDayNightManager.TIME_OF_DAY_COUNT);
    }
}