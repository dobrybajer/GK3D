using MainProject.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MainProject.Logic
{
    internal class Camera
    {
        #region Private properties

        private float CameraSpeed { get; } = AppConfig.CameraSpeed;
        private Vector3 CameraDirection { get; set; }
        private Vector3 CameraVectorUp { get; set; }

        #endregion

        #region Public properties
        
        public Matrix ProjectionMatrix { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Matrix WorldMatrix { get; set; }
        public Vector3 CameraPosition { get; set; }

        #endregion

        #region Constructors

        public Camera(GraphicsDeviceManager graphics) : this(graphics, AppConfig.CameraPosition, AppConfig.CameraVectorUp, AppConfig.CameraDirection) { }

        public Camera(GraphicsDeviceManager graphics, Vector3 cameraPosition, Vector3 cameraVectorUp, Vector3 cameraDirection)
        {
            CameraPosition = cameraPosition;
            CameraVectorUp = cameraVectorUp;
            CameraDirection = cameraDirection;

            var aspectRatio = graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;

            WorldMatrix = Matrix.CreateWorld(
                Vector3.Zero, 
                Vector3.Forward, 
                Vector3.Up);
            ViewMatrix = Matrix.CreateLookAt(
                CameraPosition, 
                CameraPosition + CameraDirection, 
                CameraVectorUp);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(AppConfig.FieldOfView), 
                aspectRatio, 
                AppConfig.NearPlaneDistance, 
                AppConfig.FarPlaneDistance);
        }

        #endregion

        public void Update(Keys[] pressedKeys)
        {
            foreach (var action in pressedKeys)
            {
                var crossVector = Vector3.Cross(CameraVectorUp, CameraDirection);
                var angle = AppConfig.CameraRotationAngle*CameraSpeed;

                switch (action)
                {
                    case Keys.Up: // MoveForward
                        CameraPosition += CameraDirection * CameraSpeed;
                        break;
                    case Keys.Down: // MoveBack
                        CameraPosition -= CameraDirection * CameraSpeed;
                        break;
                    case Keys.Left: // MoveLeft
                        CameraPosition += crossVector * CameraSpeed;
                        break;
                    case Keys.Right: // MoveRight
                        CameraPosition -= crossVector * CameraSpeed;
                        break;
                    case Keys.Z:  // GoUp
                        CameraPosition += CameraVectorUp * CameraSpeed;
                        break;
                    case Keys.X: // GoDown
                        CameraPosition -= CameraVectorUp * CameraSpeed;
                        break;
                    case Keys.D: // Yaw
                        CameraDirection = Vector3.Transform(CameraDirection, Matrix.CreateFromAxisAngle(CameraVectorUp, -angle));
                        break;
                    case Keys.A: // CounterYaw
                        CameraDirection = Vector3.Transform(CameraDirection, Matrix.CreateFromAxisAngle(CameraVectorUp, angle));
                        break;
                    case Keys.S: // CounterPitch
                        CameraDirection = Vector3.Transform(CameraDirection, Matrix.CreateFromAxisAngle(crossVector, angle));
                        CameraVectorUp = Vector3.Transform(CameraVectorUp, Matrix.CreateFromAxisAngle(crossVector, angle));
                        break;
                    case Keys.W: // Pitch
                        CameraDirection = Vector3.Transform(CameraDirection, Matrix.CreateFromAxisAngle(crossVector, -angle));
                        CameraVectorUp = Vector3.Transform(CameraVectorUp, Matrix.CreateFromAxisAngle(crossVector, -angle));
                        break;
                    case Keys.E: // Roll
                        CameraVectorUp = Vector3.Transform(CameraVectorUp, Matrix.CreateFromAxisAngle(CameraDirection, angle));
                        break;
                    case Keys.Q: // CounterRoll
                        CameraVectorUp = Vector3.Transform(CameraVectorUp, Matrix.CreateFromAxisAngle(CameraDirection, -angle));
                        break;
                    case Keys.R: // Reset
                        CameraDirection = AppConfig.CameraDirection;
                        CameraPosition = AppConfig.CameraPosition;
                        CameraVectorUp = AppConfig.CameraVectorUp;
                        break;
                }
            }

            ViewMatrix = Matrix.CreateLookAt(CameraPosition, CameraPosition + CameraDirection, CameraVectorUp);
        }
    }
}
