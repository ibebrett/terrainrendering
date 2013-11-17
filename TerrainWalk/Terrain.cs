using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrainWalk
{
    public class Terrain
    {
        public TerrainPatch[] patches;
        public VertexDeclaration dec;
        public IndexBuffer indexBuffer;
        public int numVerts = 100;
        public int numIndices = 0;
        public float[,] heightData;

        int size = 101;
        int numPatches = 100;
        const int patchSize = 11;
        const float patchScale = 22;

        public void Initialize(Texture2D heightMap, GraphicsDevice device, int depth)
        {

            // extract the height data first
            Color[] colors = new Color[size * size];
            heightMap.GetData(colors);

            // copy the height data over to the array
            heightData = new float[size, size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    heightData[x, y] = colors[x + y * size].R / 256f * 40;

            // now create the index buffer
            int[] indices = new int[(patchSize - 1) * (patchSize - 1) * 6];
            int currIndex = 0;
            for (int y = 0; y < patchSize - 1; y++)
                for (int x = 0; x < patchSize - 1; x++)
                {
                    indices[currIndex++] = x + y * patchSize;
                    indices[currIndex++] = x + (y + 1) * patchSize;
                    indices[currIndex++] = x + (y + 1) * patchSize + 1;
                    indices[currIndex++] = x + (y + 1) * patchSize + 1;
                    indices[currIndex++] = x + y * patchSize + 1;
                    indices[currIndex++] = x + y * patchSize;
                }

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
            numIndices = indices.Length;

            // now create the patches
            patches = new TerrainPatch[numPatches];
            int curr = 0;
            for (int y = 0; y < 100; y += patchSize - 1)
                for (int x = 0; x < 100; x += patchSize - 1)
                {
                    patches[curr] = new TerrainPatch();
                    patches[curr].Initialize(device, heightData, indices, x, y, patchSize, patchScale);
                    curr++;
                }
            // set the type of vertices
            dec = new VertexDeclaration(device, VertexPositionNormalTextured.VertexElements);
        }
        public float boundX(float x)
        {
            if (x >= (size - 1) * (patchScale / patchSize))
                return (patchScale / patchSize) * (size - 1) - 0.01f;
            if (x < 0.0f)
                return 0.0f;
            return x;
        }
        public float boundZ(float z)
        {
            if (z >= (size - 1) * (patchScale / patchSize))
                return (patchScale / patchSize) * (size - 1) - 0.01f;
            if (z < 0.0f)
                return 0.0f;
            return z;

        }
        public float HeightAt(float x, float z)
        {
            x = x / (patchScale/patchSize);
            z = z / (patchScale / patchSize);
            int ix = (int)x;
            int iz = (int)z;
            float inX = x - ix;
            float inZ = z - iz;
            if (ix >= size - 1 || iz >= size - 1 || ix < 0 || iz < 0)
                return 100;
            
            Vector3 a = new Vector3(ix,heightData[ix,iz],iz);
            Vector3 b;
            Vector3 c;
            // get or vectors
            if (inX / inZ > 1)
            {
                b = new Vector3(ix, heightData[ix, iz + 1], iz + 1) -a;
                c = new Vector3(ix + 1, heightData[ix + 1, iz + 1], iz + 1)-a;
            }
            else
            {
                b = new Vector3(ix + 1, heightData[ix + 1, iz + 1], iz + 1)-a;
                c = new Vector3(ix+ 1, heightData[ix+1,iz], iz) - a;
            }
            Vector3 norm = Vector3.Cross(b, c);
            float y = -(norm.X * inX + norm.Z * inZ - norm.Y *a.Y) / norm.Y;
            return y + 1.0f; // add in the 1 for "height"
          //  return heightData[ix, iz];
        }
    }

    public class TerrainPatch
    {
        public float patchSize;
        public VertexBuffer vertBuffer;
        public float minHeight, maxHeight;
        public float xCoord, zCoord;

        public void Initialize(GraphicsDevice device, float[,] heightData, int[] indices, int startX, int startY, int patchSize, float patchScale)
        {
            // set up our verts
            VertexPositionNormalTextured[] verts;
            verts = new VertexPositionNormalTextured[patchSize * patchSize];

            float inc = patchScale / patchSize;
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            xCoord = startX * inc;
            zCoord = startY * inc;

            for (int y = 0; y < patchSize; y++)
                for (int x = 0; x < patchSize; x++)
                {
                    float height = heightData[startX + x, startY + y];
                    verts[y * patchSize + x] = new VertexPositionNormalTextured();
                    verts[y * patchSize + x].Position = new Vector3(inc * (x + startX), height, inc * (y + startY));
                    verts[y * patchSize + x].Texture = new Vector2(x, y);
                    if (height > maxHeight) maxHeight = height;
                    if (height < minHeight) minHeight = height;
                }

            // add normals
            for (int i = 0; i < indices.Length; i += 3)
            {
                int index1 = indices[i];
                int index2 = indices[i + 1];
                int index3 = indices[i + 2];
                Vector3 a = verts[index1].Position - verts[index2].Position;
                Vector3 b = verts[index3].Position - verts[index2].Position;
                Vector3 norm = Vector3.Cross(a, b);
                norm.Normalize();
                verts[index1].Normal += norm;
                verts[index2].Normal += norm;
                verts[index3].Normal += norm;
            }

            for (int i = 0; i < verts.Length; i++)
                verts[i].Normal.Normalize();

            // copy everything to the buffer
            vertBuffer = new VertexBuffer(device, verts.Length * VertexPositionNormalTextured.SizeInBytes, BufferUsage.WriteOnly);
            vertBuffer.SetData(verts);
        }
    }
}
