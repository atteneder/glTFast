//UNITY_SHADER_NO_UPGRADE
#ifndef GLTF_HLSL_INCLUDE
#define GLTF_HLSL_INCLUDE

/// This is a replacement for HDRP's Emission Node that compiles/works on other targets as well
void glTFast_HDRP_GetEmissionHDRColor_float(float3 Color, float ExposureWeight, out float3 Output)
{
    float3 hdrColor = Color;

    #if SHADEROPTIONS_PRE_EXPOSITION // ExposureWeight Only in HDRP
    #ifdef SHADERGRAPH_PREVIEW
    float inverseExposureMultiplier = 1.0;
    #else
    float inverseExposureMultiplier = GetInverseCurrentExposureMultiplier();
    #endif
    // Inverse pre-expose using _EmissiveExposureWeight weight
    hdrColor = lerp(hdrColor * inverseExposureMultiplier, hdrColor, ExposureWeight);
    #endif

    Output = hdrColor;
}

#endif