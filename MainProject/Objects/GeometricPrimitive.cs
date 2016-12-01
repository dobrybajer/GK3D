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

        public void Draw(Camera camera, Effect effect)
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

            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                var primitiveCount = _indices.Count / 3;

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
            }

            ResetCurrentTechniqueToDefault(effect);
        }

        public virtual void Draw(Camera camera, Effect effect, bool textured)
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
