// Copyright 2020 Andreas Atteneder
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

#if GLTFAST_RENDER_TEST

using UnityEngine.TestTools.Graphics;

public class UniversalGraphicsTestSettings : GraphicsTestSettings
{
    public int WaitFrames = 0;
    public bool XRCompatible = true;

    public UniversalGraphicsTestSettings()
    {
        ImageComparisonSettings.TargetWidth = 512;
        ImageComparisonSettings.TargetHeight = 512;
        ImageComparisonSettings.AverageCorrectnessThreshold = 0.005f;
        ImageComparisonSettings.PerPixelCorrectnessThreshold = 0.001f;
        ImageComparisonSettings.UseBackBuffer = false;
    }
}

#endif
