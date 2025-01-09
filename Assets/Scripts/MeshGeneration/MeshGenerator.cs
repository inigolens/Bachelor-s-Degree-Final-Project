using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    int randomX, randomZ;
    float actualX, actualZ;
    [SerializeField] int xSize, zSize;
    [SerializeField] float zoom, intensity, montaña, xVelocity, zVelocity;
    [SerializeField] Gradient gradient;
    [SerializeField] NoiseType noiseType;
    float minTerrainHeight, maxTerrainHeight;
    Color[] colors;
    Vector2[] uvs;

    public enum NoiseType { Perlin, Cellular, Value, White, Simplex }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        randomX = Random.Range(0, 100000);
        randomZ = Random.Range(0, 100000);
        actualX = 0;
        actualZ = 0;
        CreateShape();
        UpdateMesh();
        MeasurePerformance();
    }

    void Update()
    {
        switch (noiseType)
        {
            case NoiseType.Perlin:
                RecalculateMeshPerlin();
                break;
            case NoiseType.Cellular:
                RecalculateMeshCellular();
                break;
            case NoiseType.Value:
                RecalculateMeshValue();
                break;
            case NoiseType.White:
                RecalculateMeshWhite();
                break;
            case NoiseType.Simplex:
                RecalculateMeshSimplex();
                break;
        }
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        colors = new Color[vertices.Length];
        triangles = new int[xSize * zSize * 6];

        int tris = 0, vert = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                tris += 6;
                vert++;
            }
            vert++;
        }
        uvs = new Vector2[vertices.Length];
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    void RecalculateMeshPerlin()
    {
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = intensity * PerlinNoise.Noise((x + actualX) * zoom, (z + actualZ) * zoom) - montaña * intensity;
                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }
                if (y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }

                i++;
            }
        }
        actualX += xVelocity * Time.deltaTime;
        actualZ += zVelocity * Time.deltaTime;
    }

    void RecalculateMeshCellular()
    {
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = CellularNoise((x + actualX) * zoom, (z + actualZ) * zoom) * intensity;
                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }
                if (y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }

                i++;
            }
        }
        actualX += xVelocity * Time.deltaTime;
        actualZ += zVelocity * Time.deltaTime;
    }

    void RecalculateMeshValue()
    {
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = ValueNoise((x + actualX) * zoom, (z + actualZ) * zoom) * intensity;
                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }
                if (y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }

                i++;
            }
        }
        actualX += xVelocity * Time.deltaTime;
        actualZ += zVelocity * Time.deltaTime;
    }

    void RecalculateMeshWhite()
    {
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = HashNoise(x + actualX, z + actualZ) * intensity;
                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }
                if (y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }

                i++;
            }
        }
        actualX += xVelocity * Time.deltaTime;
        actualZ += zVelocity * Time.deltaTime;
    }

    float HashNoise(float x, float z)
    {
        int seed = Mathf.FloorToInt(x * 1619 + z * 31337);
        seed = (seed << 13) ^ seed;
        return (1.0f - ((seed * (seed * seed * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
    }

    void RecalculateMeshSimplex()
    {
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = SimplexNoise.Evaluate((x + actualX) * zoom, (z + actualZ) * zoom) * intensity;
                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }
                if (y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }

                i++;
            }
        }
        actualX += xVelocity * Time.deltaTime;
        actualZ += zVelocity * Time.deltaTime;
    }

    float CellularNoise(float x, float z)
    {
        float minDist = float.MaxValue;
        int X = Mathf.FloorToInt(x);
        int Z = Mathf.FloorToInt(z);

        for (int xi = -1; xi <= 1; xi++)
        {
            for (int zi = -1; zi <= 1; zi++)
            {
                Vector2 randomPoint = RandomInsideCell(X + xi, Z + zi);
                float distX = x - randomPoint.x;
                float distZ = z - randomPoint.y;
                float dist = distX * distX + distZ * distZ;

                if (dist < minDist)
                {
                    minDist = dist;
                }
            }
        }

        return Mathf.Sqrt(minDist);
    }

    Vector2 RandomInsideCell(int x, int z)
    {
        float randomX = Hash(x, z) * 0.5f + 0.25f;
        float randomZ = Hash(x + 1, z + 1) * 0.5f + 0.25f;
        return new Vector2(x + randomX, z + randomZ);
    }

    float Hash(int x, int z)
    {
        int n = x + z * 57;
        n = (n << 13) ^ n;
        return (1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
    }

    float ValueNoise(float x, float z)
    {
        int xi = Mathf.FloorToInt(x) & 255;
        int zi = Mathf.FloorToInt(z) & 255;

        float xf = x - Mathf.Floor(x);
        float zf = z - Mathf.Floor(z);

        float topLeft = RandomValue(xi, zi);
        float topRight = RandomValue(xi + 1, zi);
        float bottomLeft = RandomValue(xi, zi + 1);
        float bottomRight = RandomValue(xi + 1, zi + 1);

        float u = Fade(xf);
        float v = Fade(zf);

        float top = Mathf.Lerp(topLeft, topRight, u);
        float bottom = Mathf.Lerp(bottomLeft, bottomRight, u);

        return Mathf.Lerp(top, bottom, v);
    }

    float RandomValue(int x, int z)
    {
        int n = x + z * 57;
        n = (n << 13) ^ n;
        return (1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
    }

    float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    float WhiteNoise(float x, float z)
    {
        return Random.value;
    }

    void MeasurePerformance()
    {
        int[] resolutions = { 10, 20, 50, 100, 256 };
        int numIterations = 500;

        foreach (int res in resolutions)
        {
            xSize = res;
            zSize = res;
            CreateShape();

            float perlinTime = MeasureMethod(() => RecalculateMeshPerlin(), numIterations);
            float cellularTime = MeasureMethod(() => RecalculateMeshCellular(), numIterations);
            float valueTime = MeasureMethod(() => RecalculateMeshValue(), numIterations);
            float whiteTime = MeasureMethod(() => RecalculateMeshWhite(), numIterations);
            float simplexTime = MeasureMethod(() => RecalculateMeshSimplex(), numIterations);

            UnityEngine.Debug.Log($"Resolution: {res}x{res}");
            UnityEngine.Debug.Log($"Perlin Noise Average Time: {perlinTime} ms");
            UnityEngine.Debug.Log($"Cellular Noise Average Time: {cellularTime} ms");
            UnityEngine.Debug.Log($"Value Noise Average Time: {valueTime} ms");
            UnityEngine.Debug.Log($"White Noise Average Time: {whiteTime} ms");
            UnityEngine.Debug.Log($"Simplex Noise Average Time: {simplexTime} ms");
        }
    }

    float MeasureMethod(System.Action method, int iterations)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < iterations; i++)
        {
            method();
        }

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds / (float)iterations;
    }

}

























public static class SimplexNoise
{
    private static int[][] grad3 = {
        new int[] {1,1,0}, new int[] {-1,1,0}, new int[] {1,-1,0}, new int[] {-1,-1,0},
        new int[] {1,0,1}, new int[] {-1,0,1}, new int[] {1,0,-1}, new int[] {-1,0,-1},
        new int[] {0,1,1}, new int[] {0,-1,1}, new int[] {0,1,-1}, new int[] {0,-1,-1}
    };

    private static int[] p = {
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,
        8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,
        35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,
        168, 68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,
        111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161,1,216,80,73,209,76,132,187,208,
        89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,
        186, 3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,
        82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,
        183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167,
        43,172,9,129,22,39,253, 19,98,108,110,79,113,224,232,178,
        185, 112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,
        179,162,241,81,51,145,235,249,14,239,107,49,192,214,31,181,
        199,106,157,184, 84,204,176,115,121,50,45,127,  4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,
        215,61,156,180
    };

    private static int[] perm;
    private static int[] permMod12;

    static SimplexNoise()
    {
        perm = new int[512];
        permMod12 = new int[512];
        for (int i = 0; i < 512; i++)
        {
            perm[i] = p[i & 255];
            permMod12[i] = perm[i] % 12;
        }
    }

    private static float dot(int[] g, float x, float y)
    {
        return g[0] * x + g[1] * y;
    }

    public static float Evaluate(float xin, float yin)
    {
        float n0, n1, n2;
        float F2 = 0.5f * (Mathf.Sqrt(3.0f) - 1.0f);
        float s = (xin + yin) * F2;
        int i = Mathf.FloorToInt(xin + s);
        int j = Mathf.FloorToInt(yin + s);
        float G2 = (3.0f - Mathf.Sqrt(3.0f)) / 6.0f;
        float t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = xin - X0;
        float y0 = yin - Y0;

        int i1, j1;
        if (x0 > y0)
        {
            i1 = 1; j1 = 0;
        }
        else
        {
            i1 = 0; j1 = 1;
        }

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1.0f + 2.0f * G2;
        float y2 = y0 - 1.0f + 2.0f * G2;

        int ii = i & 255;
        int jj = j & 255;
        int gi0 = permMod12[ii + perm[jj]];
        int gi1 = permMod12[ii + i1 + perm[jj + j1]];
        int gi2 = permMod12[ii + 1 + perm[jj + 1]];

        float t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0)
        {
            n0 = 0.0f;
        }
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * dot(grad3[gi0], x0, y0);
        }

        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0)
        {
            n1 = 0.0f;
        }
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * dot(grad3[gi1], x1, y1);
        }

        float t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0)
        {
            n2 = 0.0f;
        }
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * dot(grad3[gi2], x2, y2);
        }

        return 70.0f * (n0 + n1 + n2);
    }
}

public static class PerlinNoise
{
    private static int[] permutation = { 151,160,137,91,90,15, // Hash lookup table as defined by Ken Perlin.  This is a randomly
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142, // arranged array of all numbers from 0-255 inclusive.
        8,99,37,240,21,10,23,190, 6,148,247,120,234,75,0,26,
        197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,
        168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,
        133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161,1,216,80,73,
        209,76,132,187,208,89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,
        173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,
        206,59,227,47,16,58,17,182,189,28,42,223,
        183,170,213,119,248,152, 2,44,154,163,
        70,221,153,101,155,167,43,172,9,
        129,22,39,253, 19,98,108,110,79,113,
        224,232,178,185, 112,104,218,246,97,
        228,251,34,242,193,238,210,144,12,191,
        179,162,241,81,51,145,235,249,14,239,
        107,49,192,214,31,181,199,106,157,184,
        84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,
        128,195,78,66,215,61,156,180
    };

    private static int[] p;

    static PerlinNoise()
    {
        p = new int[512];
        for (int x = 0; x < 512; x++)
            p[x] = permutation[x % 256];
    }

    public static float Noise(float x, float y)
    {
        int X = Mathf.FloorToInt(x) & 255;
        int Y = Mathf.FloorToInt(y) & 255;

        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);

        float u = Fade(x);
        float v = Fade(y);

        int A = (p[X] + Y) & 255;
        int B = (p[X + 1] + Y) & 255;

        float res = Mathf.Lerp(
                        Mathf.Lerp(Grad(p[A], x, y), Grad(p[B], x - 1, y), u),
                        Mathf.Lerp(Grad(p[A + 1], x, y - 1), Grad(p[B + 1], x - 1, y - 1), u),
                        v);
        return (res + 1.0f) / 2.0f; // Normalize to [0, 1]
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}



