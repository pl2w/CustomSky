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
    
    public Plugin()
    {
        Log = Logger;
        
        PluginConfig.Init(Config);
        
        var harmony = new Harmony(PluginInfo.Guid);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        
        GorillaTagger.OnPlayerSpawned(Initialize);
    }

    private void Initialize()
    {
        var dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var exportDir = Path.Combine(dllDir, "SkyboxExport");
        
        if(!Directory.Exists(exportDir))
            StartCoroutine(SkyboxExporter.ExportAllCoroutine(exportDir));
        
        CustomSkyLoader.ApplyCustomSkies(dllDir);
    }
}

public static class PluginInfo
{
    public const string Guid = "xyz.pl2w.customsky";
    public const string Name = "CustomSky";
    public const string Version = "1.0.0";
}