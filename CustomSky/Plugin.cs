using System.Collections;
using System.IO;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using BepInEx.Logging;
using CustomSky.Behaviours;
using UnityEngine;

namespace CustomSky;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log;

    private bool _initialized, _playerLoaded;
    private bool _isAlreadyDynalit;

    private Coroutine _initCoro;
    
    public Plugin()
    {
        Log = Logger;

        PluginConfig.Init(Config);

        var harmony = new Harmony(PluginInfo.Guid);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        GorillaTagger.OnPlayerSpawned(delegate
        {
            _playerLoaded = true;
            Initialize();
        });
    }

    private void Initialize()
    {
        if(_initialized)
            return;

        var dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        _isAlreadyDynalit = GameLightingManager.instance.customVertexLightingEnabled;
        _initCoro = StartCoroutine(InitCustomSkies(dllDir));
    }

    private IEnumerator InitCustomSkies(string dllDir)
    {
        var exportDir = Path.Combine(dllDir, "SkyboxExport");
        
        if (!Directory.Exists(exportDir))
            yield return SkyboxExporter.ExportAllCoroutine(exportDir);

        yield return CustomSkyLoader.ApplyCustomSkiesCoroutine(dllDir);

        if (!_isAlreadyDynalit)
        {
            GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
            
            var currentDominantSky = BetterDayNightManager.instance.currentLerp < 0.5f
                ? BetterDayNightManager.instance.fromSky
                : BetterDayNightManager.instance.toSky;

            var color = Utils.ColorUtils.GetDominantColor(currentDominantSky);
            
            GameLightingManager.instance.SetAmbientLightDynamic(color);
        }
    
        _initialized = true;
    }

    public void OnEnable()
    {
        if(_playerLoaded)
            Initialize();
    }

    public void OnDisable()
    {
        if (_initCoro != null)
        {
            StopCoroutine(_initCoro);
            _initCoro = null;
        }

        if (_initialized)
        {
            if (!_isAlreadyDynalit)
            {
                GameLightingManager.instance.SetCustomDynamicLightingEnabled(false);
            }

            CustomSkyLoader.DisableCustomSkies();
        }

        _initialized = false;
    }
}

public static class PluginInfo
{
    public const string Guid = "xyz.pl2w.customsky";
    public const string Name = "CustomSky";
    public const string Version = "1.1.0";
}