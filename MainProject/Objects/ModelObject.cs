using MainProject.Config;
using MainProject.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    internal class ModelObject
    {
        #region Private fields

        private Matrix _scaleMatrix;
        private Matrix _translationMatrix;
        private Matrix _rotationMatrix;
        private readonly Model _model;
        private Color _color;

        #endregion

        #region Get Methods

        public Model GetModel()
        {
            return _model;
        }

        #endregion

        #region Constructors

        public ModelObject(Model model, string name, string info)
        {
            _model = model;
            PreloadingConfig(name, info);
        }

        public ModelObject(ContentManager content, string name, string info)
        {
            _model = content.Load<Model>($"Models/{name.ToLower()}");
            PreloadingConfig(name, info);
        }

        private void PreloadingConfig(string name, string info)
        {
            _scaleMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Scale");
            _translationMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Translation");
            _rotationMatrix = AppConfig.GetValueFromConfig<Matrix>(name, info, "Rotation");

            _color = AppConfig.GetValueFromConfig<Color>(name, info, "");
        }

        #endregion

        #region Draw

        // Old function with default BasicEffect
        public void Draw(Camera camera)
        {
            foreach (var mesh in _model.Meshes)
            {
                foreach (var effect1 in mesh.Effects)
                {
                    var effect = (BasicEffect)effect1;
                    effect.EnableDefaultLighting();
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                    effect.World = _rotationMatrix*
                                   _scaleMatrix*
                                   _translationMatrix*
                                   camera.WorldMatrix;
                }
                mesh.Draw();
            }
        }

        public void Draw(Camera camera, Effect e)
        {
            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = e;
                }

                foreach (var effect in mesh.Effects)
                {
                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        effect.Parameters["CameraPosition"].SetValue(camera.CameraPosition);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                        effect.Parameters["World"].SetValue(_rotationMatrix*
                                                            _scaleMatrix*
                                                            _translationMatrix*
                                                            camera.WorldMatrix);
                    }
                }
                mesh.Draw();
            }
        }

        #endregion
    }
}
