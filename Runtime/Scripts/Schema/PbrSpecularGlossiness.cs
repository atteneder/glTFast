using UnityEngine;

namespace GLTFast.Schema {

    [System.Serializable]
    public class PbrSpecularGlossiness {
        public float[] diffuseFactor = { 1, 1, 1, 1 };

        public Color diffuseColor {
            get {
                return new Color(
                    diffuseFactor[0],
                    diffuseFactor[1],
                    diffuseFactor[2],
                    diffuseFactor[3]
                );
            }
        }


        public TextureInfo diffuseTexture = null;

        /// </summary>
        public float[] specularFactor = { 1, 1, 1 };

        public Color specularColor {
            get {
                return new Color(
                    specularFactor[0],
                    specularFactor[1],
                    specularFactor[2]
                );
            }
        }


        /// </summary>
        public float glossinessFactor = 1;

        public TextureInfo specularGlossinessTexture = null;
    }
}


