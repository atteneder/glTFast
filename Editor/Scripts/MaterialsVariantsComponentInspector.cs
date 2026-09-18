// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Cloud.Gltfast.Editor
{
    [CustomEditor(typeof(MaterialsVariantsComponent))]
    class MaterialsVariantsComponentInspector : UnityEditor.Editor
    {
        [SerializeField] VisualTreeAsset m_MainMarkup;

        List<string> m_VariantNames;
        DropdownField m_Dropdown;

        public override VisualElement CreateInspectorGUI()
        {
            var control = (target as MaterialsVariantsComponent)?.Control;
            if (control != null && m_VariantNames == null)
            {
                var count = control.MaterialsVariantsCount;
                m_VariantNames = new List<string>(count + 1)
                {
                    "<no variant>"
                };
                for (var variantIndex = 0; variantIndex < count; variantIndex++)
                {
                    m_VariantNames.Add(control.GetMaterialsVariantName(variantIndex));
                }
            }
            var myInspector = new VisualElement();
            m_MainMarkup.CloneTree(myInspector);
            m_Dropdown = myInspector.Query<DropdownField>().First();

            if (m_VariantNames == null)
            {
                myInspector.SetEnabled(false);
            }
            else
            {
                m_Dropdown.choices = m_VariantNames;
                m_Dropdown.index = control != null ? control.CurrentVariantIndex + 1 : 0;
                m_Dropdown.RegisterValueChangedCallback(OnMaterialsVariantChanged);
                myInspector.Add(m_Dropdown);
            }
            return myInspector;
        }

        void OnMaterialsVariantChanged(ChangeEvent<string> evt)
        {
            var control = (target as MaterialsVariantsComponent)?.Control;
            if (control != null)
            {
                _ = control.ApplyMaterialsVariantAsync(m_Dropdown.index - 1);
            }
        }
    }
}
