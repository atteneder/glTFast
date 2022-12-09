// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

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
