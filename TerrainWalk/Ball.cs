using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace TerrainWalk
{
    public class Ball
    {
        public VertexBuffer vertBuffer;
        public IndexBuffer indexBuffer;
        public int numIndices;
        public VertexDeclaration dec;
        public float rad = 1f;
        public int n = 4;
        public int perRow = 7;
        public int numPoints = 0;
        public void Initialize(GraphicsDevice device)
        {
            VertexPositionNormalColored[] verts = new VertexPositionNormalColored[(int)(3 * perRow * Math.Pow(2, n) - 2 * perRow + 2)];
            int numInRow = perRow;
            float heightAngStep = (float)Math.PI / (2 * n );
            float heightStep = rad * 2 / (2 * n + 1);
            int currVert = 1;
            int currHeight = 1;
            
            verts[0] = new VertexPositionNormalColored();
            verts[0].Position = new Vector3(0, rad, 0);

            for (currHeight = 1; currHeight < n; currHeight++)
            {
                float angStep = (float)(Math.PI * 2) / numInRow;
                float height = (float)(rad * (Math.Cos(heightAngStep * currHeight)));
                float wid = (float)(rad * (Math.Sin(heightAngStep * currHeight)));
              //  float height = rad - heightStep * currHeight;
             //   float wid = rad*(float)Math.Sin(Math.Acos(height / rad));
                for (int j = 0; j < numInRow; j++)
                {
                    verts[currVert] = new VertexPositionNormalColored();
                    verts[currVert].Position = new Vector3();
                    verts[currVert].Position.X = (float)(wid * Math.Cos(j * angStep));
                    verts[currVert].Position.Z = (float)(wid * Math.Sin(j * angStep));
                    verts[currVert].Position.Y = height;
                   // verts[currVert].Color = Color.Green;
                    currVert++;
                }
                numInRow = numInRow * 2;
            }

            // ok now we are at the middle strip and numPerRow already equals 2^(n+1)
            {
                float angStep = (float)(Math.PI * 2) / numInRow;
                for (int i = 0; i < numInRow; i++)
                {
                    verts[currVert] = new VertexPositionNormalColored();
                    verts[currVert].Position = new Vector3(rad * (float)Math.Cos(i * angStep), 0, rad * (float)Math.Sin(i * angStep));
                    currVert++;
                }
            }
            numInRow = numInRow / 2;
            // now do the last set of n-1 rows
            for (currHeight = n+1; currHeight < 2*n; currHeight++)
            {
                float angStep = (float)(Math.PI * 2) / numInRow;
                float height = rad * ((float)Math.Cos(heightAngStep * currHeight));
                float wid = rad * ((float)Math.Sin(heightAngStep * currHeight));
              //  float height = rad - heightStep * currHeight;
              //  float wid = rad * (float)Math.Sin(Math.Acos(height / rad));
                for (int j = 0; j < numInRow; j++)
                {
                    verts[currVert] = new VertexPositionNormalColored();
                    verts[currVert].Position = new Vector3();
                    verts[currVert].Position.X = wid * (float)Math.Cos(j * angStep);
                    verts[currVert].Position.Z = wid * (float)Math.Sin(j * angStep);
                    verts[currVert].Position.Y = height;
                    verts[currVert].Color = Color.Red;
                    verts[currVert].Normal = Vector3.Zero;
                    currVert++;
                }
                numInRow = numInRow / 2;
            }
            // now the last one
            verts[currVert] = new VertexPositionNormalColored();
            verts[currVert].Position = new Vector3(0, -rad, 0);

            dec = new VertexDeclaration(device, VertexPositionNormalColored.VertexElements);
            numPoints = verts.Length;
            vertBuffer = new VertexBuffer(device, verts.Length * VertexPositionNormalColored.SizeInBytes, BufferUsage.WriteOnly);
            vertBuffer.SetData(verts);

            // do first row of indices
            int start = 1;
            int curr = 0;
            //int[] indices = new int[3*2*perRow*(3*(int)Math.Pow(2, n)-2)];
            int[] indices = new int[2*3*perRow*(1+3*((int)Math.Pow(2,n-1)-1))];
            for (int i = 0; i < perRow; i++)
            {
                indices[curr++] = 0;
                indices[curr++] = start + i;
                indices[curr++] = start + ((i + 1) % perRow);
            }
            numInRow = perRow;

            // do rows between 1 and n-1
            int next = start + numInRow;
            for (int level = 1; level < n; level++)
            {
                for (int i = 0; i < numInRow; i++)
                {
                    // i + start is curr index, (i+1) % numInRow + start is the next in the row
                    // 2i + next is copy index, 2i+1 + next is next one, 2(i+1) % inRow*2 + next
                    indices[curr++] = i + start;
                    indices[curr++] = 2 * i + next;
                    indices[curr++] = 2 * i + 1 + next;

                    indices[curr++] = i + start;
                    indices[curr++] = 2 * i + 1 + next;
                    indices[curr++] = ((i + 1) % numInRow) + start;

                    indices[curr++] = ((i + 1) % numInRow) + start;
                    indices[curr++] = 2 * i + 1 + next;
                    indices[curr++] = ((2 * (i + 1)) % (numInRow * 2)) + next;

                }
                start += numInRow;
                numInRow *= 2;
                next = start + numInRow;
            }

            // do row n to 2n-1
            numInRow = perRow;
            start = currVert - numInRow;
            next = start - numInRow * 2;
            for (int level = 0; level < n-1; level++)
            {
                for (int i = 0; i < numInRow; i++)
                {
                    // i + start is curr index, (i+1) % numInRow + start is the next in the row
                    // 2i + next is copy index, 2i+1 + next is next one, 2(i+1) % inRow*2 + next
                    indices[curr++] = i + start;
                    indices[curr++] = 2 * i + next;
                    indices[curr++] = 2 * i + 1 + next;

                    indices[curr++] = i + start;
                    indices[curr++] = 2 * i + 2 + next;
                    indices[curr++] = ((i + 1) % numInRow) + start;

                    indices[curr++] = ((i + 1) % numInRow) + start;
                    indices[curr++] = 2 * i + 1 + next;
                    indices[curr++] = (2 * (i + 1)) % (numInRow * 2) + next;

                }
                start -= numInRow * 2;
                numInRow *= 2;
                next = start - numInRow * 2;
            }


            // do last row
            start = currVert - perRow;
            for (int i = 0; i < perRow; i++)
            {
                indices[curr++] = currVert;
                indices[curr++] = start + i;
                indices[curr++] = start + ((i + 1) % perRow);
            }

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
            numIndices = indices.Length;
        }
    }
}
