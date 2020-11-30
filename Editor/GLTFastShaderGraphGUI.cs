#if GLTFAST_SHADER_GRAPH
using System;
using System.Collections.Generic;
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
                uvTransform = TextureRotationSlider(
                    material,
                    uvTransform,
                    mainTexScaleTransform,
                    mainTexRotation
                    );
            }

            // var filteredProperties = new List<MaterialProperty>();
            // foreach (var property in properties)
            // {
            //     if (property.name != "baseColorTextureRotation") {
            //         filteredProperties.Add(property);
            //     }
            // }
            // base.OnGUI(materialEditor, filteredProperties.ToArray());
            
            base.OnGUI(materialEditor, properties);
        }
    }
}
#endif
