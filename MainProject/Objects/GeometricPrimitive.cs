using System;
using System.Collections.Generic;
using System.Linq;
using MainProject.Config;
using MainProject.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    internal abstract class GeometricPrimitive : IDisposable
    {
        #region Consts

        private const string DefaultTechnique = "BasicPhongLightning";

        #endregion

        #region Private fields

        private Matrix _scaleMatrix;
        private Matrix _translationMatrix;
        private Matrix _rotationMatrix;
        
        private readonly List<VertexPositionColorNormal> _vertices = new List<VertexPositionColorNormal>();
        private readonly List<ushort> _indices = new List<ushort>();
        private readonly List<VertexPositionNormalTexture[]> _textureVertices = new List<VertexPositionNormalTexture[]>();
        private readonly List<List<int>> _textureIndices = new List<List<int>>();

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        #endregion

        #region Protected fields

        protected float TextureResolution;
        protected float Size;
        protected float ScaleX;
        protected float ScaleY;
        protected float ScaleZ;
        protected Color Color;
        protected Texture[] Textures;
        
        #endregion

        #region Preparing operation matrices

        protected void PreloadingConfig(string name, string info)
        {
            _scaleMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Scale");
            _translationMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Translation");
            _rotationMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Rotation");

            Color = AppConfig.GetValueFromConfig<Color>(name, info, "");
            TextureResolution = AppConfig.GetValueFromConfig<float>(name, info, "TextureResolution");
            Size = AppConfig.GetValueFromConfig<float>(name, info, "Size");
            ScaleX = AppConfig.GetValueFromConfig<float>(name, info, "ScaleX");
            ScaleY = AppConfig.GetValueFromConfig<float>(name, info, "ScaleY");
            ScaleZ = AppConfig.GetValueFromConfig<float>(name, info, "ScaleZ");
        }

        #endregion

        #region Initialization

        #region Color methods

        protected void AddVertex(Vector3 position, Color color, Vector3 normal)
        {
            _vertices.Add(new VertexPositionColorNormal(position, color, normal));
        }

        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(index));

            _indices.Add((ushort)index);
        }

        protected int CurrentVertex => _vertices.Count;

        #endregion

        #region Texture methods

        protected void PreInitialization(int count, int[] sizes)
        {
            for (var i = 0; i < count; i++)
            {
                _textureVertices.Add(new VertexPositionNormalTexture[sizes[i]]);
                _textureIndices.Add(new List<int>());
            }
        }

        protected void AddVertex(int id, int index, Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            _textureVertices[id][index] = new VertexPositionNormalTexture(position, normal, textureCoordinate);
        }

        protected void AddIndex(int id, int index)
        {
            _textureIndices[id].Add(index);
        }

        protected void MakeVertices(int id, IReadOnlyList<int> faces, Vector3 normal, Matrix rotation)
        {
            var dim1 = faces[0];
            var dim2 = faces[1];
            var dim3 = faces[2];

            for (var i = 0; i < dim1; i++)
            {
                for (var j = 0; j < dim3; j++)
                {
                    var pos1 = i - dim1 / 2;
                    var pos3 = j - dim3 / 2;

                    var position = new Vector3(pos1, dim2, -pos3);
                    var textureCoordinates = new Vector2(pos1 / TextureResolution, pos3 / TextureResolution);

                    AddVertex(id, i + j * dim1, Vector3.Transform(position, rotation), normal, textureCoordinates);
                }
            }
        }

        protected void MakeIndicies(int id, int dim1, int dim2)
        {
            for (var i = 0; i < dim2 - 1; i++)
            {
                for (var j = 0; j < dim1 - 1; j++)
                {
                    var bottomLeft = j + i * dim1;
                    var bottomRight = j + 1 + i * dim1;
                    var topLeft = j + (i + 1) * dim1;
                    var topRight = j + 1 + (i + 1) * dim1;

                    AddIndex(id, topLeft);
                    AddIndex(id, bottomRight);
                    AddIndex(id, bottomLeft);

                    AddIndex(id, topLeft);
                    AddIndex(id, topRight);
                    AddIndex(id, bottomRight);
                }
            }
        }


        #endregion

        #region Common methods

        protected void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            if (_vertices.Any())
            {
                _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorNormal), _vertices.Count, BufferUsage.None);
                _vertexBuffer.SetData(_vertices.ToArray());
            }

            if (_indices.Any())
            {
                _indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), _indices.Count, BufferUsage.None);
                _indexBuffer.SetData(_indices.ToArray());
            }

            Textures = new Texture[2];
        }

        ~GeometricPrimitive()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _vertexBuffer?.Dispose();

            _indexBuffer?.Dispose();
        }

        #endregion

        #endregion

        #region Draw

        public void Draw(Camera c, Effect e)
        {
            if(Textures[0] != null) DrawWithTexture(c, e);
            else DrawWithoutTexture(c, e);
        }

        private void DrawWithoutTexture(Camera camera, Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;
            
            effect.Parameters["CameraPosition"].SetValue(camera.CameraPosition);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            effect.Parameters["World"].SetValue(_rotationMatrix*
                                                _scaleMatrix*
                                                _translationMatrix*
                                                camera.WorldMatrix);

            effect.Parameters["FogEnabled"].SetValue(AppConfig.FogEnabled ? 1f : 0f);
            effect.Parameters["FogStart"].SetValue(AppConfig.FogStart);
            effect.Parameters["FogEnd"].SetValue(AppConfig.FogEnd);

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                var primitiveCount = _indices.Count / 3;

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
            }

            ResetCurrentTechniqueToDefault(effect);
        }

        private void DrawWithTexture(Camera camera, Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            var texture1 = Textures.Length > 1 && Textures[1] != null ? Textures[1] : null;
            effect.CurrentTechnique = texture1 != null ? effect.Techniques["MultiTextured"] : effect.Techniques["Textured"];

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;

            effect.Parameters["CameraPosition"].SetValue(camera.CameraPosition);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            effect.Parameters["World"].SetValue(_rotationMatrix *
                                                _translationMatrix *
                                                camera.WorldMatrix);

            effect.Parameters["Texture"].SetValue(Textures[0]);
            effect.Parameters["TextureMatrix"].SetValue(Matrix.Identity);

            if (texture1 != null)
                effect.Parameters["Texture1"].SetValue(Textures[1]);

            effect.Parameters["FogEnabled"].SetValue(AppConfig.FogEnabled ? 1f : 0f);
            effect.Parameters["FogStart"].SetValue(AppConfig.FogStart);
            effect.Parameters["FogEnd"].SetValue(AppConfig.FogEnd);

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                for (var i = 0; i < _textureVertices.Count; ++i)
                {
                    graphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        _textureVertices[i],
                        0,
                        _textureVertices[i].Length,
                        _textureIndices[i].ToArray(),
                        0,
                        _textureIndices[i].Count/3,
                        VertexPositionNormalTexture.VertexDeclaration);

                }
            }

            ResetCurrentTechniqueToDefault(effect);
        }

        private static void ResetCurrentTechniqueToDefault(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques[DefaultTechnique];
        }

        #endregion
    }
}
