using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.XR;

[System.Serializable]
public class BiomePlane
{
    public int[] voxelIds; // El array de int que este contenedor guarda
    public ShapeSettings shapeSettings;

    public BiomePlanePrep prepConvert(int voxelIdsStartIndex, int noiseSettingsStartIndex)
    {
        BiomePlanePrep b = new BiomePlanePrep();
        b.voxelIdsLength = voxelIds.Length;
        b.noiseSettingsLength = shapeSettings.noiseLayers.Length;
        b.voxelIdsStartIndex = voxelIdsStartIndex;
        b.noiseSettingsStartIndex = noiseSettingsStartIndex;

        return b;
    }
}

//public struct BiomePlanePrep
//{
//    public int[] voxelIds;
//    public NoiseSettingsShaderPrep[] noiseSettingsShaderPreps;
//}

public struct BiomePlanePrep
{
    // Asumiremos que manejas los conteos y posiblemente índices aquí
    public int voxelIdsStartIndex;
    public int voxelIdsLength;
    public int noiseSettingsStartIndex;
    public int noiseSettingsLength;
}