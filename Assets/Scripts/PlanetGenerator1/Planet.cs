using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class Planet : MonoBehaviour
{

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    
    [Range(2, 256)]
    public int resolution = 256;//A mesh cant have more than 256*256 vertices
    public bool autoUpdate = true;
    public enum FaceRenderMask {All, Top, Botton, Left, Right, Front, Back};
    public FaceRenderMask faceRenderMask;


    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    public ShapeSettings[] shapeSettingsList;
    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colorSettingsFoldout;


    //private void OnValidate()
    //{
    //    GeneratePlanet();
    //}

    private void Start()
    {
        ColorSettingsGenerator col = GameObject.FindGameObjectWithTag("Manager").GetComponent<ColorSettingsGenerator>();
        colorSettings = col.CreateColorSettings();
        shapeSettings = col.CreateShapeSettings();
        shapeSettingsList = new ShapeSettings[colorSettings.biomeColourSettings.biomes.Length];
        shapeSettingsList[0] = shapeSettings;
        if(shapeSettingsList.Length > 1)
        {
            for(int i = 1; i < shapeSettingsList.Length; i++)
            {
                shapeSettingsList[i] = col.CreateShapeSettings();
            }
        }

        //Inicialize();
        //GenerateMesh();
        GeneratePlanet();
    }
    void Inicialize()
    {
        
        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colorSettings);
        if (meshFilters == null || meshFilters.Length != 6)
        {
            meshFilters = new MeshFilter[6];
        }

        terrainFaces = new TerrainFace[6];
        
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;
                meshObj.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                meshFilters[i] = meshFilter;
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colorSettings.planetMaterial;
            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
            
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    public void GeneratePlanet()
    {
        Inicialize();
        GenerateMesh();
        GenerateColor();

    }

    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            Inicialize();
            GenerateMesh();
        }
        
    }
    public void OnColorSettingsUpdated()
    {
        if (autoUpdate)
        {
            Inicialize();
            GenerateColor();
        }
    }


    void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }
        colourGenerator.UpdateElevationMinMax(shapeGenerator.elevationMinMax);
    }
    void GenerateColor()
    {
        for (int i = 0; i < 6; i++)
        {
            colourGenerator.UpdateColours();
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colourGenerator);
            }
        }
    }
}
