using System.Collections;
using System.IO;
using CustomSky.Behaviours;
using UnityEngine;

namespace CustomSky;

public static class SkyboxExporter
{
    public static IEnumerator ExportAllCoroutine(string exportDir)
    {
        var mgr = BetterDayNightManager.instance;
        if (!mgr)
            yield break;

        Directory.CreateDirectory(exportDir);

        yield return ExportLayerCoroutine(mgr.dayNightSkyboxTextures, exportDir, "Sky", SkySlotMap.SlotToTimeName);
        yield return ExportLayerCoroutine(mgr.cloudsDayNightSkyboxTextures, exportDir, "Clouds", SkySlotMap.SlotToTimeName);
        yield return ExportLayerCoroutine(mgr.beachDayNightSkyboxTextures, exportDir, "Beach", SkySlotMap.SlotToTimeName);
        yield return ExportLayerCoroutine(mgr.dayNightWeatherSkyboxTextures, exportDir, "Weather", SkySlotMap.WeatherSlotToName);

        Plugin.Log.LogInfo($"Exported skyboxes to {exportDir}");
    }
    
    private static IEnumerator ExportLayerCoroutine(Texture2D[] textures, string outDir, string label, string[] slotNames)
    {
        var written = new System.Collections.Generic.HashSet<string>();

        for (var i = 0; i < textures.Length && i < slotNames.Length; i++)
        {
            var tex = textures[i];
            if (!tex)
                continue;

            var name = slotNames[i];
            if (!written.Add(name)) continue;

            var png = ToReadablePNG(tex);
            File.WriteAllBytes(Path.Combine(outDir, $"{label}_{name}.png"), png);

            yield return null;
        }
    }

    private static byte[] ToReadablePNG(Texture2D source)
    {
        var rt = RenderTexture.GetTemporary(
            source.width, source.height, 0,
            RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        Graphics.Blit(source, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;

        var readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        readable.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        var png = readable.EncodeToPNG();
        Object.Destroy(readable);
        return png;
    }
}