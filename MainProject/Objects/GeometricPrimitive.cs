using System;
using System.Collections.Generic;
using MainProject.Config;
using MainProject.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    internal abstract class GeometricPrimitive : IDisposable
    {
        #region Private fields

        private Matrix _scaleMatrix;
        private Matrix _translationMatrix;
        private Matrix _rotationMatrix;
        
        private readonly List<VertexPositionColorNormal> _vertices = new List<VertexPositionColorNormal>();
        private readonly List<ushort> _indices = new List<ushort>();

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        
        #endregion

        #region Protected fields

        protected float Size;
        protected Color Color;

        #endregion

        #region Preparing operation matrices

        protected void PreloadingConfig(string name, string info)
        {
            _scaleMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Scale");
            _translationMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Translation");
            _rotationMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Rotation");

            Color = AppConfig.GetValueFromConfig<Color>(name, info, "");
            Size = AppConfig.GetValueFromConfig<float>(name, info, "Size");
        }

        #endregion

        #region Initialization

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

        protected void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorNormal), _vertices.Count, BufferUsage.None);
            _vertexBuffer.SetData(_vertices.ToArray());

            _indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort), _indices.Count, BufferUsage.None);
            _indexBuffer.SetData(_indices.ToArray());
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
        }
        
        #endregion
    }
}
