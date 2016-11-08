using Microsoft.Xna.Framework;

namespace MainProject.Objects
{
    internal class Light
    {
        public int LightType { get; set; }
        public bool Enabled { get; set; }
        public Vector3 LightPosition { get; set; }
        public Vector3 LightDirection { get; set; } = new Vector3(0, 0, 0);
        public Color DiffuseColor { get; set; }
        public float DiffuseIntensity { get; set; }
        public Color SpecularColor { get; set; }
        public float SpecularIntensity { get; set; }
        public float SpecularPower { get; set; }
        public float SpotAngle { get; set; }
    }
}
