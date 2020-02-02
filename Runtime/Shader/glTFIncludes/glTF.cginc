#ifndef GLTF_INCLUDED
#define GLTF_INCLUDED

#include "glTFUnityStandardCore.cginc"

void fragDeferredFacing (
    VertexOutputDeferred i,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3          // RT3: emission (rgb), --unused-- (a)
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    ,out half4 outShadowMask : SV_Target4       // RT4: shadowmask (rgba)
#endif
    ,half facing : VFACE
)
{
#ifdef _TANGENT_TO_WORLD
    i.tangentToWorldAndPackedData[0].xyz *= facing;
    i.tangentToWorldAndPackedData[1].xyz *= facing;
#endif
    i.tangentToWorldAndPackedData[2].xyz *= facing;
    fragDeferred(
        i,
        outGBuffer0,
        outGBuffer1,
        outGBuffer2,
        outEmission
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
        ,outShadowMask
#endif
    );
}

#endif // GLTF_INCLUDED