using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    internal class TexturedFloor : GeometricPrimitive
    {
        #region Constructor

        public TexturedFloor(GraphicsDevice graphicsDevice, string name, bool drawLiquid = false, Texture texture0 = null, Texture texture1 = null)
        {
            PreloadingConfig(name, "Floor");

            var width = Size * ScaleX;
            var height = Size * ScaleY;
            var depth = Size * ScaleZ;

            var faces = new List<int[]>
            {
                new[] {(int) width, (int) height/2, (int) depth}, // Up
            };

            PreInitialization(1, faces.Select(f => f[0] * f[2]).ToArray());

            MakeVertices(0, faces[0], Vector3.Up, Matrix.Identity);

            for (var i = 0; i < faces.Count; i++)
            {
                MakeIndicies(i, faces[i][0], faces[i][2]);
            }

            InitializePrimitive(graphicsDevice);

            Textures[0] = texture0;
            Textures[1] = texture1;
            DrawLiquid = drawLiquid;
        }

        #endregion
    }
}
