// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_URP || USING_HDRP || (UNITY_SHADER_GRAPH_12_OR_NEWER && GLTFAST_BUILTIN_SHADER_GRAPH)
#define GLTFAST_SHADER_GRAPH
#endif

#if GLTFAST_SHADER_GRAPH || UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using static GLTFast.Materials.MaterialGenerator;

namespace GLTFast.Editor
{
    class ShaderGraphGUI : ShaderGUIBase
    {

        UvTransform? m_UVTransform;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor.target is Material material) {
                m_UVTransform = TextureRotationSlider(
                    material,
                    m_UVTransform,
                    BaseColorTextureScaleTransformProperty,
                    BaseColorTextureRotationProperty,
                    label:"Base Color Tex Rotation"
                    );

                if (GUI.changed) {
                    EditorUtility.SetDirty(material);
                }
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
