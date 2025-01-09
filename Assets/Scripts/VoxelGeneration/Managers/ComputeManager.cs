using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeManager : MonoBehaviour
{
    public ComputeShader noiseShader;
    public ComputeShader voxelShader;

    private List<MeshBuffer> allMeshComputeBuffers = new List<MeshBuffer>();
    private Queue<MeshBuffer> availableMeshComputeBuffers = new Queue<MeshBuffer>();

    private List<NoiseBuffer> allNoiseComputeBuffers = new List<NoiseBuffer>();
    private Queue<NoiseBuffer> availableNoiseComputeBuffers = new Queue<NoiseBuffer>();

    

    public int numMaxVoxels = 3;
    public BiomePlane[] BiomePlanes;

    ComputeBuffer noiseLayersArray;
    ComputeBuffer voxelColorsArray;

    private int xThreads;
    private int yThreads;
    public int numberMeshBuffers = 0;

    [Header("Noise Settings")]
    public int seed;
    public NoiseLayers[] noiseLayers;

    public ShapeSettings shapeSettings;
    public BiomePlane BiomePlaneNoise;

    public Shader planetMaterial;

    public UnityEngine.Color floor;

    public Material skyboxMaterial;
    public bool generateCaves;

    static float ColorfTo32(Color32 c)
    {

        uint packedColor = (uint)(
        (c.r << 24) |
        (c.g << 16) |
        (c.b << 8) |
        (c.a)
    );

        return System.BitConverter.ToSingle(System.BitConverter.GetBytes(packedColor), 0);
    }

    public void Initialize(int count = 18)
    {
        ShapeSettings[] shapesettingsList = null;
        ColorSettingsGenerator generator = new ColorSettingsGenerator();
        generator.planetMat = planetMaterial;
        ColorSettings colorSettings;
        GameObject startingdata = GameObject.Find("SolarSystem");
        if(startingdata != null)
        {
            print("Existen datos iniciales");
            
            seed = SolarSystemManager.instance.seed;
            Planet startplanet = SolarSystemManager.instance.planets[SolarSystemManager.instance.lastplanetttouched].gameObject.transform.GetComponentInChildren<Planet>();
            if(startplanet != null)
            {
                print("Planeta encontrado");
                colorSettings = startplanet.colorSettings;
                shapesettingsList = startplanet.shapeSettingsList;

            }
            else
            {
                print("No se ha encontrado planeta");
                colorSettings = generator.CreateColorSettings();
            }

            SolarSystemManager.instance.DeactiveorActiveChildren(false);
        }
        else
        {
            colorSettings = generator.CreateColorSettings();
        }


        if (skyboxMaterial != null && skyboxMaterial.shader.name == "Shader Graphs/SkyBoxPlane")
        {
            print("AAAAAAAAAAAAAAAAAAAAAAA");
            // Cambiar el color del sol
            //skyboxMaterial.SetColor("_SunColor", Color.red);

            // Cambiar el color del cielo
            skyboxMaterial.SetColor("_ZenithColor", colorSettings.oceanColor.Evaluate(0.25f));

            // Cambiar el color del horizonte
            skyboxMaterial.SetColor("_NadirColor", colorSettings.oceanColor.Evaluate(0.95f));
            skyboxMaterial.SetColor("_CloudColor", colorSettings.oceanColor.Evaluate(1f));



            // Asegúrate de actualizar el material modificado
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment(); // Actualiza la iluminación global basada en los nuevos colores del skybox
        }

        WorldManager.Instance.WorldColors = new VoxelColor[colorSettings.biomeColourSettings.biomes.Length *2 +1];
        WorldManager.Instance.WorldColors[0].color = floor;
        WorldManager.Instance.WorldColors[0].smoothness = 0;
        WorldManager.Instance.WorldColors[0].metallic = 0;

        for (int i = 0; i < colorSettings.biomeColourSettings.biomes.Length; i++)
        {
            WorldManager.Instance.WorldColors[i*2 + 1].color = colorSettings.biomeColourSettings.biomes[i].gradient.Evaluate(0.25f);
            WorldManager.Instance.WorldColors[i*2 + 1].smoothness = 0;
            WorldManager.Instance.WorldColors[i*2 + 1].metallic = 0;

            WorldManager.Instance.WorldColors[i*2+2].color = colorSettings.biomeColourSettings.biomes[i].gradient.Evaluate(0.95f);
            WorldManager.Instance.WorldColors[i*2+2].smoothness = 0;
            WorldManager.Instance.WorldColors[i*2+2].metallic = 0;
        }

        BiomePlanes = new BiomePlane[colorSettings.biomeColourSettings.biomes.Length];

        for (int bp = 0; bp < colorSettings.biomeColourSettings.biomes.Length; bp++)
        {
            
            BiomePlanes[bp] = new BiomePlane();
            if (shapesettingsList!=null)
            {
                BiomePlanes[bp].shapeSettings = shapesettingsList[bp];
            }
            else
            {
                BiomePlanes[bp].shapeSettings = generator.CreateShapeSettings();
            }
            
            BiomePlanes[bp].voxelIds = new int[2];
            BiomePlanes[bp].voxelIds[0] = bp * 2 + 1 +2;
            BiomePlanes[bp].voxelIds[1] = bp * 2 + 2;
                

        }





        xThreads = WorldManager.WorldSettings.chunkSize / 8 + 1;
        yThreads = WorldManager.WorldSettings.maxHeight / 8;

        noiseLayersArray = new ComputeBuffer(noiseLayers.Length, Marshal.SizeOf(typeof(NoiseLayers)));
        noiseLayersArray.SetData(noiseLayers);



        noiseShader.SetInt("chunkSizeX", WorldManager.WorldSettings.chunkSize);
        noiseShader.SetInt("chunkSizeY", WorldManager.WorldSettings.maxHeight);

        noiseShader.SetBool("generateCaves", generateCaves);
        noiseShader.SetBool("forceFloor", true);

        noiseShader.SetInt("maxHeight", WorldManager.WorldSettings.maxHeight);
        noiseShader.SetInt("oceanHeight", 42);
        noiseShader.SetInt("seed", seed);
        noiseShader.SetFloat("minHeight", WorldManager.WorldSettings.minHeight);

        noiseShader.SetBuffer(0, "noiseArray", noiseLayersArray);
        noiseShader.SetInt("noiseCount", noiseLayers.Length);

        ComputeBuffer noiseLayersSet = new ComputeBuffer(shapeSettings.noiseLayers.Length, Marshal.SizeOf(typeof(NoiseSettingsShaderPrep)));
        noiseLayersSet.SetData(NoiseSettingsConverter.ConvertToShaderPrepList(shapeSettings).ToArray());

        noiseShader.SetInt("numNoiseSetLayers", shapeSettings.noiseLayers.Length);
        noiseShader.SetBuffer(0, "noiseSetLayers", noiseLayersSet);


        //ComputeBuffer BiomePlaneBuffer = new ComputeBuffer(BiomePlanes.Length, Marshal.SizeOf(typeof(BiomePlanePrep)));
        //List<BiomePlanePrep> BiomePlanesPrep = new List<BiomePlanePrep>();
        //foreach(BiomePlane b in BiomePlanes)
        //{
        //    BiomePlanesPrep.Add(b.prepConvert());
        //}

        //BiomePlaneBuffer.SetData(BiomePlanesPrep.ToArray());

        List<BiomePlanePrep> BiomePlanesPrep = new List<BiomePlanePrep>();
        List<int> allVoxelIds = new List<int>();
        List<NoiseSettingsShaderPrep> allNoiseSettings = new List<NoiseSettingsShaderPrep>();

        int voxelIdIndex = 1;
        int noiseSettingsIndex = 0;
        allVoxelIds.Add(voxelIdIndex);
        
        foreach (BiomePlane b in BiomePlanes)
        {
       
            BiomePlanePrep bprep = b.prepConvert(voxelIdIndex, noiseSettingsIndex); // Asegúrate de que este método se ajuste a la nueva estructura de BiomePlanePrep

            bprep.noiseSettingsStartIndex = noiseSettingsIndex;
            foreach (NoiseSettingsShaderPrep nssp in NoiseSettingsConverter.ConvertToShaderPrepListByLayers(b.shapeSettings))
            {
                noiseSettingsIndex++;
                allNoiseSettings.Add(nssp);

                //print("Str: " + nssp.strength + " BR: " + nssp.baseRoughnes + " R: " + nssp.roughness);


            }
            bprep.noiseSettingsLength = noiseSettingsIndex - bprep.noiseSettingsStartIndex;
            BiomePlanesPrep.Add(bprep);
            allVoxelIds.AddRange(b.voxelIds);
            //print("IDS: " + b.voxelIds[0] + " " + b.voxelIds[1] + " voxelIdIndex: " + voxelIdIndex + "Lenght: " + b.voxelIds.Length);

            voxelIdIndex += b.voxelIds.Length;



        }
        BiomePlanePrep bnprep = BiomePlaneNoise.prepConvert(voxelIdIndex, noiseSettingsIndex);
        bnprep.noiseSettingsStartIndex = noiseSettingsIndex;
        foreach (NoiseSettingsShaderPrep nssp in NoiseSettingsConverter.ConvertToShaderPrepList(BiomePlaneNoise.shapeSettings))
        {
            noiseSettingsIndex++;
            allNoiseSettings.Add(nssp);

        }
        bnprep.noiseSettingsLength = noiseSettingsIndex - bnprep.noiseSettingsStartIndex;
        BiomePlanesPrep.Add(bnprep);


        noiseShader.SetInt("lastBiomePlane", BiomePlanes.Length);
        noiseShader.SetInt("numBiomes", BiomePlanes.Length);

        for (int i = 0; i < allVoxelIds.Count; i++)
        {
            //print("Numero" + i + ": Str: " + allNoiseSettings[i].strength + " Type: " + allNoiseSettings[i].filterType);
            print("Numero" + i + ": " + allVoxelIds[i]);
        }
        ComputeBuffer BiomePlaneBuffer = new ComputeBuffer(BiomePlanesPrep.Count, Marshal.SizeOf(typeof(BiomePlanePrep)));
        BiomePlaneBuffer.SetData(BiomePlanesPrep.ToArray());

        ComputeBuffer voxelIdBuffer = new ComputeBuffer(allVoxelIds.Count, sizeof(int));
        voxelIdBuffer.SetData(allVoxelIds.ToArray());

        ComputeBuffer noiseSettingsBuffer = new ComputeBuffer(allNoiseSettings.Count, Marshal.SizeOf(typeof(NoiseSettingsShaderPrep)));
        noiseSettingsBuffer.SetData(allNoiseSettings.ToArray());

        noiseShader.SetBuffer(0, "BiomePlanesBuffer", BiomePlaneBuffer);
        noiseShader.SetBuffer(0, "voxelIdsBuffer", voxelIdBuffer);
        noiseShader.SetBuffer(0, "noiseSettingsBuffer", noiseSettingsBuffer);

        VoxelColor32[] converted = new VoxelColor32[WorldManager.Instance.WorldColors.Length];
        int cCount = 0;

        foreach (VoxelColor c in WorldManager.Instance.WorldColors)
        {
            VoxelColor32 bcol = new VoxelColor32();
            bcol.color = ColorfTo32(c.color);
            bcol.smoothness = c.smoothness;
            bcol.metallic = c.metallic;
            //print("VC"+cCount+": " + bcol.color);
            converted[cCount++] = bcol;
            
        }


        voxelColorsArray = new ComputeBuffer(converted.Length, Marshal.SizeOf(typeof(VoxelColor32)));
        voxelColorsArray.SetData(converted);

        voxelShader.SetBuffer(0, "voxelColors", voxelColorsArray);
        voxelShader.SetInt("chunkSizeX", WorldManager.WorldSettings.chunkSize);
        voxelShader.SetInt("chunkSizeY", WorldManager.WorldSettings.maxHeight);
        voxelShader.SetBool("sharedVertices", WorldManager.WorldSettings.sharedVertices);
        voxelShader.SetBool("useTextures", WorldManager.WorldSettings.useTextures);
        for (int i = 0; i < count; i++)
        {
            CreateNewNoiseBuffer();
            CreateNewMeshBuffer();
        }
    }

    public void GenerateVoxelData(Chunk cont, Vector3 pos)
    {

        NoiseBuffer noiseBuffer = GetNoiseBuffer();
        noiseBuffer.countBuffer.SetCounterValue(0);
        noiseBuffer.countBuffer.SetData(new uint[] { 0 });
        noiseShader.SetBuffer(0, "voxelArray", noiseBuffer.noiseBuffer);
        noiseShader.SetBuffer(0, "count", noiseBuffer.countBuffer);

        noiseShader.SetVector("chunkPosition", cont.chunkPosition);
        noiseShader.SetVector("seedOffset", Vector3.zero);

        noiseShader.Dispatch(0, xThreads, yThreads, xThreads);

        MeshBuffer meshBuffer = GetMeshBuffer();
        meshBuffer.countBuffer.SetCounterValue(0);
        meshBuffer.countBuffer.SetData(new uint[] { 0, 0 });

        voxelShader.SetVector("chunkPosition", cont.chunkPosition);

        voxelShader.SetBuffer(0, "voxelArray", noiseBuffer.noiseBuffer);
        voxelShader.SetBuffer(0, "counter", meshBuffer.countBuffer);
        voxelShader.SetBuffer(0, "vertexBuffer", meshBuffer.vertexBuffer);
        voxelShader.SetBuffer(0, "normalBuffer", meshBuffer.normalBuffer);
        voxelShader.SetBuffer(0, "colorBuffer", meshBuffer.colorBuffer);
        voxelShader.SetBuffer(0, "indexBuffer", meshBuffer.indexBuffer);
        voxelShader.Dispatch(0, xThreads, yThreads, xThreads);

        AsyncGPUReadback.Request(meshBuffer.countBuffer, (callback) =>
        {
            if (WorldManager.Instance.activeChunks.ContainsKey(pos))
            {
                WorldManager.Instance.activeChunks[pos].UploadMesh(meshBuffer);

                Voxel[] voxels = new Voxel[WorldManager.WorldSettings.ChunkCount];
                noiseBuffer.noiseBuffer.GetData(voxels);
                cont.voxelArray.array = voxels; // Asignamos los datos al IndexedArray<Voxel>
                
            }
            ClearAndRequeueBuffer(noiseBuffer);
            ClearAndRequeueBuffer(meshBuffer);

        });
    }

    private void ClearVoxelData(NoiseBuffer buffer)
    {
        buffer.countBuffer.SetData(new int[] { 0 });
        noiseShader.SetBuffer(1, "voxelArray", buffer.noiseBuffer);
        noiseShader.Dispatch(1, xThreads, yThreads, xThreads);
    }

    #region MeshBuffer Pooling
    public MeshBuffer GetMeshBuffer()
    {
        if (availableMeshComputeBuffers.Count > 0)
        {
            return availableMeshComputeBuffers.Dequeue();
        }
        else
        {
            Debug.Log("Generate chunk");
            return CreateNewMeshBuffer(false);
        }
    }

    public MeshBuffer CreateNewMeshBuffer(bool enqueue = true)
    {
        MeshBuffer buffer = new MeshBuffer();
        buffer.InitializeBuffer();

        allMeshComputeBuffers.Add(buffer);

        if (enqueue)
            availableMeshComputeBuffers.Enqueue(buffer);

        numberMeshBuffers++;

        return buffer;
    }

    public void ClearAndRequeueBuffer(MeshBuffer buffer)
    {
        availableMeshComputeBuffers.Enqueue(buffer);
    }
    #endregion

    #region NoiseBuffer Pooling
    public NoiseBuffer GetNoiseBuffer()
    {
        if (availableNoiseComputeBuffers.Count > 0)
        {
            return availableNoiseComputeBuffers.Dequeue();
        }
        else
        {
            return CreateNewNoiseBuffer(false);
        }
    }

    public NoiseBuffer CreateNewNoiseBuffer(bool enqueue = true)
    {
        NoiseBuffer buffer = new NoiseBuffer();
        buffer.InitializeBuffer();
        allNoiseComputeBuffers.Add(buffer);

        if (enqueue)
            availableNoiseComputeBuffers.Enqueue(buffer);

        return buffer;
    }

    public void ClearAndRequeueBuffer(NoiseBuffer buffer)
    {
        ClearVoxelData(buffer);
        availableNoiseComputeBuffers.Enqueue(buffer);
    }
    #endregion

    private void OnApplicationQuit()
    {
        DisposeAllBuffers();
    }

    public void DisposeAllBuffers()
    {
        noiseLayersArray?.Dispose();
        voxelColorsArray?.Dispose();
        foreach (NoiseBuffer buffer in allNoiseComputeBuffers)
            buffer.Dispose();
        foreach (MeshBuffer buffer in allMeshComputeBuffers)
            buffer.Dispose();
    }


    private static ComputeManager _instance;

    public static ComputeManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<ComputeManager>();
            return _instance;
        }
    }
}

public struct NoiseBuffer
{
    public ComputeBuffer noiseBuffer;
    public ComputeBuffer countBuffer;
    public bool Initialized;
    public bool Cleared;

    public void InitializeBuffer()
    {
        countBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Counter);
        countBuffer.SetCounterValue(0);
        countBuffer.SetData(new uint[] { 0 });

        //voxelArray = new IndexedArray<Voxel>();
        noiseBuffer = new ComputeBuffer(WorldManager.WorldSettings.ChunkCount, 4);
        Initialized = true;
    }

    public void Dispose()
    {
        countBuffer?.Dispose();
        noiseBuffer?.Dispose();

        Initialized = false;
    }

}

public struct MeshBuffer
{
    public ComputeBuffer vertexBuffer;
    public ComputeBuffer normalBuffer;
    public ComputeBuffer colorBuffer;
    public ComputeBuffer indexBuffer;
    public ComputeBuffer countBuffer;

    public bool Initialized;
    public bool Cleared;
    public IndexedArray<Voxel> voxelArray;

    public void InitializeBuffer()
    {
        if (Initialized)
            return;

        countBuffer = new ComputeBuffer(2, 4, ComputeBufferType.Counter);
        countBuffer.SetCounterValue(0);
        countBuffer.SetData(new uint[] { 0, 0 });

        int maxTris = WorldManager.WorldSettings.chunkSize * WorldManager.WorldSettings.maxHeight * WorldManager.WorldSettings.chunkSize / 6;
        //width*height*width*faces*tris
        int maxVertices = WorldManager.WorldSettings.sharedVertices ? maxTris / 3 : maxTris;
        int maxNormals = WorldManager.WorldSettings.sharedVertices ? maxVertices * 3 : 1;
        vertexBuffer ??= new ComputeBuffer(maxVertices * 3, 12);
        colorBuffer ??= new ComputeBuffer(maxVertices * 3, 16);
        normalBuffer ??= new ComputeBuffer(maxNormals, 12);
        indexBuffer ??= new ComputeBuffer(maxTris * 3, 4);

        Initialized = true;
    }

    public void Dispose()
    {
        vertexBuffer?.Dispose();
        normalBuffer?.Dispose();
        colorBuffer?.Dispose();
        indexBuffer?.Dispose();
        countBuffer?.Dispose();

        Initialized = false;

    }
}

