using UnityEngine;

namespace GLTFast.Schema {

    /// <summary>
    /// The material appearance of a primitive.
    /// </summary>
    [System.Serializable]
    public class Material : RootChild {

        public enum AlphaMode
        {
            OPAQUE,
            MASK,
            BLEND
        }

        public MaterialExtension extensions;


        /// <summary>
        /// A set of parameter values that are used to define the metallic-roughness
        /// material model from Physically-Based Rendering (PBR) methodology.
        /// </summary>
        public PbrMetallicRoughness pbrMetallicRoughness;

        /// <summary>
        /// A set of parameter values used to light flat-shaded materials
        /// </summary>
        //public MaterialCommonConstant CommonConstant;

        /// <summary>
        /// A tangent space normal map. Each texel represents the XYZ components of a
        /// normal vector in tangent space.
        /// </summary>
        public NormalTextureInfo normalTexture;

        /// <summary>
        /// The occlusion map is a greyscale texture, with white indicating areas that
        /// should receive full indirect lighting and black indicating no indirect
        /// lighting.
        /// </summary>
        public OcclusionTextureInfo occlusionTexture;

        /// <summary>
        /// The emissive map controls the color and intensity of the light being emitted
        /// by the material. This texture contains RGB components in sRGB color space.
        /// If a fourth component (A) is present, it is ignored.
        /// </summary>
        public TextureInfo emissiveTexture;

        /// <summary>
        /// The RGB components of the emissive color of the material.
        /// If an emissiveTexture is specified, this value is multiplied with the texel
        /// values.
        /// <items>
        ///  <minimum>0.0</minimum>
        ///  <maximum>1.0</maximum>
        /// </items>
        /// <minItems>3</minItems>
        /// <maxItems>3</maxItems>
        /// </summary>
        [UnityEngine.SerializeField]
        float[] emissiveFactor = {0,0,0};

        public Color emissive {
            get {
                return new Color(
                    emissiveFactor[0],
                    emissiveFactor[1],
                    emissiveFactor[2]
                );
            }
        }

        /// <summary>
        /// The material's alpha rendering mode enumeration specifying the interpretation of the
        /// alpha value of the main factor and texture. In `OPAQUE` mode, the alpha value is
        /// ignored and the rendered output is fully opaque. In `MASK` mode, the rendered output
        /// is either fully opaque or fully transparent depending on the alpha value and the
        /// specified alpha cutoff value. In `BLEND` mode, the alpha value is used to composite
        /// the source and destination areas. The rendered output is combined with the background
        /// using the normal painting operation (i.e. the Porter and Duff over operator).
        /// </summary>
        [SerializeField]
        string alphaMode;

        AlphaMode? _alphaModeEnum;
        public AlphaMode alphaModeEnum {
            get {
                if ( _alphaModeEnum.HasValue ) {
                    return _alphaModeEnum.Value;
                }
                if (!string.IsNullOrEmpty (alphaMode)) {
                    _alphaModeEnum = (AlphaMode)System.Enum.Parse (typeof(AlphaMode), alphaMode, true);
                    alphaMode = null;
                    return _alphaModeEnum.Value;
                } else {
                    return AlphaMode.OPAQUE;
                }
            }
        }

        /// <summary>
        /// Specifies the cutoff threshold when in `MASK` mode. If the alpha value is greater than
        /// or equal to this value then it is rendered as fully opaque, otherwise, it is rendered
        /// as fully transparent. This value is ignored for other modes.
        /// </summary>
        public float alphaCutoff = 0.5f;

        /// <summary>
        /// Specifies whether the material is double sided. When this value is false, back-face
        /// culling is enabled. When this value is true, back-face culling is disabled and double
        /// sided lighting is enabled. The back-face must have its normals reversed before the
        /// lighting equation is evaluated.
        /// </summary>
        public bool doubleSided = false;
    }
}