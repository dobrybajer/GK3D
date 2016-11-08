using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    public struct VertexPositionColorNormal : IVertexType
    {
        #region Private fields

        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        #endregion

        #region Constructor
        
        public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal)
        {
            Position = position;
            Color = color;
            Normal = normal;
        }

        #endregion

        #region Interface members

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float)*3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        #endregion
    }
}
