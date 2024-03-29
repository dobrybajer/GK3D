﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace MainProject.Config
{
    internal class AppConfig
    {
        #region Live configuration

        public static bool FogEnabled = false;
        public static float FogStart = 1f;
        public static float FogEnd = 500;

        #endregion

        #region Camera

        public const float CameraSpeed = 2f;
        public static readonly Vector3 CameraPosition = new Vector3(0f, 0f, -200f); // 400f, 90f, 80f
        public static readonly Vector3 CameraDirection = new Vector3(0f, 0f, 1f); // -1f, -0.9f, -0.9f
        public static readonly Vector3 CameraVectorUp = new Vector3(0f, 1f, 0f);
        public static readonly float CameraRotationAngle = MathHelper.PiOver4/150*5;

        #endregion

        #region Plane

        public const float NearPlaneDistance = 1;
        public const float FarPlaneDistance = 1200;
        public const float FieldOfView = 45f; // degrees

        #endregion

        #region Park Objects

        // Station box
        public const float StationCubeSizeSingle = 1000;
        public static readonly Color StationCubeColor = Color.DarkRed;
        public static readonly Matrix StationCubeScaleMatrix = Matrix.CreateScale(1f, 0.25f, 0.25f);

        // Platform
        public static readonly float PlatformCubeTextureResolutionSingle = 100;
        public const float PlatformCubeSizeSingle = 1000;
        public static readonly Color PlatformCubeColor = Color.DarkSeaGreen;
        public static readonly float PlatformCubeScaleXSingle = 1f;
        public static readonly float PlatformCubeScaleYSingle = 0.025f;
        public static readonly float PlatformCubeScaleZSingle = 0.125f;
        public static readonly Matrix PlatformCubeScaleMatrix = Matrix.CreateScale(PlatformCubeScaleXSingle, PlatformCubeScaleYSingle, PlatformCubeScaleZSingle);
        public static readonly Matrix PlatformCubeTranslationMatrix = Matrix.CreateTranslation(0f, -62f, 59f);

        // Floor
        public static readonly float GroundFloorTextureResolutionSingle = 100;
        public const float GroundFloorSizeSingle = 1000;
        public static readonly Color GroundFloorColor = Color.DimGray;
        public static readonly float GroundFloorScaleXSingle = 1f;
        public static readonly float GroundFloorScaleYSingle = 0.25f;
        public static readonly float GroundFloorScaleZSingle = 0.25f;
        public static readonly Matrix GroundFloorScaleMatrix = Matrix.CreateScale(GroundFloorScaleXSingle, GroundFloorScaleYSingle, GroundFloorScaleZSingle);
        public static readonly Matrix GroundFloorTranslationMatrix = Matrix.CreateTranslation(0f, -StationCubeSizeSingle / 5, 0);

        // Screen as Cube
        public static readonly float ScreenCubeTextureResolutionSingle = 100;
        public const float ScreenCubeSizeSingle = 100;
        public static readonly float ScreenCubeScaleXSingle = 1f;
        public static readonly float ScreenCubeScaleYSingle = 1f;
        public static readonly float ScreenCubeScaleZSingle = 1f;
        public static readonly bool ScreenCubeTextureTranslationBoolean = true;
        public static readonly Matrix ScreenCubeScaleMatrix = Matrix.CreateScale(ScreenCubeScaleXSingle, ScreenCubeScaleYSingle, ScreenCubeScaleZSingle);
        public static readonly Matrix ScreenCubeTranslationMatrix = Matrix.CreateTranslation(0f, 0f, 0f);
        public static readonly Matrix ScreenCubeRotationMatrix = Matrix.Identity;

        // Screen as Floor
        public static readonly float ScreenFloorTextureResolutionSingle = 100; // 50
        public const float ScreenFloorSizeSingle = 100;
        public static readonly float ScreenFloorScaleXSingle = 1f;
        public static readonly float ScreenFloorScaleYSingle = 1f;
        public static readonly float ScreenFloorScaleZSingle = 1f;
        public static readonly bool ScreenFloorTextureTranslationBoolean = true; // false
        public static readonly Matrix ScreenFloorScaleMatrix = Matrix.CreateScale(ScreenFloorScaleXSingle, ScreenFloorScaleYSingle, ScreenFloorScaleZSingle);
        public static readonly Matrix ScreenFloorTranslationMatrix = Matrix.CreateTranslation(0f, 0f, 0f); // -540f, 0f, 70f
        public static readonly Matrix ScreenFloorRotationMatrix = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(0), MathHelper.ToRadians(-90), MathHelper.ToRadians(180f)); // 90

        // Panther
        public static readonly Matrix PantherModelScaleMatrix = Matrix.CreateScale(1f, 1f, 1f);
        public static readonly Matrix PantherModelTranslationMatrix = Matrix.CreateTranslation(-140f, -49f, 3f);
        public static readonly Matrix PantherModelRotationMatrix = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(180), MathHelper.ToRadians(0), MathHelper.ToRadians(0));

        // Panther 2
        public static readonly Matrix PantherModel2ScaleMatrix = Matrix.CreateScale(1f, 1f, 1f);
        public static readonly Matrix PantherModel2TranslationMatrix = Matrix.CreateTranslation(-110f, -48f, 15f);
        public static readonly Matrix PantherModel2RotationMatrix = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(210), MathHelper.ToRadians(0), MathHelper.ToRadians(0));

        // Locomotive
        public static readonly Matrix LocomotiveModelScaleMatrix = Matrix.CreateScale(0.5f, 0.5f, 0.5f);
        public static readonly Matrix LocomotiveModelTranslationMatrix = Matrix.CreateTranslation(0f, -26f, -50f); 
        public static readonly Matrix LocomotiveModelRotationMatrix = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(90), MathHelper.ToRadians(0), MathHelper.ToRadians(270));

        #endregion

        #region GetValueFromConfig method

        public static T GetValueFromConfig<T>(string name, string info, string type)
        {
            var field = typeof(AppConfig).GetFields(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(f => f.Name == $"{name}{info}{type}{typeof(T).Name}");
            var value = field?.GetValue(null);
            if (value != null) return (T)value;

            if (typeof(T) == typeof(Matrix))
            {
                switch (type)
                {
                    case "Scale":
                        return (T)Convert.ChangeType(Matrix.CreateScale(1f, 1f, 1f), typeof(T));
                    case "Translation":
                        return (T)Convert.ChangeType(Matrix.CreateTranslation(0f, 0f, 0f), typeof(T));
                    case "Rotation":
                        return (T)Convert.ChangeType(Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(0f), MathHelper.ToRadians(0f), MathHelper.ToRadians(0f)), typeof(T));
                    default:
                        return default(T);
                }
            }

            if(typeof(T) == typeof(Color))
            {
                return (T)Convert.ChangeType(Color.Azure, typeof(T));
            }

            if (typeof(T) == typeof(float))
            {
                return (T)Convert.ChangeType(1, typeof(T));
            }

            if (typeof(T) == typeof(bool))
            {
                return (T)Convert.ChangeType(true, typeof(T));
            }

            return default(T);
        }

        #endregion
    }
}
