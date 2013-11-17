using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrainWalk
{
    public class Camera
    {
  
        float aspect = 0f;
        int screenWidth;
        int screenHeight;
        Vector3 pos = new Vector3(0, 0, 0);
        Vector3 lookAt = new Vector3(0, 0, 1);
        float yaw = 0f;
        float pitch = 0f;
        const float nearPlane = 1.0f;
        const float farPlane = 100000.0f;
        float speed = 0.1f;
        float lookSpeed = 0.001f;
        float lookTolerance = 0.0001f;
        Matrix view;
        Matrix proj;
        Matrix world = Matrix.Identity;
        
        public Matrix View
        {
            get
            {
                return view;
            }
            set
            {
                view = value;
            }
        }
        public Matrix World
        {
            get
            {
                return world;
            }
            set
            {
                world = value;
            }
        }
        public Matrix Proj
        {
            get
            {
                return proj;
            }
            set
            {
                proj = value;
            }
        }
        public Vector3 Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }

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
            view = Matrix.CreateLookAt(pos, pos + lookAt, Vector3.UnitY);
            proj = Matrix.CreatePerspective(1, aspect, nearPlane, farPlane);
        }
        public bool BoxVisible(Vector3 corner1, Vector3 corner2)
        {
            Vector3[] verts = new Vector3[8];

            Matrix trans = Matrix.CreateTranslation(-pos);
            trans *= Matrix.CreateRotationY(-yaw);
            trans *= Matrix.CreateRotationX(-pitch);

            verts[0] = Vector3.Transform(corner1, trans);
            verts[1] = Vector3.Transform(new Vector3(corner1.X, corner1.Y, corner2.Z), trans);
            verts[2] = Vector3.Transform(new Vector3(corner2.X, corner1.Y, corner2.Z), trans);
            verts[3] = Vector3.Transform(new Vector3(corner2.X, corner1.Y, corner1.Z), trans);
            verts[4] = Vector3.Transform(corner2, trans);
            verts[5] = Vector3.Transform(new Vector3(corner2.X, corner2.Y, corner1.Z), trans);
            verts[6] = Vector3.Transform(new Vector3(corner1.X, corner2.Y, corner1.Z), trans);
            verts[7] = Vector3.Transform(new Vector3(corner1.X, corner2.Y, corner2.Z), trans);

            foreach (Vector3 v in verts)
            {
                if (((v.X / v.Z) < 0.5f*nearPlane) && ((v.X / v.Z) > -0.5f*nearPlane) && (v.Z < farPlane) && (v.Z > nearPlane)
                   && ((v.Y / v.Z) < (0.5f * aspect*nearPlane)) && ((v.Y / v.Z) > (-0.5f * aspect*nearPlane)))
                    return true;

            }

            return false;
        }
        public void MoveForward(float dir, float mil)
        {
            pos += lookAt * dir * mil * speed;
        }
        public void MoveForwardNoPitch(float dir, float mil)
        {
            Vector3 noPitchLook = Vector3.Transform(Vector3.UnitZ, Matrix.CreateRotationY(yaw));
            pos += noPitchLook * dir * mil * speed;
        }
        public void Strafe(float dir, float mil)
        {
            Vector3 right = Vector3.Transform(Vector3.UnitZ, Matrix.CreateRotationY(yaw + -MathHelper.PiOver2));
            pos += right * dir * mil * speed;
        }
        public void Rotate(float _pitch, float _yaw, float mil)
        {
            yaw += _yaw * mil * lookSpeed;
            pitch += _pitch * mil * lookSpeed;
            while (yaw > MathHelper.TwoPi) yaw -= MathHelper.TwoPi;
            while (yaw < -MathHelper.TwoPi) yaw += MathHelper.TwoPi;
            while (pitch > MathHelper.TwoPi) pitch -= MathHelper.TwoPi;
            while (pitch < -MathHelper.TwoPi) pitch += MathHelper.TwoPi;
            pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + lookTolerance, MathHelper.PiOver2 - lookTolerance);
        }
    }
}