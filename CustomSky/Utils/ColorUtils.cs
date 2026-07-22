using System.Collections.Generic;
using UnityEngine;

namespace CustomSky.Utils;

public static class ColorUtils
{
    public static Color GetDominantColor(Texture2D source, int sampleSize = 32, int quantizeStep = 16)
    {
        var small = ScaleTexture(source, sampleSize, sampleSize);
        var pixels = small.GetPixels32();

        var counts = new Dictionary<int, int>();
        var samples = new Dictionary<int, Color32>();

        foreach (var p in pixels)
        {
            var luminance = (0.299f * p.r + 0.587f * p.g + 0.114f * p.b) / 255f;

            if (luminance is < 0.05f or > 0.95f)
                continue;
            
            var key = ((p.r / quantizeStep) << 16) | ((p.g / quantizeStep) << 8) | (p.b / quantizeStep);
            
            if (counts.TryGetValue(key, out var c))
                counts[key] = c + 1;
            else
            {
                counts[key] = 1;
                samples[key] = p;
            }
        }

        Color result;
        if (samples.Count == 0)
        {
            result = AveragePixels(pixels);
        }
        else
        {
            int bestKey = 0, bestCount = 0;
            foreach (var kvp in counts)
            {
                if (kvp.Value <= bestCount)
                    continue;

                bestCount = kvp.Value;
                bestKey = kvp.Key;
            }

            result = samples[bestKey];
        }

        Object.Destroy(small);
        return result;
    }

    private static Texture2D ScaleTexture(Texture2D src, int width, int height)
    {
        var rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(src, rt);

        var prev = RenderTexture.active;
        RenderTexture.active = rt;

        var result = new Texture2D(width, height, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
    
    private static Color AveragePixels(Color32[] pixels)
    {
        if (pixels.Length == 0)
            return Color.gray;

        float r = 0, g = 0, b = 0;
        foreach (var p in pixels)
        {
            r += p.r;
            g += p.g;
            b += p.b;
        }

        var count = pixels.Length;
        return new Color(r / count / 255f, g / count / 255f, b / count / 255f);
    }
}