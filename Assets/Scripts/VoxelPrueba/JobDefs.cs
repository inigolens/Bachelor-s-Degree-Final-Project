using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class JobDefs
{

    
    public struct ChunkJob : IJob
    {
        public Vector3 chunkPos;
        public NativeArray<Vector3> vertices;
        public NativeArray<int> triangles;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> vertexIndex;
        public NativeArray<int> triangleIndex;

        public void Execute()
        {
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            vertexIndex[0] = 0;
            triangleIndex[0] = 0;

            for (int x = 0; x < DataDefs.chunkSize; x++)
            {
                for (int y = 0; y < DataDefs.chunkSize; y++)
                {
                    for (int z = 0; z < DataDefs.chunkSize; z++)
                    {
                        if (IsSolid(noise, x, y, z))
                        {
                            DrawVoxel(noise, x, y, z);
                        }
                    }
                }
            }
        }
        private void DrawVoxel(FastNoiseLite noise, int x, int y, int z)
        {
            Vector3 pos = new Vector3(x, y, z);

            for (int face = 0; face < 6; face++)
            {
                if (!IsSolid(noise, DataDefs.NeighborOffset[face].x + x, DataDefs.NeighborOffset[face].y + y, DataDefs.NeighborOffset[face].z + z))
                {
                    vertices[vertexIndex[0] + 0] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 0]];
                    vertices[vertexIndex[0] + 1] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 1]];
                    vertices[vertexIndex[0] + 2] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 2]];
                    vertices[vertexIndex[0] + 3] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 3]];

                    // get the correct triangle index
                    triangles[triangleIndex[0] + 0] = vertexIndex[0] + 0;
                    triangles[triangleIndex[0] + 1] = vertexIndex[0] + 1;
                    triangles[triangleIndex[0] + 2] = vertexIndex[0] + 2;
                    triangles[triangleIndex[0] + 3] = vertexIndex[0] + 2;
                    triangles[triangleIndex[0] + 4] = vertexIndex[0] + 1;
                    triangles[triangleIndex[0] + 5] = vertexIndex[0] + 3;

                    uvs[vertexIndex[0] + 0] = new Vector2(0, 0);
                    uvs[vertexIndex[0] + 1] = new Vector2(0, 1);
                    uvs[vertexIndex[0] + 2] = new Vector2(1, 0);
                    uvs[vertexIndex[0] + 3] = new Vector2(1, 1);

                    // increment by 4 because we only added 4 vertices
                    vertexIndex[0] += 4;

                    // increment by 6 because we only added 6 ints (6 / 3 = 2 triangles)
                    triangleIndex[0] += 6;
                }
            }
        }

        private bool IsSolid(FastNoiseLite noise, int x, int y, int z)
        {
            float height = (noise.GetNoise(x + chunkPos.x, z + chunkPos.z) + 1) / 2 * DataDefs.chunkSize;

            if (y <= height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}