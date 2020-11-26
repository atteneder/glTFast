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

// Based on Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "glTF/Unlit" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _MainTexRotation ("Texture rotation", Vector) = (1,0,0,1)
    [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass {
        
        Cull [_CullMode]

        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
            #if UNITY_VERSION >= 201900
            #pragma shader_feature_local _UV_ROTATION
            #else
            #pragma shader_feature _UV_ROTATION
            #endif

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTexRotation;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);

#ifdef _UV_ROTATION
                // 2x2 matrix multiplication to apply rotation
                o.texcoord.x = v.texcoord.x * _MainTexRotation.x + v.texcoord.y * _MainTexRotation.z;
                o.texcoord.y = v.texcoord.x * _MainTexRotation.y + v.texcoord.y * _MainTexRotation.w;
                o.texcoord = TRANSFORM_TEX(o.texcoord, _MainTex);
#else
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
#endif

                UNITY_TRANSFER_FOG(o,o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                UNITY_OPAQUE_ALPHA(col.a);
                col *= i.color;
                return col;
            }
        ENDCG
    }
}

}
