using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public Vector3 chunkPosition;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    public IndexedArray<Voxel> voxelArray;
    public void Initialize(Material mat, Vector3 position)
    {
        ConfigureComponents();
        meshRenderer.sharedMaterial = mat;
        chunkPosition = position;


        voxelArray = new IndexedArray<Voxel>();
    }

    public void ClearData()
    {
        meshFilter.sharedMesh = null;
        meshCollider.sharedMesh = null;

        mesh.Clear();
        Destroy(mesh);
        mesh = null;

        voxelArray.Clear();
    }

    public void UploadMesh(MeshBuffer meshBuffer)
    {

        if (meshRenderer == null)
            ConfigureComponents();

        //Get the count of vertices/tris from the shader
        int[] faceCount = new int[2] { 0, 0 };
        meshBuffer.countBuffer.GetData(faceCount);
        MeshData meshData = WorldManager.Instance.GetMeshData();

        meshData.verts = new Vector3[faceCount[0]];
        meshData.colors = new Color[faceCount[0]];
        meshData.norms = new Vector3[faceCount[0]];
        meshData.indices = new int[faceCount[1]];
        //Get all of the meshData from the buffers to local arrays
        meshBuffer.vertexBuffer.GetData(meshData.verts, 0, 0, faceCount[0]);
        meshBuffer.indexBuffer.GetData(meshData.indices, 0, 0, faceCount[1]);
        meshBuffer.colorBuffer.GetData(meshData.colors, 0, 0, faceCount[0]);
        if (WorldManager.WorldSettings.sharedVertices)
            meshBuffer.normalBuffer.GetData(meshData.norms, 0, 0, faceCount[0]);

        //Assign the mesh
        mesh = new Mesh();
        mesh.SetVertices(meshData.verts, 0, faceCount[0]);

        if(WorldManager.WorldSettings.sharedVertices)
            mesh.SetNormals(meshData.norms, 0, faceCount[0]);

        mesh.SetIndices(meshData.indices, 0, faceCount[1], MeshTopology.Triangles, 0);
        mesh.SetColors(meshData.colors, 0, faceCount[0]);
        mesh.MarkDynamic();
        if (!WorldManager.WorldSettings.sharedVertices)
            mesh.RecalculateNormals();

        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.UploadMeshData(false);

        Mesh goodMesh = MakeReadableMeshCopy(mesh);

        meshFilter.sharedMesh = goodMesh;
        meshCollider.sharedMesh = goodMesh;
        WorldManager.Instance.ClearAndRequeueMeshData(meshData);
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
    }

    public Mesh MakeReadableMeshCopy(Mesh nonReadableMesh)
    {
        Mesh meshCopy = new Mesh();
        meshCopy.indexFormat = nonReadableMesh.indexFormat;

        // Handle vertices
        GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
        int totalSize = verticesBuffer.stride * verticesBuffer.count;
        byte[] data = new byte[totalSize];
        verticesBuffer.GetData(data);
        meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
        meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
        verticesBuffer.Release();

        // Handle triangles
        meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
        GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
        int tot = indexesBuffer.stride * indexesBuffer.count;
        byte[] indexesData = new byte[tot];
        indexesBuffer.GetData(indexesData);
        meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
        meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
        indexesBuffer.Release();

        // Restore submesh structure
        uint currentIndexOffset = 0;
        for (int i = 0; i < meshCopy.subMeshCount; i++)
        {
            uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
            meshCopy.SetSubMesh(i, new SubMeshDescriptor((int)currentIndexOffset, (int)subMeshIndexCount));
            currentIndexOffset += subMeshIndexCount;
        }

        // Recalculate normals and bounds
        meshCopy.RecalculateNormals();
        meshCopy.RecalculateBounds();

        return meshCopy;
    }


    private void ConfigureComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Dispose()
    {
        mesh.Clear();
        Destroy(mesh);
        mesh = null;
    }

    public Voxel this[Vector3 index]
    {
        get
        {
            if (WorldManager.Instance.modifiedVoxels.ContainsKey(chunkPosition))
            {
                if (WorldManager.Instance.modifiedVoxels[chunkPosition].ContainsKey(index))
                {
                    return WorldManager.Instance.modifiedVoxels[chunkPosition][index];
                }
                else return new Voxel() { ID = 0 };
            }
            else return new Voxel() { ID = 0 };
        }

        set
        {
            if (!WorldManager.Instance.modifiedVoxels.ContainsKey(chunkPosition))
                WorldManager.Instance.modifiedVoxels.TryAdd(chunkPosition, new Dictionary<Vector3, Voxel>());
            if (!WorldManager.Instance.modifiedVoxels[chunkPosition].ContainsKey(index))
                WorldManager.Instance.modifiedVoxels[chunkPosition].Add(index, value);
            else
                WorldManager.Instance.modifiedVoxels[chunkPosition][index] = value;
        }
    }

    
}

[System.Serializable]
public class MeshData
{
    public int[] indices;
    public Vector3[] verts;
    public Vector3[] norms;
    public Color[] colors;
    public Mesh mesh;

    public int arraySize;

    public void Initialize()
    {
        int maxTris = WorldManager.WorldSettings.chunkSize * WorldManager.WorldSettings.maxHeight * WorldManager.WorldSettings.chunkSize / 4;
        arraySize = maxTris * 3;
    }
    public void ClearArrays()
    {
        indices = null;
        verts = null;
        norms = null;
        colors = null;
    }


}