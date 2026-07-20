using System.Collections;
using System.IO;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using BepInEx.Logging;
using CustomSky.Behaviours;

namespace CustomSky;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log;
    
    private bool _initialized, _playerLoaded;
    
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
        var exportDir = Path.Combine(dllDir, "SkyboxExport");
        
        if(!Directory.Exists(exportDir))
            StartCoroutine(SkyboxExporter.ExportAllCoroutine(exportDir));
        
        StartCoroutine(InitCustomSkies(dllDir));
    }

    private IEnumerator InitCustomSkies(string dllDir)
    {
        yield return CustomSkyLoader.ApplyCustomSkiesCoroutine(dllDir);
        _initialized = true;
    }

    public void OnEnable()
    {
        if(_playerLoaded)
            Initialize();
    }
    
    public void OnDisable()
    {
        CustomSkyLoader.DisableCustomSkies();
        
        _initialized = false;
    }
}

public static class PluginInfo
{
    public const string Guid = "xyz.pl2w.customsky";
    public const string Name = "CustomSky";
    public const string Version = "1.0.1";
}