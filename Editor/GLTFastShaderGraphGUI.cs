#if GLTFAST_SHADER_GRAPH
using System;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using static GLTFast.Materials.StandardShaderHelper;

namespace GLTFast.Editor
{
    public class GLTFastShaderGraphGUI : GLTFastShaderGUIBase
    {

        private UvTransform? uvTransform;
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor.target is Material material) {
                uvTransform = TextureRotationSlider(material, uvTransform, ShaderGraphMaterialGenerator.baseColorTextureRotationScalePropId );
            }

            base.OnGUI(materialEditor, properties);
        }
    }
}
#endif
