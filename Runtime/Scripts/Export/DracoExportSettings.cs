// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if DRACO_UNITY
using Draco.Encode;
#endif

namespace GLTFast.Export
{
    /// <summary>
    /// Settings for Draco mesh compression
    /// </summary>
    public class DracoExportSettings
    {
        // TODO: Look into world-space size and precision based quantization
        // public float positionalPrecision = 0.001f;

        /// <summary>Encoding speed level. 0 means slow and small. 10 is fastest.</summary>
        public int encodingSpeed = 0;

        /// <summary>Decoding speed level. 0 means slow and small. 10 is fastest.</summary>
        public int decodingSpeed = 4;

        /// <summary>Positional quantization.</summary>
        public int positionQuantization = 14;

        /// <summary>Normal quantization.</summary>
        public int normalQuantization = 10;

        /// <summary>Texture coordinate quantization.</summary>
        public int texCoordQuantization = 12;

        /// <summary>Color quantization.</summary>
        public int colorQuantization = 8;

#if DRACO_UNITY
        public static implicit operator QuantizationSettings(DracoExportSettings s) => new QuantizationSettings(
            s.positionQuantization,
            s.normalQuantization,
            s.texCoordQuantization,
            s.colorQuantization
        );

        public static implicit operator SpeedSettings(DracoExportSettings s) => new SpeedSettings(
            s.encodingSpeed,
            s.decodingSpeed
        );
#endif
    }
}
