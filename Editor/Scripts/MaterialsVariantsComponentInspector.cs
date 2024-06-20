// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GLTFast.Editor
{
    [CustomEditor(typeof(MaterialsVariantsComponent))]
    class MaterialsVariantsComponentInspector : UnityEditor.Editor
    {
        [SerializeField] VisualTreeAsset m_MainMarkup;

        List<string> m_VariantNames;
#if UNITY_2021_2_OR_NEWER
        DropdownField m_Dropdown;
#endif

        public override VisualElement CreateInspectorGUI()
        {
            if (m_VariantNames == null)
            {
                var control = (target as MaterialsVariantsComponent)?.Control;
                if (control != null)
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
            }
            var myInspector = new VisualElement();
#if UNITY_2021_2_OR_NEWER
            m_MainMarkup.CloneTree(myInspector);
            m_Dropdown = myInspector.Query<DropdownField>().First();

            if (m_VariantNames == null)
            {
                myInspector.SetEnabled(false);
            }
            else
            {
                m_Dropdown.choices = m_VariantNames;
                m_Dropdown.index = 0;
                m_Dropdown.RegisterValueChangedCallback(OnMaterialsVariantChanged);
                myInspector.Add(m_Dropdown);
            }
#else
            if (m_VariantNames == null)
            {
                myInspector.SetEnabled(false);
            }
            else
            {
                for (var i = 0; i < m_VariantNames.Count; i++)
                {
                    var button = new Button
                    {
                        text = m_VariantNames[i]
                    };

                    button.RegisterCallback<ClickEvent, int>(OnVariantButtonClicked, i - 1); // asset is the root visual element that will be closed
                    myInspector.Add(button);
                }
            }
#endif
            return myInspector;
        }

#if UNITY_2021_2_OR_NEWER
        void OnMaterialsVariantChanged(ChangeEvent<string> evt)
        {
            var control = (target as MaterialsVariantsComponent)?.Control;
            if (control != null)
            {
                _ = control.ApplyMaterialsVariantAsync(m_Dropdown.index - 1);
            }
        }
#else
        void OnVariantButtonClicked(ClickEvent evt, int variantIndex)
        {
            var control = (target as MaterialsVariantsComponent)?.Control;
            if (control != null)
            {
                _ = control.ApplyMaterialsVariantAsync(variantIndex);
            }
        }
#endif
    }
}
