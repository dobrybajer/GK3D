using MainProject.Config;
using MainProject.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MainProject.Objects
{
    internal class ModelObject
    {
        #region Consts

        private const string DefaultTechnique = "BasicPhongLightning";

        #endregion

        #region Private fields

        private Matrix _scaleMatrix;
        private Matrix _translationMatrix;
        private Matrix _rotationMatrix;
        private readonly Model _model;
        private Color _color;
        private Texture _texture;

        #endregion

        #region Get Methods

        public Model GetModel()
        {
            return _model;
        }

        #endregion

        #region Constructors

        public ModelObject(Model model, string name, string info, Texture texture = null)
        {
            _model = model;
            _texture = texture;
            PreloadingConfig(name, info);
        }

        public ModelObject(ContentManager content, string name, string info, Texture texture = null)
        {
            _model = content.Load<Model>($"Models/{name.ToLower()}");
            _texture = texture;
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
            if (_texture == null) DrawWithoutTexture(camera, e);
            else DrawWithTexture(camera, e);
        }

        private void DrawWithoutTexture(Camera camera, Effect e)
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

                        effect.Parameters["FogEnabled"].SetValue(AppConfig.FogEnabled ? 1f : 0f);
                        effect.Parameters["FogStart"].SetValue(AppConfig.FogStart);
                        effect.Parameters["FogEnd"].SetValue(AppConfig.FogEnd);
                    }
                }
                mesh.Draw();
            }

            ResetCurrentTechniqueToDefault(e);
        }

        private void DrawWithTexture(Camera camera, Effect e)
        {
            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = e;
                }

                foreach (var effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Textured"];

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        effect.Parameters["CameraPosition"].SetValue(camera.CameraPosition);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                        effect.Parameters["World"].SetValue(_rotationMatrix *
                                                            _scaleMatrix *
                                                            _translationMatrix *
                                                            camera.WorldMatrix);

                        effect.Parameters["Texture"].SetValue(_texture);
                        effect.Parameters["TextureMatrix"].SetValue(Matrix.Identity);

                        effect.Parameters["FogEnabled"].SetValue(AppConfig.FogEnabled ? 1f : 0f);
                        effect.Parameters["FogStart"].SetValue(AppConfig.FogStart);
                        effect.Parameters["FogEnd"].SetValue(AppConfig.FogEnd);
                    }
                }
                mesh.Draw();
            }

            ResetCurrentTechniqueToDefault(e);
        }

        private static void ResetCurrentTechniqueToDefault(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques[DefaultTechnique];
        }

        #endregion

    }
}
