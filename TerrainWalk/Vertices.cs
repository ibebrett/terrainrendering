using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrainWalk
{
    public struct VertexPositionNormalColored
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public static int SizeInBytes = 7 * 4;
        public static VertexElement[] VertexElements = new VertexElement[]
     {
         new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
         new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0 ),
         new VertexElement( 0, sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 )
     };
    }
    public struct VertexPositionNormalTextured
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;
        public static int SizeInBytes = 8 * 4;
        public static VertexElement[] VertexElements = new VertexElement[]
     {
         new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
         new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
         new VertexElement( 0, sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0)
     };
    }
}
