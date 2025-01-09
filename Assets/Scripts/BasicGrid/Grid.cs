using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GameObject[] treeprefabs;
    public Material terrainMaterial;
    public Material edgeMaterial;
    public int size = 100;
    public float scale = 0.1f;
    public float watermax = 0.4f;
    public float lengthwalls = 1f;
    public float treedensity = 0.5f, treenoiseScale = 0.05f;
    public float colorNoiseScale = 0.3f;
    public Color aColor, bColor, cColor;
    Cell[,] cell;
    
    // Start is called before the first frame update
    void Start()
    {
        float[,] noisemap = new float[size,size];
        float randomOffX = Random.Range(-10000f,10000f);
        float randomOffY = Random.Range(-10000f, 10000f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                noisemap[x, y] = Mathf.PerlinNoise(x*scale + randomOffX, y*scale + randomOffY);
            }
        }

        float[,] falloffmap = new float[size,size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float xv = x / (float)size * 2 - 1;
                float yv = y / (float)size * 2 - 1;
                //Por si el mapa no es cuadrado-
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                //Grafico que suabiza el valor entre 0 y 1
                falloffmap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }

        cell = new Cell[size, size];
        for(int x = 0; x < size; x++)
        {
            for(int y = 0; y < size; y++)
            {
                float noiseValue = noisemap[x, y];
                noiseValue -= falloffmap[x, y];
                Cell ncell = new Cell();
                ncell.iswater = noiseValue < watermax;
                cell[x, y] = ncell;
            }
        }
        DrawTerrainMesh(cell);
        DrawEdgeMesh(cell);
        DrawTexture(cell);
        generateTrees(cell);
    }

    void DrawTerrainMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3> ();
        List<int> triangles = new List<int> ();
        List<Vector2> uvs = new List<Vector2>();
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Cell cell = grid[x, y];
                if (!cell.iswater) {
                    Vector3 a = new Vector3(x - 0.5f, 0, y + 0.5f);
                    Vector3 b = new Vector3(x + 0.5f, 0, y + 0.5f);
                    Vector3 c = new Vector3(x - 0.5f, 0, y - 0.5f);
                    Vector3 d = new Vector3(x + 0.5f, 0, y - 0.5f);
                    Vector3[] v = new Vector3[] {a, b, c, b, d, c};
                    Vector2 uvA = new Vector2(x / (float)size, y / (float)size);
                    Vector2 uvB = new Vector2((x + 1) / (float)size, y / (float)size);
                    Vector2 uvC = new Vector2(x / (float)size, (y + 1) / (float)size);
                    Vector2 uvD = new Vector2((x + 1) / (float)size, (y + 1) / (float)size);
                    Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
                    for (int k = 0; k < v.Length; k++)
                    {
                        vertices.Add (v[k]);
                        triangles.Add (triangles.Count);
                        uvs.Add(uv[k]);
                    }
                
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter> ();
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer> ();
    }

    void DrawTexture(Cell[,] grid)
    {
        float[,] noisemap = new float[size, size];
        float randomOffX = Random.Range(-10000f, 10000f);
        float randomOffY = Random.Range(-10000f, 10000f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                noisemap[x, y] = Mathf.PerlinNoise(x * colorNoiseScale + randomOffX, y * colorNoiseScale + randomOffY);
            }
        }
        Texture2D texture = new Texture2D(size, size);
        Color[] colorMap = new Color[size * size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Cell cell = grid[x, y];
                if (cell.iswater)
                {
                    colorMap[y * size + x] = Color.blue;
                }
                else
                {
                    if (noisemap[x,y] < 0.33)
                    {
                        colorMap[(y * size) + x] = aColor;
                    }else if (noisemap[x, y] < 0.66)
                    {
                        colorMap[(y * size) + x] = bColor;
                    }
                    else
                    {
                        colorMap[(y * size) + x] = cColor;
                    }
                    
                }
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();

        MeshRenderer meshrenderer = gameObject.GetComponent<MeshRenderer>();
        meshrenderer.material = terrainMaterial;
        meshrenderer.material.mainTexture = texture;

    }
    void DrawEdgeMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.iswater)
                {
                    if (x > 0)
                    {
                        Cell left = grid[x - 1, y];
                        if (left.iswater)
                        {
                            Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -lengthwalls, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -lengthwalls, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (x < size - 1)
                    {
                        Cell right = grid[x + 1, y];
                        if (right.iswater)
                        {
                            Vector3 a = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -lengthwalls, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -lengthwalls, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y > 0)
                    {
                        Cell down = grid[x, y - 1];
                        if (down.iswater)
                        {
                            Vector3 a = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -lengthwalls, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -lengthwalls, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y < size - 1)
                    {
                        Cell up = grid[x, y + 1];
                        if (up.iswater)
                        {
                            Vector3 a = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -lengthwalls, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -lengthwalls, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(transform);

        MeshFilter meshFilter = edgeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = edgeMaterial;
    }
    private void generateTrees(Cell[,] grid)
    {
        float[,] noisemap = new float[size, size];
        float randomOffX = Random.Range(-10000f, 10000f);
        float randomOffY = Random.Range(-10000f, 10000f);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                noisemap[x, y] = Mathf.PerlinNoise(x * treenoiseScale + randomOffX, y * treenoiseScale + randomOffY);
            }
        }
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Cell cell = grid[x, y];
                if (!cell.iswater)
                {
                    float v = Random.Range(0f, treedensity);
                    if (noisemap[x, y]< v)
                    {
                        GameObject prefab = treeprefabs[Random.Range(0, treeprefabs.Length)];
                        GameObject tree = Instantiate(prefab, transform);
                        tree.transform.position = new Vector3(x, 0, y);
                        tree.transform.rotation = Quaternion.Euler(-90, Random.Range(0, 360f), 0);
                        //tree.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
                    }
                }
            }
        }

    }







    private void ondrawgizmos()
    {
        if (!Application.isPlaying) return;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Cell ncell = cell[x, y];
                if (ncell.iswater)
                {
                    Gizmos.color = Color.blue;
                }
                else
                {
                    Gizmos.color = Color.green;
                }
                Vector3 pos = new Vector3(x, 0, y);
                Gizmos.DrawCube(pos, Vector3.one);
            }
        }
    }
}
