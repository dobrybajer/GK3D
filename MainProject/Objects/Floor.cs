using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    internal class Floor : GeometricPrimitive
    {
        #region Constructor

        public Floor(GraphicsDevice graphicsDevice, string name, Texture texture0 = null, Texture texture1 = null)
        {
            PreloadingConfig(name, "Floor");

            var normal = new Vector3(0, 1, 0);

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
            
            InitializePrimitive(graphicsDevice);

            Textures[0] = texture0;
            Textures[1] = texture1;
        }

        #endregion
    }
}
