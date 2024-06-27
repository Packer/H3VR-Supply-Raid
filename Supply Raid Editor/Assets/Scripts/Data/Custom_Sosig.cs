using UnityEngine;

namespace CustomSosigLoader
{
    [System.Serializable]
    public class Custom_Sosig
    {
        public string name; //For internal use
        public SosigEnemyID baseSosigID = SosigEnemyID.Misc_Dummy;
        public string customTextureName = "";

        //Voice
        public float voicePitch = 1;
        public float voiceVolume = 1;

        //Scale
        public Vector3 scaleBody = Vector3.one;
        public Vector3 scaleHead = Vector3.one;
        public Vector3 scaleTorso = Vector3.one;
        public Vector3 scaleLegsUpper = Vector3.one;
        public Vector3 scaleLegsLower = Vector3.one;

        //Materials
        public bool useCustomSkin = false;  //Default or White
        public Color color;
        public float metallic = 0;
        public float specularity = 0.3f;
        public float specularTint = 0f;
        public float roughness = 1f;
        public float normalStrength = 1f;
        public bool specularHighlights = true;
        public bool glossyReflections = true;
    }
}
