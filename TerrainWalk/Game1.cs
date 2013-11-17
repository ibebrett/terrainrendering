using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TerrainWalk
{
    public class TerrainGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        Camera camera = new Camera();
        Terrain terrain = new Terrain();
        Effect effect;
        Ball ball = new Ball();

        bool hasFocus = true;
        int width = 800;
        int height = 600;

        Vector3 playerPos = new Vector3(10, 10, 10);

        Texture2D terrainTex;

        public void GameActivated(object sender, EventArgs e)
        {
            hasFocus = true;

        }
        public void GameDeactivated(object sender, EventArgs e)
        {
            hasFocus = false;
        }
        public TerrainGame()
        {

            Activated += new EventHandler(GameActivated);
            Deactivated += new EventHandler(GameDeactivated);

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
            base.Initialize();
        }


        protected override void LoadContent()
        {
            effect = Content.Load<Effect>("effects");
            Texture2D heightMap = Content.Load<Texture2D>("heightmap100");
            terrainTex = Content.Load<Texture2D>("tex");
            
            terrain.Initialize(heightMap, device, 1);
            camera.Initialize(width, height);
            ball.rad = 4;
            ball.Initialize(device);
        }
 

/*        public void InputXbox(float mil)
        {
            GamePadState pad = GamePad.GetState(0);
            if (pad.DPad.Up == ButtonState.Pressed)
                camera.MoveForwardNoPitch(1, mil);
            if (pad.DPad.Down == ButtonState.Pressed)
                camera.MoveForwardNoPitch(-1, mil);
            if (pad.DPad.Left == ButtonState.Pressed)
                camera.Strafe(-1, mil);
            if (pad.DPad.Right == ButtonState.Pressed)
                camera.Strafe(1, mil);
            camera.MoveForwardNoPitch(pad.ThumbSticks.Left.Y, mil);
            camera.Strafe(pad.ThumbSticks.Left.X, mil);
            float dx = pad.ThumbSticks.Right.X;
            float dy = pad.ThumbSticks.Right.Y;
            camera.Rotate(dy, -dx, mil);
        }*/
    
        public void Input( float mil)
        {

            MouseState mouse = Mouse.GetState();
            KeyboardState keys = Keyboard.GetState();
                      
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


        protected override void Update(GameTime gameTime)
        {
            float mil = gameTime.ElapsedGameTime.Milliseconds;

       //     InputXbox(mil);
            if(hasFocus)
                Input(mil);

            camera.Pos = new Vector3(terrain.boundX(camera.Pos.X),
                               0,
                               terrain.boundZ(camera.Pos.Z));
            camera.Pos = new Vector3(camera.Pos.X, 
                                     terrain.HeightAt(camera.Pos.X, camera.Pos.Z), 
                                     camera.Pos.Z);

            camera.Calculate();

            base.Update(gameTime);
        }

        void DrawTerrain()
        {
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = terrain.dec;

                device.Indices = terrain.indexBuffer;
                foreach (TerrainPatch patch in terrain.patches)
                {
                    device.Vertices[0].SetSource(patch.vertBuffer, 0, VertexPositionNormalTextured.SizeInBytes);
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrain.numVerts, 0, terrain.numIndices / 3);
                }

                pass.End();
            }
            effect.End();
        }

        void DrawBall()
        {
            effect.Begin();
            effect.Parameters["xWorld"].SetValue(Matrix.CreateTranslation(20,20,20));
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.VertexDeclaration = ball.dec;

                device.Indices = ball.indexBuffer;
          
                device.Vertices[0].SetSource(ball.vertBuffer, 0, VertexPositionNormalColored.SizeInBytes);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ball.numPoints, 0, ball.numIndices/3);
                
                pass.End();
            }
            effect.End();
        }
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
            device.RenderState.FillMode = FillMode.WireFrame;
            device.RenderState.CullMode = CullMode.None;

            effect.CurrentTechnique = effect.Techniques["Textured"];
            effect.Parameters["xTexture"].SetValue(terrainTex);
            effect.Parameters["xView"].SetValue(camera.View);
            effect.Parameters["xProjection"].SetValue(camera.Proj);
            effect.Parameters["xWorld"].SetValue(camera.World);
            
            DrawTerrain();
            effect.CurrentTechnique = effect.Techniques["Colored"];
            DrawBall();
            base.Draw(gameTime);
        }
    }
}
