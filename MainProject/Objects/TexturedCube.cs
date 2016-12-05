using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    internal class TexturedCube : GeometricPrimitive
    {
        #region Constructor 

        public TexturedCube(GraphicsDevice graphicsDevice, string name, Texture texture0 = null, Texture texture1 = null)
        {
            PreloadingConfig(name, "Cube");

            var width = Size*ScaleX;
            var height = Size*ScaleY;
            var depth = Size*ScaleZ;

            var faces = new List<int[]>
            {
                new[] {(int) width, (int) height/2, (int) depth}, // Up
                new[] {(int) width, -(int) height/2, (int) depth}, // Down
                new[] {(int) height, (int) width/2 - 1, (int) depth}, // Right
                new[] {(int) height, (int) width/2, (int) depth}, // Left
                new[] {(int) width, (int) depth/2, (int) height}, // Backward
                new[] {(int) width, (int) depth/2, (int) height} // Forward
            };

            PreInitialization(6, faces.Select(f => f[0]*f[2]).ToArray());

            const float rot90 = (float) Math.PI/2f;

            MakeVertices(0, faces[0], Vector3.Up, Matrix.Identity);
            MakeVertices(1, faces[1], Vector3.Down, Matrix.Identity);
            MakeVertices(2, faces[2], Vector3.Right, Matrix.CreateRotationZ(-rot90));
            MakeVertices(3, faces[3], Vector3.Left, Matrix.CreateRotationZ(rot90));
            MakeVertices(4, faces[4], Vector3.Backward, Matrix.CreateRotationX(rot90));
            MakeVertices(5, faces[5], Vector3.Forward, Matrix.CreateRotationX(-rot90));

            for (var i = 0; i < faces.Count; i++)
            {
                MakeIndicies(i, faces[i][0], faces[i][2]);
            }

            InitializePrimitive(graphicsDevice);

            Textures[0] = texture0;
            Textures[1] = texture1;
        }

        #endregion
    }
}
