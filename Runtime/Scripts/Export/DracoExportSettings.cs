// Copyright 2020-2023 Andreas Atteneder
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
    }
}