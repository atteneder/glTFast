// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Materials
{

    /// <summary>
    /// Contains material related constant variables that are required for both
    /// import (glTF to Unity) and export (Unity to glTF) material conversions.
    /// TODO: Make const var location consistent
    /// </summary>
    public static class Constants
    {

        /// <summary>
        /// Shader keyword for normal mapping
        /// </summary>
        public const string NormalMapKeyword = "_NORMALMAP";
    }
}
