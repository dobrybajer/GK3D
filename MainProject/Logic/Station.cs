using System;
using System.Collections.Generic;
using System.Linq;
using MainProject.Config;
using MainProject.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MainProject.Logic
{
    public class Station : Game
    {
        //private SpriteBatch _spriteBatch;
        //private GameTime PrevUpdateGameTime { get; set; }
        //private GameTime PrevDrawGameTime { get; set; }

        #region Private fields and properties

        private readonly GraphicsDeviceManager _graphics;
        private Effect _effect; // own shader
        private Camera _camera;
        private readonly List<ModelObject> _objects = new List<ModelObject>();
        private readonly List<GeometricPrimitive> _primitives = new List<GeometricPrimitive>();
        private readonly List<GeometricPrimitive> _texturedPrimitives = new List<GeometricPrimitive>();
        private List<Light> _lights;
        private RasterizerState _wireFrameState;
        private bool _isWireframe;
        private KeyboardState _keyboardState;
        private KeyboardState _prevKeyboardState;
        private FilterLevel[] _filters;
        private float _mipMapLevelOfDetailBias = -10;
        private bool _multiSampling;

        #endregion

        #region Constructors

        public Station()
        {
            _graphics = new GraphicsDeviceManager(this);
            
            Window.Title = "Subway Station";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        #endregion

        #region Window events

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.Viewport.Width;
            _graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.Viewport.Height;
            _graphics.ApplyChanges();
        }

        #endregion

        #region Initialize

        protected override void Initialize()
        {
            base.Initialize();
            
            //PrevDrawGameTime = new GameTime();
            //PrevUpdateGameTime = new GameTime();

            _camera = new Camera(_graphics);

            _wireFrameState = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None,
            };

            _filters = new[]
            {
                FilterLevel.Anisotropic,
                FilterLevel.Anisotropic,
                FilterLevel.Anisotropic
            };
        }

        #endregion

        #region Managing Content

        private void AddObjectsToScene()
        {
            _primitives.Add(new Cube(GraphicsDevice, "Station"));
            _texturedPrimitives.Add(new TexturedCube(GraphicsDevice, "Platform", Content.Load<Texture>("Textures/rock1")));
            _primitives.Add(new TexturedFloor(GraphicsDevice, "Ground", Content.Load<Texture>("Textures/ground1")));

            _lights = new List<Light>
            {
                new Light
                {
                    LightType = 0,
                    DiffuseColor = Color.White,
                    DiffuseIntensity = 0.3f,
                    SpecularColor = Color.Red,
                    SpecularIntensity = 0.2f,
                    SpecularPower = 32,
                    LightPosition = new Vector3(-200, 250, 0),
                    Enabled = false
                },
                new Light
                {
                    LightType = 0,
                    DiffuseColor = Color.White,
                    DiffuseIntensity = 0.3f,
                    SpecularColor = Color.White,
                    SpecularIntensity = 0.2f,
                    SpecularPower = 32,
                    LightPosition = new Vector3(300, 250, 0),
                    Enabled = false
                },
                new Light
                {
                    LightType = 1,
                    LightPosition = new Vector3(0, 0, 0),
                    LightDirection = new Vector3(1, 0, 0),
                    DiffuseColor = Color.AliceBlue,
                    DiffuseIntensity = 0.5f,
                    SpecularColor = Color.Orange,
                    SpecularIntensity = 0.1f,
                    SpecularPower = 32,
                    Enabled = false
                },
                new Light
                {
                    LightType = 2,
                    LightPosition = new Vector3(500, 100, -50),
                    LightDirection = new Vector3(0, 0, 1),
                    DiffuseColor = Color.Red,
                    DiffuseIntensity = 0.6f,
                    SpecularColor = Color.Yellow,
                    SpecularIntensity = 0.3f,
                    SpecularPower = 32,
                    Enabled = true,
                    SpotAngle = MathHelper.ToRadians(1f)
                }
            };
        }

        private void LoadData()
        {
            _effect = Content.Load<Effect>("Shaders/shader");
            _objects.Add(new ModelObject(Content, "Panther", "Model", Content.Load<Texture>("Textures/matrix1")));
            _objects.Add(new ModelObject(_objects[0].GetModel(), "Panther", "Model2", Content.Load<Texture>("Textures/daradevil")));
            _objects.Add(new ModelObject(Content, "Locomotive", "Model", Content.Load<Texture>("Textures/metal")));
        }

        protected override void LoadContent()
        {
            Content.RootDirectory = "Content";

            LoadData();
            AddObjectsToScene();
        }

        protected override void UnloadContent()
        {

        }

        #endregion

        #region Update

        protected override void Update(GameTime gameTime)
        {
            _keyboardState = Keyboard.GetState();

            if (_keyboardState.IsKeyDown(Keys.Escape)) Exit();

            var pressedKeys = _keyboardState.GetPressedKeys();
            var prevPressedKeys = _prevKeyboardState.GetPressedKeys();

            _camera.Update(pressedKeys);
            UpdateOnActions(pressedKeys, prevPressedKeys);
            UpdateTexturesParameters(pressedKeys, prevPressedKeys);
            UpdateFogParameters(pressedKeys, prevPressedKeys);

            _prevKeyboardState = _keyboardState;
            //PrevUpdateGameTime = gameTime;

            base.Update(gameTime);
        }

        private void UpdateOnActions(Keys[] pressedKeys, Keys[] prevPressedKeys)
        {
            if (pressedKeys.Contains(Keys.C) && !prevPressedKeys.Contains(Keys.C)) _isWireframe = !_isWireframe;
            if (pressedKeys.Contains(Keys.D1) && !prevPressedKeys.Contains(Keys.D1)) _lights[0].Enabled = !_lights[0].Enabled;
            if (pressedKeys.Contains(Keys.D2) && !prevPressedKeys.Contains(Keys.D2)) _lights[1].Enabled = !_lights[1].Enabled;
            if (pressedKeys.Contains(Keys.D3) && !prevPressedKeys.Contains(Keys.D3)) _lights[2].Enabled = !_lights[2].Enabled;
            if (pressedKeys.Contains(Keys.D4) && !prevPressedKeys.Contains(Keys.D4)) _lights[3].Enabled = !_lights[3].Enabled;
        }

        private static void UpdateFogParameters(Keys[] pressedKeys, IEnumerable<Keys> prevPressedKeys)
        {
            if (pressedKeys.Contains(Keys.F) && !prevPressedKeys.Contains(Keys.F)) AppConfig.FogEnabled = !AppConfig.FogEnabled;
            if (pressedKeys.Contains(Keys.G)) AppConfig.FogStart += 5f;
            if (pressedKeys.Contains(Keys.H)) AppConfig.FogStart -= 5f;
            if (pressedKeys.Contains(Keys.B)) AppConfig.FogEnd += 5f;
            if (pressedKeys.Contains(Keys.N)) AppConfig.FogEnd -= 5f;
        }

        private void UpdateTexturesParameters(Keys[] pressedKeys, Keys[] prevPressedKeys)
        {
            if (pressedKeys.Contains(Keys.OemMinus)) _mipMapLevelOfDetailBias -= 0.1f;
            if (pressedKeys.Contains(Keys.OemPlus)) _mipMapLevelOfDetailBias += 0.1f;
            
            if (pressedKeys.Contains(Keys.Multiply) && !prevPressedKeys.Contains(Keys.Multiply)) _filters[0] = (FilterLevel)(((int)_filters[0] + 1) % 3);
            if (pressedKeys.Contains(Keys.OemOpenBrackets) && !prevPressedKeys.Contains(Keys.OemOpenBrackets)) _filters[1] = (FilterLevel)(((int)_filters[1] + 1) % 3);
            if (pressedKeys.Contains(Keys.OemCloseBrackets) && !prevPressedKeys.Contains(Keys.OemCloseBrackets)) _filters[2] = (FilterLevel)(((int)_filters[2] + 1) % 3);

            if (pressedKeys.Contains(Keys.D8) && !prevPressedKeys.Contains(Keys.D8)) UpdateFiltersWithValue(FilterLevel.Point);
            if (pressedKeys.Contains(Keys.D9) && !prevPressedKeys.Contains(Keys.D9)) UpdateFiltersWithValue(FilterLevel.Linear);
            if (pressedKeys.Contains(Keys.D0) && !prevPressedKeys.Contains(Keys.D0)) UpdateFiltersWithValue(FilterLevel.Anisotropic);

            if (pressedKeys.Contains(Keys.M) && !prevPressedKeys.Contains(Keys.M)) _multiSampling = !_multiSampling;

            /* ground textures change */
            //if (pressedKeys.Contains(Keys.T) && !prevPressedKeys.Contains(Keys.T))
            //{
            //    if (_ground.Texture1 == _sandTexture)
            //        _ground.Texture1 = _snowTexture;
            //    else if (_ground.Texture1 == _snowTexture)
            //        _ground.Texture1 = _rockTexture;
            //    else if (_ground.Texture1 == _rockTexture)
            //        _ground.Texture1 = null;
            //    else if (_ground.Texture1 == null)
            //        _ground.Texture1 = _sandTexture;
            //}
        }

        private void UpdateFiltersWithValue(FilterLevel value)
        {
            for (var i = 0; i < _filters.Length; i++)
            {
                _filters[i] = value;
            }
        }

        #endregion

        #region Draw

        protected override void Draw(GameTime gameTime)
        {
            ConfigureShader();
            SetUpGraphicDeviceParameters();
            DrawScene(_camera);
            
            base.Draw(gameTime);
        }

        private void SetUpGraphicDeviceParameters()
        {
            _graphics.GraphicsDevice.Clear(Color.White);
            
            //_graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            _graphics.PreferMultiSampling = _multiSampling;
            _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = _multiSampling ? 8 : 0;
            _graphics.GraphicsDevice.RasterizerState = _isWireframe ? _wireFrameState : new RasterizerState
            {
                MultiSampleAntiAlias = _multiSampling,
                CullMode = CullMode.CullCounterClockwiseFace
            };

            var ss = new SamplerState
            {
                Filter = TextureFilterFromMinMagMip(_filters),
                //MaxMipLevel = 255,
                MaxAnisotropy = 16,
                MipMapLevelOfDetailBias = _mipMapLevelOfDetailBias,
                AddressU = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap
            };

            _graphics.GraphicsDevice.SamplerStates[0] = ss;
            _graphics.GraphicsDevice.SamplerStates[1] = ss;

            _graphics.ApplyChanges();
        }

        private void DrawScene(Camera camera)
        {
            var originalRasterizeState = _graphics.GraphicsDevice.RasterizerState;
            _graphics.GraphicsDevice.RasterizerState = _isWireframe ? _wireFrameState : RasterizerState.CullNone;

            foreach (var p in _primitives) p.Draw(camera, _effect);
            foreach (var p in _texturedPrimitives) p.Draw(camera, _effect);

            _graphics.GraphicsDevice.RasterizerState = originalRasterizeState;

            foreach (var o in _objects) o.Draw(camera, _effect);
        }

        private void ConfigureShader()
        {
            _effect.CurrentTechnique = _effect.Techniques["BasicPhongLightning"];

            _effect.Parameters["Projection"].SetValue(_camera.ProjectionMatrix);
            _effect.Parameters["View"].SetValue(_camera.ViewMatrix);
            _effect.Parameters["World"].SetValue(_camera.WorldMatrix);

            _effect.Parameters["CameraPosition"].SetValue(_camera.CameraPosition);

            _effect.Parameters["LightType"].SetValue(_lights.Select(l => (float)l.LightType).ToArray());
            _effect.Parameters["LightEnabled"].SetValue(_lights.Select(l => l.Enabled ? 1.0f : 0.0f).ToArray());
            _effect.Parameters["LightPosition"].SetValue(_lights.Select(l => l.LightPosition).ToArray());
            _effect.Parameters["LightDirection"].SetValue(_lights.Select(l => l.LightDirection).ToArray());
            _effect.Parameters["SpotAngle"].SetValue(_lights.Select(l => l.SpotAngle).ToArray());

            _effect.Parameters["DiffuseColors"].SetValue(_lights.Select(l => l.DiffuseColor.ToVector4()).ToArray());
            _effect.Parameters["DiffuseIntensities"].SetValue(_lights.Select(l => l.DiffuseIntensity).ToArray());
            _effect.Parameters["SpecularColors"].SetValue(_lights.Select(l => l.SpecularColor.ToVector4()).ToArray());
            _effect.Parameters["SpecularIntensities"].SetValue(_lights.Select(l => l.SpecularIntensity).ToArray());
            _effect.Parameters["SpecularPower"].SetValue(_lights.Select(l => l.SpecularPower).ToArray());

            _effect.Parameters["AmbientIntensity"].SetValue(0.2f);
            _effect.Parameters["AmbientColor"].SetValue(Color.LightSeaGreen.ToVector4());
        }

        #endregion

        #region Filters

        private static TextureFilter TextureFilterFromMinMagMip(IReadOnlyList<FilterLevel> filters)
        {
            var minFilter = filters[0];
            var magFilter = filters[1];
            var mipFilter = filters[2];

            var value = "Min" + minFilter + "Mag" + magFilter + "Mip" + mipFilter;

            TextureFilter filter;
            var parsedValue = Enum.TryParse(value, out filter);

            if (parsedValue) return filter;

            if (minFilter == FilterLevel.Anisotropic && magFilter == FilterLevel.Anisotropic) return TextureFilter.Anisotropic;
            if (minFilter == FilterLevel.Point && magFilter == FilterLevel.Point)
            {
                return mipFilter == FilterLevel.Linear ? TextureFilter.PointMipLinear : TextureFilter.Point;
            }
            return mipFilter == FilterLevel.Point ? TextureFilter.LinearMipPoint : TextureFilter.Linear;
        }

        private enum FilterLevel
        {
            Point = 0,
            Linear = 1,
            Anisotropic = 2
        }

        #endregion
    }
}
