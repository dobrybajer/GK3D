using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    internal class Cube : GeometricPrimitive
    {
        #region Constructor 

        public Cube(GraphicsDevice graphicsDevice, string name, Texture texture0 = null, Texture texture1 = null)
        {
            PreloadingConfig(name, "Cube");

            Vector3[] normals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
            };

            foreach (var normal in normals)
            {
                var side1 = new Vector3(normal.Y, normal.Z, normal.X);
                var side2 = Vector3.Cross(normal, side1);

                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 1);
                AddIndex(CurrentVertex + 2);

                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 2);
                AddIndex(CurrentVertex + 3);

                AddVertex((normal - side1 - side2) * Size / 2, Color, normal);
                AddVertex((normal - side1 + side2) * Size / 2, Color, normal);
                AddVertex((normal + side1 + side2) * Size / 2, Color, normal);
                AddVertex((normal + side1 - side2) * Size / 2, Color, normal);
            }

            InitializePrimitive(graphicsDevice);

            Textures[0] = texture0;
            Textures[1] = texture1;
        }

        #endregion
    }
}
