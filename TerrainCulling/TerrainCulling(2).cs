using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace TerrrainCulling
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
        public static int SizeInBytes = 8 * 4 ;
        public static VertexElement[] VertexElements = new VertexElement[]
     {
         new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
         new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
         new VertexElement( 0, sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0)
     };
    }
    public class Terrain
    {
        public TerrainPatch[] patches;
        public VertexDeclaration dec;
        public IndexBuffer indexBuffer;
        public int numVerts = 100;
        public int numIndices = 0;
        public float[,] heightData;

        public int size = 101;
        public int numPatches = 100;
        public int patchSize = 11;
        public float patchScale = 22;


        public Texture2D tex;

        public void Initialize(Texture2D heightMap, GraphicsDevice device)
        {
            // extract the height data first
            Color[] colors = new Color[size*size];
            heightMap.GetData(colors);

            // copy the height data over to the array
            heightData = new float[size,size];
            for(int y = 0; y < size; y++)
                for(int x = 0; x < size; x++)
                    heightData[x, y] = colors[x + y * size].R/256f * 40;

            // now create the index buffer
            int[] indices = new int[(patchSize-1) * (patchSize-1) * 6];
            int currIndex = 0;
            for (int y = 0; y < patchSize - 1; y++)
                for (int x = 0; x < patchSize - 1; x++)
                {
                    indices[currIndex++] = x + y * patchSize;
                    indices[currIndex++] = x + (y+1) * patchSize ;
                    indices[currIndex++] = x + (y+1) * patchSize + 1;
                    indices[currIndex++] = x + (y+1) * patchSize + 1;
                    indices[currIndex++] = x + y * patchSize + 1;
                    indices[currIndex++] = x + y * patchSize;
                }

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
            numIndices = indices.Length;

            // now create the patches
            patches = new TerrainPatch[numPatches];
            int curr = 0;
            for(int y = 0; y < 100; y += patchSize-1)
                for (int x = 0; x < 100; x += patchSize-1)
                {
                    patches[curr] = new TerrainPatch();
                    patches[curr].Initialize(device, heightData, indices, x, y, patchSize, patchScale);
                    curr++;
                }
            // set the type of vertices
            dec = new VertexDeclaration(device, VertexPositionNormalTextured.VertexElements);
        }
    }
    public class TerrainPatch
    {
        public float patchSize;
        public VertexBuffer vertBuffer;
        public float minHeight, maxHeight;
        public float xCoord, zCoord;      

        public void Initialize(GraphicsDevice device, float[,]  heightData, int[] indices, int startX, int startY, int patchSize, float patchScale)
        {
            // set up our verts
            VertexPositionNormalTextured[] verts;
            verts = new VertexPositionNormalTextured[patchSize * patchSize];
            
            float inc =  patchScale/patchSize;
            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            xCoord = startX*inc;
            zCoord = startY*inc;
            
            for (int y = 0; y < patchSize; y++)
                for (int x = 0; x < patchSize; x++)
                {
                    float height = heightData[startX + x, startY + y];
                    verts[y * patchSize + x] = new VertexPositionNormalTextured();
                    verts[y * patchSize + x].Position = new Vector3(inc * (x + startX), height, inc * (y+startY));
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

    public class Camera
    {
        int screenWidth;
        int screenHeight;

        public Vector3 pos = new Vector3(0, 0, 0);
        public Vector3 lookAt = new Vector3(0, 0, 1);
        public float yaw = 0f;
        public float pitch = 0f;
        float nearPlane = 1.0f;
        public float farPlane = 100000.0f;
        public float aspect = 0f;
        float speed = 0.1f;
        float lookSpeed = 0.005f;
        float lookTolerance = 0.0001f;
        public Matrix view;
        public Matrix proj;
        public Matrix world = Matrix.Identity;

        // calculate matrices we need so that 
        public void Initialize(int _screenWidth, int _screenHeight)
        {
            screenWidth = _screenWidth;
            screenHeight = _screenHeight;

            aspect = (float)screenHeight / (float)screenWidth;
        }
        public void Calculate()
        {
            lookAt = Vector3.Transform(Vector3.UnitZ, Matrix.CreateRotationX(pitch));
            lookAt = Vector3.Transform(lookAt, Matrix.CreateRotationY(yaw));
            lookAt.Normalize();
            view = Matrix.CreateLookAt(pos, pos+lookAt, Vector3.UnitY);            
            proj = Matrix.CreatePerspective(1, aspect, nearPlane, farPlane);
        }

        public void MoveForward(float dir, float mil)
        {
            pos += lookAt * dir * mil * speed;
        }
        public void Strafe(float dir, float mil)
        {
            Vector3 right = Vector3.Transform(Vector3.UnitZ, Matrix.CreateRotationY(yaw+-MathHelper.PiOver2));
            pos += right * dir * mil * speed;
        }
        public void Rotate(float _pitch, float _yaw, float mil)
        {
            yaw += _yaw*mil*lookSpeed;
            pitch += _pitch*mil*lookSpeed;
            while (yaw > MathHelper.TwoPi) yaw -= MathHelper.TwoPi;
            while (yaw < -MathHelper.TwoPi) yaw += MathHelper.TwoPi;
            while (pitch > MathHelper.TwoPi) pitch -= MathHelper.TwoPi;
            while (pitch < -MathHelper.TwoPi) pitch += MathHelper.TwoPi;
            pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + lookTolerance, MathHelper.PiOver2-lookTolerance);
        }
    }

    public class TerrainCulling : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        Effect effect;

        Terrain terrain = new Terrain();
        Camera camera = new Camera();
        Camera funCamera = new Camera();
        public VertexDeclaration cdec;   // bad, just to finish things off

        bool drawBox = true;
        bool lighting = true;
        bool textured = true;
        int whichCamera = 0;
        int width = 800;
        int height = 600;

        void drawCameraFustrum(Camera c, GraphicsDevice d)
        {
            //Matrix inv = Matrix.Invert(c.view);
            Matrix inv = Matrix.CreateRotationX(c.pitch);
            inv = inv * Matrix.CreateRotationY(c.yaw);
            inv = inv * Matrix.CreateTranslation(c.pos);
            VertexPositionNormalColored[] verts = new VertexPositionNormalColored[8];
            verts[0].Position = Vector3.Transform(new Vector3(-.5f, .5f * c.aspect, 1.0f), inv);
            verts[1].Position = Vector3.Transform(new Vector3(.5f, .5f* c.aspect, 1.0f), inv);
            verts[2].Position = Vector3.Transform(new Vector3(-.5f, -.5f *  c.aspect, 1.0f), inv);
            verts[3].Position = Vector3.Transform(new Vector3(.5f, -.5f  * c.aspect, 1.0f), inv);
            verts[4].Position = Vector3.Transform(new Vector3(-.5f * c.farPlane, .5f * c.aspect * c.farPlane, c.farPlane), inv);
            verts[5].Position = Vector3.Transform(new Vector3(.5f * c.farPlane, .5f * c.aspect * c.farPlane, c.farPlane), inv);
            verts[6].Position = Vector3.Transform(new Vector3(-.5f * c.farPlane, -.5f * c.aspect * c.farPlane, c.farPlane), inv);
            verts[7].Position = Vector3.Transform(new Vector3(.5f * c.farPlane, -.5f * c.aspect * c.farPlane, c.farPlane), inv);
            for (int i = 0; i < 8; i++) verts[i].Color = Color.Black;
            int[] indices = new int[24];
            indices[0] = 0;
            indices[1] = 4;
            indices[2] = 6;

            indices[3] = 4;
            indices[4] = 6;
            indices[5] = 2;

            indices[6] = 0;
            indices[7] = 4;
            indices[8] = 5;

            indices[9] = 1;
            indices[10] = 5;
            indices[11] = 4;

            indices[12] = 5;
            indices[13] = 7;
            indices[14] = 1;

            indices[15] = 5;
            indices[16] = 7;
            indices[17] = 3;

            indices[18] = 2;
            indices[19] = 6;
            indices[20] = 3;
            
            indices[21] = 2;
            indices[22] = 7;
            indices[23] = 3;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = terrain.dec;

                foreach (TerrainPatch patch in terrain.patches)
                {
                    
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, 8, indices, 0, 8);
                }
                pass.End();
            }
            effect.End();
        }

        public TerrainCulling()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
      
        protected override void Initialize()
        {
            Window.Title = "Terrain Culling - Brett Jurman";
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            width = graphics.GraphicsDevice.Viewport.Width;
            height = graphics.GraphicsDevice.Viewport.Height;
            device = graphics.GraphicsDevice;
            cdec = new VertexDeclaration(device, VertexPositionNormalColored.VertexElements);
            base.Initialize();
        }

         protected override void LoadContent()
        {
            effect = Content.Load<Effect>("effects");
            Texture2D heightMap = Content.Load<Texture2D>("heightmap100");
            terrain.tex = Content.Load<Texture2D>("tex");
            terrain.Initialize(heightMap, device);
            camera.Initialize(width, height);
            funCamera.Initialize(width, height);
            funCamera.Calculate();
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            float mil = gameTime.ElapsedGameTime.Milliseconds;
            //MouseState mouse = Mouse.GetState();
            //GamePadState pad = GamePad.GetState(PlayerIndex.One);

            MouseState mouse = Mouse.GetState();
            KeyboardState keys = Keyboard.GetState();

            if (whichCamera == 0)
            {                
                if (keys.IsKeyDown(Keys.W))
                    camera.MoveForward(1, mil);
                if (keys.IsKeyDown(Keys.S))
                    camera.MoveForward(-1, mil);
                if (keys.IsKeyDown(Keys.A))
                    camera.Strafe(-1, mil);
                if (keys.IsKeyDown(Keys.D))
                    camera.Strafe(1, mil);

                int wo2 = width / 2;
                int ho2 = height / 2;
                float dx = mouse.X - wo2;
                float dy = mouse.Y - ho2;
                Mouse.SetPosition(wo2, ho2);
                camera.Rotate(dy, -dx, mil);
            }

            if (whichCamera == 1)
            {
                if (keys.IsKeyDown(Keys.W))
                    funCamera.MoveForward(1, mil);
                if (keys.IsKeyDown(Keys.S))
                    funCamera.MoveForward(-1, mil);
                if (keys.IsKeyDown(Keys.A))
                    funCamera.Strafe(-1, mil);
                if (keys.IsKeyDown(Keys.D))
                    funCamera.Strafe(1, mil);

                int wo2 = width / 2;
                int ho2 = height / 2;
                float dx = mouse.X - wo2;
                float dy = mouse.Y - ho2;
                Mouse.SetPosition(wo2, ho2);
                funCamera.Rotate(dy, -dx, mil);
            }

            if(keys.IsKeyDown(Keys.Y))
                drawBox = !drawBox;
            if (keys.IsKeyDown(Keys.U))
                whichCamera = (whichCamera+1) % 2;
            if (keys.IsKeyDown(Keys.I))
                lighting = !lighting;
            if (keys.IsKeyDown(Keys.O))
                textured = !textured;
            
            /*
            if (pad.IsButtonDown(Buttons.B))
                drawBox = !drawBox;
            if (pad.IsButtonDown(Buttons.A))
                whichCamera = (whichCamera+1) % 2;
            if (pad.IsButtonDown(Buttons.Y))
                lighting = !lighting;
            if (pad.IsButtonDown(Buttons.X))
                textured = !textured;
           */
            camera.Calculate();
            funCamera.Calculate();
            base.Update(gameTime);
        }

        void DrawBox(GraphicsDevice device, float bigY, float smallY, float x, float z, float size, Color c)
        {
            VertexPositionNormalColored[] verts = new VertexPositionNormalColored[8];
            for (int i = 0; i < 8; i++)
            {
                verts[i] = new VertexPositionNormalColored();
                verts[i].Color = c;
                verts[i].Normal = Vector3.Zero;
            }
            verts[0].Position = new Vector3(x, smallY, z);
            verts[1].Position = new Vector3(x+size, smallY, z);
            verts[2].Position = new Vector3(x+size, smallY, z+size);
            verts[3].Position = new Vector3(x, smallY, z+size);
            verts[4].Position = new Vector3(x, bigY, z);
            verts[5].Position = new Vector3(x + size, bigY, z);
            verts[6].Position = new Vector3(x + size, bigY, z + size);
            verts[7].Position = new Vector3(x, bigY, z + size);
            int[] indices = new int[18];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            
            indices[3] = 1;
            indices[4] = 2;
            indices[5] = 3;

            indices[6] = 0;
            indices[7] = 4;
            indices[8] = 1;
            
            indices[9] = 4;
            indices[10] = 5;
            indices[11] = 1;

            indices[12] = 4;
            indices[13] = 5;
            indices[14] = 6;

            indices[15] = 5;
            indices[16] = 6;
            indices[17] = 7;
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts,0, 8, indices, 0, 6);
        }

        bool BoxVisible(Camera c, float bigY, float smallY, float x, float z, float size)
        {
            Vector3[] verts = new Vector3[8];
            Matrix trans = Matrix.CreateTranslation(-c.pos);
            trans *= Matrix.CreateRotationY(-c.yaw);
            trans *= Matrix.CreateRotationX(-c.pitch);
            
            
            
            verts[0] = Vector3.Transform(new Vector3(x, smallY, z), trans);
            verts[1] = Vector3.Transform(new Vector3(x+size, smallY, z), trans);
            verts[2] = Vector3.Transform(new Vector3(x + size, smallY, z + size), trans);
            verts[3] = Vector3.Transform(new Vector3(x, smallY, z + size), trans);
            verts[4] = Vector3.Transform(new Vector3(x, bigY, z), trans);
            verts[5] = Vector3.Transform(new Vector3(x + size, bigY, z), trans);
            verts[6] = Vector3.Transform(new Vector3(x + size, bigY, z + size), trans);
            verts[7] = Vector3.Transform(new Vector3(x, bigY, z + size), trans);
            
            foreach (Vector3 v in verts)
            {
                if (((v.X / v.Z) < 0.5f) && ((v.X / v.Z) > -0.5f) && (v.Z < c.farPlane) && (v.Z > 1f)
                   && ((v.Y / v.Z) < ( 0.5*c.aspect)) && ((v.Y / v.Z) > ( -0.5*c.aspect)))
                    return true;
                
            }

            return false;
        }

        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            device.RenderState.FillMode = FillMode.Solid;
            
            device.RenderState.CullMode = CullMode.None;
            
            Vector3 lightDir = new Vector3(-1,1,-1);
            lightDir.Normalize();
            if (textured)
                device.RenderState.FillMode = FillMode.Solid;
            else
                device.RenderState.FillMode = FillMode.WireFrame;
            effect.CurrentTechnique = effect.Techniques["Textured"];

            effect.Parameters["xTexture"].SetValue(terrain.tex);
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xLightDirection"].SetValue(lightDir);
            effect.Parameters["xAmbient"].SetValue(0.1f);
            effect.Parameters["xEnableLighting"].SetValue(lighting);
            if (whichCamera == 0)
            {
                effect.Parameters["xView"].SetValue(camera.view);
                effect.Parameters["xProjection"].SetValue(camera.proj);
                effect.Parameters["xWorld"].SetValue(camera.world);
            }
            else
            {
                effect.Parameters["xView"].SetValue(funCamera.view);
                effect.Parameters["xProjection"].SetValue(funCamera.proj);
                effect.Parameters["xWorld"].SetValue(funCamera.world);
            }

            
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = terrain.dec;
               
                device.Indices = terrain.indexBuffer;
                foreach(TerrainPatch patch in terrain.patches)
                {
                    device.Vertices[0].SetSource(patch.vertBuffer, 0, VertexPositionNormalTextured.SizeInBytes);
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrain.numVerts, 0, terrain.numIndices/3);
                }

                pass.End();
            }
            effect.End();

            effect.CurrentTechnique = effect.Techniques["Colored"];
            effect.Parameters["xEnableLighting"].SetValue(false);
            device.RenderState.FillMode = FillMode.WireFrame;
            if (drawBox)
            {
                effect.Begin();
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.VertexDeclaration = cdec;

                    foreach (TerrainPatch patch in terrain.patches)
                    {
                        if (BoxVisible(funCamera, patch.maxHeight, patch.minHeight, patch.xCoord, patch.zCoord, terrain.patchScale))
                            DrawBox(device, patch.maxHeight, patch.minHeight, patch.xCoord, patch.zCoord, terrain.patchScale, Color.CornflowerBlue);
                        else 
                            DrawBox(device, patch.maxHeight, patch.minHeight, patch.xCoord, patch.zCoord, terrain.patchScale, Color.Red);
                     }

                    pass.End();
                }
                effect.End();
            }

            drawCameraFustrum(funCamera,device);

            base.Draw(gameTime);
        }
    }
}
