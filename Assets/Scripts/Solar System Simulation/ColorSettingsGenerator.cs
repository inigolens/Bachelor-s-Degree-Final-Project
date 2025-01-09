using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSettingsGenerator : MonoBehaviour
{
    [SerializeField] public Shader planetMat;
    
    // Start is called before the first frame update
    void Start()
    {
        

    }
    
    public ColorSettings CreateColorSettings()
    {
        ColorSettings newColorSettings = ScriptableObject.CreateInstance<ColorSettings>();

        // Asigna los valores deseados a la nueva instancia
        newColorSettings.planetMaterial = new Material(planetMat);


        //print("Es null");
        // Configura BiomeColourSettings
        newColorSettings.biomeColourSettings = new ColorSettings.BiomeColourSettings();
        FillBiomeColourSettings(newColorSettings.biomeColourSettings);

        if (newColorSettings.oceanColor == null)
        {
            newColorSettings.oceanColor = new Gradient();
        }

        // Generar un color aleatorio una vez
        Color colorAleatorio = Random.ColorHSV();

        // Asignar colores aleatorios a porcentajes específicos
        newColorSettings.oceanColor.colorKeys = new GradientColorKey[] {
            new GradientColorKey(Random.ColorHSV(), 0.25f),      // 25%
            new GradientColorKey(colorAleatorio, 0.95f),   // 95% - Color aleatorio
            new GradientColorKey(Random.ColorHSV(), 1.0f)     // 100% - Color aleatorio
        };
        // Configura Gradient y otros valores según tus necesidades
        return newColorSettings;
    }

    public ShapeSettings CreateShapeSettings()
    {
        ShapeSettings newShapeSettings = new ShapeSettings();

        newShapeSettings.planetRadius = .5f;
        newShapeSettings.noiseLayers = new ShapeSettings.NoiseLayer[3];
        /**
         * Para la pimera capa
         *      Strength 0.2 - 0.3  / 0.0001 - 0.08
         *      Num Layers 2 / 2 - 8
         *      Base Roughnes 1 - 2 / 0.005 - 0.01
         *      Roughness 3- 6 / 0.005 - 1.7
         *      Persistence 0.1 / 0.2
         *      Min Value 0.6 
         * 
         * Segunda Capa
         *      S 0.3 - 0.8
         *      NL 4- 8
         *      BR 1 - 4
         *      R 1 - 4
         *      P 0.1
         *      M 0.1 - 0.7
         * Tercera Capa
         *      S 0.2 - 0.7
         *      NL 6
         *      BR 1 - 3.5
         *      R 1- 10
         *      P 0.1
         *      MV 0
         *      WM 1
         * 
         * 
         **/
        //Capa inicial
        newShapeSettings.noiseLayers[0] = new ShapeSettings.NoiseLayer();
        newShapeSettings.noiseLayers[0].enable = true;
        newShapeSettings.noiseLayers[0].useFirstLayerAsMask = true;
        newShapeSettings.noiseLayers[0].noiseSettings = new NoiseSettings();
        newShapeSettings.noiseLayers[0].noiseSettings.filterType = NoiseSettings.FilterType.Simple;
        newShapeSettings.noiseLayers[0].noiseSettings.simplenoisesettings = new NoiseSettings.SimpleNoiseSettings();
        newShapeSettings.noiseLayers[0].noiseSettings.simplenoisesettings.strength = Random.Range(0.2f, 0.3f);
        newShapeSettings.noiseLayers[0].noiseSettings.simplenoisesettings.numLayers = 2;
        newShapeSettings.noiseLayers[0].noiseSettings.simplenoisesettings.baseRoughnes = Random.Range(.1f, 2f);
        newShapeSettings.noiseLayers[0].noiseSettings.simplenoisesettings.roughness = Random.Range(3f, 6f);
        newShapeSettings.noiseLayers[0].noiseSettings.simplenoisesettings.persistence = 0.1f;
        newShapeSettings.noiseLayers[0].noiseSettings.simplenoisesettings.minValue = Random.Range(0.3f, 0.6f);
        //Capa intermedia
        newShapeSettings.noiseLayers[1] = new ShapeSettings.NoiseLayer();
        newShapeSettings.noiseLayers[1].enable = true;
        newShapeSettings.noiseLayers[1].useFirstLayerAsMask = true;
        newShapeSettings.noiseLayers[1].noiseSettings = new NoiseSettings();
        newShapeSettings.noiseLayers[1].noiseSettings.filterType = NoiseSettings.FilterType.Simple;
        newShapeSettings.noiseLayers[1].noiseSettings.simplenoisesettings = new NoiseSettings.SimpleNoiseSettings();
        newShapeSettings.noiseLayers[1].noiseSettings.simplenoisesettings.strength = Random.Range(0.3f, 0.8f);
        newShapeSettings.noiseLayers[1].noiseSettings.simplenoisesettings.numLayers = Random.Range(4, 8);
        newShapeSettings.noiseLayers[1].noiseSettings.simplenoisesettings.baseRoughnes = Random.Range(1f, 4f);
        newShapeSettings.noiseLayers[1].noiseSettings.simplenoisesettings.roughness = Random.Range(1f, 4f);
        newShapeSettings.noiseLayers[1].noiseSettings.simplenoisesettings.persistence = 0.1f;
        newShapeSettings.noiseLayers[1].noiseSettings.simplenoisesettings.minValue = Random.Range(0.1f, 0.7f);
        //Capa de picos
        newShapeSettings.noiseLayers[2] = new ShapeSettings.NoiseLayer();
        newShapeSettings.noiseLayers[2].enable = true;
        newShapeSettings.noiseLayers[2].useFirstLayerAsMask = true;
        newShapeSettings.noiseLayers[2].noiseSettings = new NoiseSettings();
        newShapeSettings.noiseLayers[2].noiseSettings.filterType = NoiseSettings.FilterType.Rigid;
        newShapeSettings.noiseLayers[2].noiseSettings.rigidnoisesettings = new NoiseSettings.RigidNoiseSettings();
        newShapeSettings.noiseLayers[2].noiseSettings.rigidnoisesettings.strength = Random.Range(0.2f, 0.7f);
        newShapeSettings.noiseLayers[2].noiseSettings.rigidnoisesettings.numLayers = 6;
        newShapeSettings.noiseLayers[2].noiseSettings.rigidnoisesettings.baseRoughnes = Random.Range(1f, 3.5f);
        newShapeSettings.noiseLayers[2].noiseSettings.rigidnoisesettings.roughness = Random.Range(3f, 10f);
        newShapeSettings.noiseLayers[2].noiseSettings.rigidnoisesettings.persistence = 0.1f;
        newShapeSettings.noiseLayers[2].noiseSettings.rigidnoisesettings.minValue = 0f;
        newShapeSettings.noiseLayers[2].noiseSettings.rigidnoisesettings.weightMultiplayer = .8f;

        return newShapeSettings;
    }

    private void FillBiomeColourSettings(ColorSettings.BiomeColourSettings b)
    {
        // Genera Biomes
        b.biomes = new ColorSettings.BiomeColourSettings.Biome[Random.Range(1, 5)];
        for (int i = 0; i < b.biomes.Length; i++)
        {
            ColorSettings.BiomeColourSettings.Biome biome = new ColorSettings.BiomeColourSettings.Biome();
            biome.startHeight = 1f / b.biomes.Length * (i);
            biome.tintPercent = 0;

            // Rellenar el Gradient con al menos dos puntos
            Gradient gradientBiome = new Gradient();
            gradientBiome.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Random.ColorHSV(), 0f), new GradientColorKey(Random.ColorHSV(), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );

            biome.gradient = gradientBiome;
            b.biomes[i] = biome;
        }

        // Los Noise Settings
        b.noise = new NoiseSettings();
        b.noise.filterType = NoiseSettings.FilterType.Simple;
        b.noise.simplenoisesettings = new NoiseSettings.SimpleNoiseSettings();  // Instanciar SimpleNoiseSettings
        b.noise.simplenoisesettings.strength = 1;  // Asignar la propiedad strength de SimpleNoiseSettings
        b.noise.simplenoisesettings.numLayers = 4;
        b.noise.simplenoisesettings.minValue = 0;
        b.noise.simplenoisesettings.roughness = Random.Range(0.8f, 1.6f);
        b.noise.simplenoisesettings.persistence = 1;

        // Otros ajustes
        b.blendAmount = 0.15f;
        b.noiseOffset = Random.Range(1.5f, 3.5f);
        b.noiseStrength = 0.1f;


    }

    void FillGradientWithRandomColors(Gradient g)
    {
        int width = 50;
        int height = 1;
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float normalizedX = (float)x / width;
                float normalizedY = (float)y / height;

                Color randomColor = g.Evaluate(Random.Range(0f, 1f));

                texture.SetPixel(x, y, randomColor);
            }
        }

        texture.Apply();
    }
}
