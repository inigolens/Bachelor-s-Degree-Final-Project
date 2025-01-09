using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum FilterType
    {
        Simple,
        Rigid
    };
    public FilterType filterType;
    [ConditionalHide("filterType", 0)]
    public SimpleNoiseSettings simplenoisesettings;
    [ConditionalHide("filterType", 1)]
    public RigidNoiseSettings rigidnoisesettings;
    [System.Serializable]
    public class SimpleNoiseSettings
    {
        public float strength = 1;
        [Range(1, 8)]
        public int numLayers = 1;
        public float baseRoughnes = 1;
        public float roughness = 2;
        public float persistence = .5f;
        public Vector3 center;
        public float minValue;
    }
    [System.Serializable]
    public class RigidNoiseSettings : SimpleNoiseSettings
    {
        public float weightMultiplayer = .8f;
    }

}

public struct NoiseSettingsShaderPrep
{
    public int filterType;
    public float strength;
    public int numLayers;
    public float baseRoughnes;
    public float roughness;
    public float persistence;
    public float centerX;
    public float centerY;
    public float centerZ;
    public float minValue;
    public float weightMultiplayer;
    public int useFirstLayer;
}



public static class NoiseSettingsConverter
{
    public static List<NoiseSettingsShaderPrep> ConvertToShaderPrepList(ShapeSettings shapeSettings)
    {
        List<NoiseSettingsShaderPrep> shaderPrepList = new List<NoiseSettingsShaderPrep>();

        foreach (ShapeSettings.NoiseLayer layer in shapeSettings.noiseLayers)
        {
            if (!layer.enable) continue; // Si la capa no está habilitada, la saltamos.

            NoiseSettingsShaderPrep prep = new NoiseSettingsShaderPrep();

            var noiseSettings = layer.noiseSettings;
            //// Asignamos valores comunes.
            //prep.strength = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.strength, 0.2f, 0.8f, 0.0001f, 0.08f)  : Remap(noiseSettings.rigidnoisesettings.strength, 0.2f, 0.8f, 0.0001f, 0.08f);
            //prep.numLayers = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.numLayers : noiseSettings.rigidnoisesettings.numLayers;
            //prep.baseRoughnes = noiseSettings.filterType == NoiseSettings.FilterType.Simple ?  Remap(noiseSettings.simplenoisesettings.baseRoughnes, 1, 4, 0.005f, 0.01f) :  Remap(noiseSettings.rigidnoisesettings.baseRoughnes, 1, 4, 0.005f, 0.01f);
            //prep.roughness = noiseSettings.filterType == NoiseSettings.FilterType.Simple ?  Remap(noiseSettings.simplenoisesettings.roughness, 1, 10, 0.005f, 1.7f) : Remap(noiseSettings.rigidnoisesettings.roughness, 1, 10, 0.005f, 1.7f);
            //prep.persistence = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.persistence : noiseSettings.rigidnoisesettings.persistence;
            //prep.centerX = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.x : (int)noiseSettings.rigidnoisesettings.center.x;
            //prep.centerY = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.y : (int)noiseSettings.rigidnoisesettings.center.y;
            //prep.centerZ = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.z : (int)noiseSettings.rigidnoisesettings.center.z;
            //prep.minValue = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.minValue - 0.5f : noiseSettings.rigidnoisesettings.minValue - 0.5f;

            prep.strength = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.strength : noiseSettings.rigidnoisesettings.strength;
            prep.numLayers = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.numLayers : noiseSettings.rigidnoisesettings.numLayers;
            prep.baseRoughnes = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.baseRoughnes : noiseSettings.rigidnoisesettings.baseRoughnes;
            prep.roughness = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.roughness : noiseSettings.rigidnoisesettings.roughness;
            prep.persistence = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.persistence : noiseSettings.rigidnoisesettings.persistence;
            prep.centerX = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.x : (int)noiseSettings.rigidnoisesettings.center.x;
            prep.centerY = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.y : (int)noiseSettings.rigidnoisesettings.center.y;
            prep.centerZ = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.z : (int)noiseSettings.rigidnoisesettings.center.z;
            prep.minValue = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.minValue : noiseSettings.rigidnoisesettings.minValue;
            prep.useFirstLayer = layer.useFirstLayerAsMask == true? 0:0;
            // Especificamos el tipo de filtro.
            prep.filterType = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? 0 : 1;

            // Si el filtro es Rígido, asignamos weightMultiplier.
            if (noiseSettings.filterType == NoiseSettings.FilterType.Rigid)
            {
                prep.weightMultiplayer = noiseSettings.rigidnoisesettings.weightMultiplayer;
            }
            else
            {
                // Si el filtro es Simple, podríamos decidir asignar un valor predeterminado o dejarlo como está.
                prep.weightMultiplayer = 0; // Ejemplo de valor predeterminado, ajusta según sea necesario.
            }

            shaderPrepList.Add(prep);
        }

        return shaderPrepList;
    }

    //static float Remap(float value, float from1, float from2, float to1, float to2)
    //{
    //    Debug.Log("Value: " + value + " from1: " + from1 + " from2: " + from2+ " to1 " + to1 + " to2 "+ to2 + " Result: " + from2 + (value - from1) * (to2 - from2) / (to1 - from1));
    //    return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
    //}


    public static List<NoiseSettingsShaderPrep> ConvertToShaderPrepListByLayers(ShapeSettings shapeSettings)
    {
        List<NoiseSettingsShaderPrep> shaderPrepList = new List<NoiseSettingsShaderPrep>();

        ShapeSettings.NoiseLayer layer = shapeSettings.noiseLayers[0];
        NoiseSettingsShaderPrep prep = new NoiseSettingsShaderPrep();
        if (layer.enable)
        {

            var noiseSettings = layer.noiseSettings;
            // Asignamos valores comunes.
            prep.strength = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.strength, 0.2f, 0.3f, 0.7f, 0.8f) : Remap(noiseSettings.rigidnoisesettings.strength, 0.2f, 0.8f, 0.0001f, 0.08f);
            prep.numLayers = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.numLayers + 4 : noiseSettings.rigidnoisesettings.numLayers;
            prep.baseRoughnes = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.baseRoughnes, 1, 4, 0.005f, 0.01f) : Remap(noiseSettings.rigidnoisesettings.baseRoughnes, 1, 4, 0.005f, 0.01f);
            prep.roughness = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.roughness, 1, 10, 0.005f, 1.4f) : Remap(noiseSettings.rigidnoisesettings.roughness, 1, 10, 0.005f, 1.7f);
            prep.persistence = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.persistence : noiseSettings.rigidnoisesettings.persistence;
            prep.centerX = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.x : (int)noiseSettings.rigidnoisesettings.center.x;
            prep.centerY = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.y : (int)noiseSettings.rigidnoisesettings.center.y;
            prep.centerZ = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.z : (int)noiseSettings.rigidnoisesettings.center.z;
            prep.minValue = noiseSettings.filterType == NoiseSettings.FilterType.Simple ?  Remap(noiseSettings.simplenoisesettings.strength, 0.2f, 0.3f, 0.25f, 0.6f) : noiseSettings.rigidnoisesettings.minValue - 0.5f;

            // Especificamos el tipo de filtro.
            prep.filterType = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? 0 : 1;

            // Si el filtro es Rígido, asignamos weightMultiplier.
            if (noiseSettings.filterType == NoiseSettings.FilterType.Rigid)
            {
                prep.weightMultiplayer = noiseSettings.rigidnoisesettings.weightMultiplayer;
            }
            else
            {
                // Si el filtro es Simple, podríamos decidir asignar un valor predeterminado o dejarlo como está.
                prep.weightMultiplayer = 0; // Ejemplo de valor predeterminado, ajusta según sea necesario.
            }

            prep.useFirstLayer = layer.useFirstLayerAsMask == true ? 0 : 0;
        }


        shaderPrepList.Add(prep);

        layer = shapeSettings.noiseLayers[1];
        prep = new NoiseSettingsShaderPrep();
        if (layer.enable)
        {

            var noiseSettings = layer.noiseSettings;
            // Asignamos valores comunes.
            prep.strength = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.strength, 0.3f, 0.8f, 0f, 1f) : Remap(noiseSettings.rigidnoisesettings.strength, 0.2f, 0.8f, 0.0001f, 0.08f);
            prep.numLayers = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.numLayers : noiseSettings.rigidnoisesettings.numLayers;
            prep.baseRoughnes = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.baseRoughnes, 1, 4, 0.01f, 0.05f) : Remap(noiseSettings.rigidnoisesettings.baseRoughnes, 1, 4, 0.005f, 0.01f);
            prep.roughness = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.roughness, 1, 4, 1f, 3.5f) : Remap(noiseSettings.rigidnoisesettings.roughness, 3, 6, 1.5f, 1.7f);
            prep.persistence = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.persistence : noiseSettings.rigidnoisesettings.persistence;
            prep.centerX = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.x : (int)noiseSettings.rigidnoisesettings.center.x;
            prep.centerY = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.y + 10000 : (int)noiseSettings.rigidnoisesettings.center.y;
            prep.centerZ = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.z : (int)noiseSettings.rigidnoisesettings.center.z;
            prep.minValue = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.minValue : noiseSettings.rigidnoisesettings.minValue - 0.5f;

            // Especificamos el tipo de filtro.
            prep.filterType = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? 0 : 1;

            // Si el filtro es Rígido, asignamos weightMultiplier.
            if (noiseSettings.filterType == NoiseSettings.FilterType.Rigid)
            {
                prep.weightMultiplayer = noiseSettings.rigidnoisesettings.weightMultiplayer;
            }
            else
            {
                // Si el filtro es Simple, podríamos decidir asignar un valor predeterminado o dejarlo como está.
                prep.weightMultiplayer = 0; // Ejemplo de valor predeterminado, ajusta según sea necesario.
            }
            prep.useFirstLayer = layer.useFirstLayerAsMask == true ? 1 : 0;
        }
        shaderPrepList.Add(prep);

        layer = shapeSettings.noiseLayers[2];
        prep = new NoiseSettingsShaderPrep();
        if (layer.enable)
        {

            var noiseSettings = layer.noiseSettings;
            // Asignamos valores comunes.
            prep.strength = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.strength, 0.2f, 0.8f, 0.8f, 2f) : Remap(noiseSettings.rigidnoisesettings.strength, 0.2f, 0.8f, 0.0001f, 0.08f);
            prep.numLayers = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.numLayers : noiseSettings.rigidnoisesettings.numLayers;
            prep.baseRoughnes = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.baseRoughnes, 1, 4, 0.005f, 0.02f) : Remap(noiseSettings.rigidnoisesettings.baseRoughnes, 1, 4, 0.005f, 0.01f);
            prep.roughness = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? Remap(noiseSettings.simplenoisesettings.roughness, 1, 10, 0.005f, 2f) : Remap(noiseSettings.rigidnoisesettings.roughness, 1, 10, 0.005f, 1.7f);
            prep.persistence = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.persistence : noiseSettings.rigidnoisesettings.persistence;
            prep.centerX = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.x + 10000 : (int)noiseSettings.rigidnoisesettings.center.x;
            prep.centerY = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.y : (int)noiseSettings.rigidnoisesettings.center.y;
            prep.centerZ = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.center.z : (int)noiseSettings.rigidnoisesettings.center.z;
            prep.minValue = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? noiseSettings.simplenoisesettings.minValue - 0.5f : noiseSettings.rigidnoisesettings.minValue - 0.5f;

            // Especificamos el tipo de filtro.
            prep.filterType = noiseSettings.filterType == NoiseSettings.FilterType.Simple ? 0 : 1;

            // Si el filtro es Rígido, asignamos weightMultiplier.
            if (noiseSettings.filterType == NoiseSettings.FilterType.Rigid)
            {
                prep.weightMultiplayer = noiseSettings.rigidnoisesettings.weightMultiplayer * 10;
            }
            else
            {
                // Si el filtro es Simple, podríamos decidir asignar un valor predeterminado o dejarlo como está.
                prep.weightMultiplayer = 0; // Ejemplo de valor predeterminado, ajusta según sea necesario.
            }
            prep.useFirstLayer = layer.useFirstLayerAsMask == true ? 1 : 0;
        }
        shaderPrepList.Add(prep);



        return shaderPrepList;
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}