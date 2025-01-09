using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ColourGenerator
{
    ColorSettings colorSettings;
    Texture2D texture;
    const int textureResolution = 50;
    INoiseFilter biomeNoiseFilter;
    public void UpdateSettings(ColorSettings colorSettings)
    {
        this.colorSettings = colorSettings;
        if(texture == null || texture.height != colorSettings.biomeColourSettings.biomes.Length)
        {
            texture = new Texture2D(textureResolution*2, colorSettings.biomeColourSettings.biomes.Length, TextureFormat.RGBA32, false);
        }
        biomeNoiseFilter = NoiseFilterFactory.CreateNoiseFilter(colorSettings.biomeColourSettings.noise);
        
    }

    public void UpdateElevationMinMax(MinMax minMax)
    {
        colorSettings.planetMaterial.SetVector("_elevationMinMax", new Vector4(minMax.min, minMax.max));
    }
    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
    {
        float heightPercent = (pointOnUnitSphere.y + 1) / 2f;
        heightPercent += (biomeNoiseFilter.Evaluate(pointOnUnitSphere) - colorSettings.biomeColourSettings.noiseOffset) * colorSettings.biomeColourSettings.noiseStrength;
        float biomeIndex = 0;
        int numBiomes = colorSettings.biomeColourSettings.biomes.Length;
        float blendRange = colorSettings.biomeColourSettings.blendAmount/2f + .001f;
        for(int i = 0; i < numBiomes; i++)
        {
            float dst = heightPercent - colorSettings.biomeColourSettings.biomes[i].startHeight;
            float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);
            biomeIndex *= (1 - weight);
            biomeIndex += i * weight;
        }
        return biomeIndex / Mathf.Max(1, numBiomes - 1);
    }
    public void UpdateColours()
    {
        Color[] colors = new Color[texture.width * texture.height];
        int colourIndex = 0;
        foreach (var biome in colorSettings.biomeColourSettings.biomes)
        {
            for (int i = 0; i < textureResolution * 2; i++)
            {
                Color gradientCol;
                if (i < textureResolution)
                {
                    gradientCol = colorSettings.oceanColor.Evaluate(i / (textureResolution - 1f));
                }
                else
                {
                    gradientCol = biome.gradient.Evaluate((i - textureResolution) / (textureResolution - 1f));
                }
                Color tintCol = biome.tint;
                colors[colourIndex] = gradientCol * (1 - biome.tintPercent) + tintCol * biome.tintPercent;
                colourIndex++;
            }
        }

        
        texture.SetPixels(colors);
        texture.Apply();
        colorSettings.planetMaterial.SetTexture("_planetTexture", texture);
    }
}
